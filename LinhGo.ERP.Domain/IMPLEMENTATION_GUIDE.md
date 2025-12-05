# Implementation Guide - Infrastructure Layer

This guide shows how to implement the Domain layer in your Infrastructure project.

## 1. Database Context with Entity Framework Core

```csharp
// File: LinhGo.ERP.Infrastructure/Data/ErpDbContext.cs

using Microsoft.EntityFrameworkCore;
using LinhGo.ERP.Domain.Common;
using LinhGo.ERP.Domain.Companies.Entities;
using LinhGo.ERP.Domain.Users.Entities;
using LinhGo.ERP.Domain.Customers.Entities;
using LinhGo.ERP.Domain.Inventory.Entities;
using LinhGo.ERP.Domain.Orders.Entities;

namespace LinhGo.ERP.Infrastructure.Data;

public class ErpDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public ErpDbContext(DbContextOptions<ErpDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    // Companies
    public DbSet<Company> Companies { get; set; }
    public DbSet<CompanySettings> CompanySettings { get; set; }

    // Users
    public DbSet<User> Users { get; set; }
    public DbSet<UserCompany> UserCompanies { get; set; }
    public DbSet<UserPermission> UserPermissions { get; set; }

    // Customers
    public DbSet<Customer> Customers { get; set; }
    public DbSet<CustomerContact> CustomerContacts { get; set; }
    public DbSet<CustomerAddress> CustomerAddresses { get; set; }

    // Inventory
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<Stock> Stocks { get; set; }
    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<InventoryTransaction> InventoryTransactions { get; set; }

    // Orders
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<OrderPayment> OrderPayments { get; set; }
    public DbSet<OrderShipment> OrderShipments { get; set; }
    public DbSet<OrderShipmentItem> OrderShipmentItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ErpDbContext).Assembly);

        // Global query filters for tenant isolation
        ConfigureTenantFilters(modelBuilder);
    }

    private void ConfigureTenantFilters(ModelBuilder modelBuilder)
    {
        // Apply global query filter for all tenant entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(ITenantEntity.CompanyId));
                var companyId = Expression.Property(
                    Expression.Constant(_tenantContext),
                    nameof(ITenantContext.CurrentCompanyId)
                );

                var filter = Expression.Lambda(
                    Expression.Equal(property, companyId),
                    parameter
                );

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }

        // Soft delete filter
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var filter = Expression.Lambda(
                    Expression.Equal(property, Expression.Constant(false)),
                    parameter
                );

                var existingFilter = entityType.GetQueryFilter();
                if (existingFilter != null)
                {
                    var combined = Expression.AndAlso(existingFilter.Body, filter.Body);
                    filter = Expression.Lambda(combined, parameter);
                }

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    // Set CreatedBy from current user context
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    // Set UpdatedBy from current user context
                    break;

                case EntityState.Deleted:
                    // Implement soft delete
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    // Set DeletedBy from current user context
                    break;
            }
        }
    }
}
```

## 2. Entity Configurations

```csharp
// File: LinhGo.ERP.Infrastructure/Data/Configurations/CompanyConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LinhGo.ERP.Domain.Companies.Entities;

namespace LinhGo.ERP.Infrastructure.Data.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(c => c.Code)
            .IsUnique();

        builder.Property(c => c.Currency)
            .HasMaxLength(3);

        // Configure relationships
        builder.HasMany(c => c.Settings)
            .WithOne(s => s.Company)
            .HasForeignKey(s => s.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

// File: LinhGo.ERP.Infrastructure/Data/Configurations/CustomerConfiguration.cs

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        // Tenant isolation
        builder.HasIndex(c => c.CompanyId);
        builder.HasIndex(c => new { c.CompanyId, c.Code }).IsUnique();

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        // Relationships
        builder.HasMany(c => c.Contacts)
            .WithOne(cc => cc.Customer)
            .HasForeignKey(cc => cc.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Addresses)
            .WithOne(ca => ca.Customer)
            .HasForeignKey(ca => ca.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

// Similar configurations for all other entities...
```

## 3. Repository Implementation

```csharp
// File: LinhGo.ERP.Infrastructure/Repositories/GenericRepository.cs

using Microsoft.EntityFrameworkCore;
using LinhGo.ERP.Domain.Common;
using LinhGo.ERP.Domain.Common.Interfaces;
using LinhGo.ERP.Infrastructure.Data;

namespace LinhGo.ERP.Infrastructure.Repositories;

public class GenericRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly ErpDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(ErpDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            _dbSet.Remove(entity); // Soft delete via SaveChanges override
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(e => e.Id == id, cancellationToken);
    }
}

// File: LinhGo.ERP.Infrastructure/Repositories/TenantRepository.cs

public class TenantRepository<T> : ITenantRepository<T> where T : TenantEntity
{
    protected readonly ErpDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public TenantRepository(ErpDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(
            e => e.CompanyId == companyId && e.Id == id,
            cancellationToken
        );
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(e => e.CompanyId == companyId).ToListAsync(cancellationToken);
    }

    public virtual async Task<T> AddAsync(Guid companyId, T entity, CancellationToken cancellationToken = default)
    {
        entity.CompanyId = companyId;
        await _dbSet.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public virtual async Task UpdateAsync(Guid companyId, T entity, CancellationToken cancellationToken = default)
    {
        if (entity.CompanyId != companyId)
            throw new UnauthorizedAccessException("Cannot update entity from different company");

        _dbSet.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task DeleteAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(companyId, id, cancellationToken);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public virtual async Task<bool> ExistsAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(e => e.CompanyId == companyId && e.Id == id, cancellationToken);
    }
}
```

## 4. Specific Repository Implementations

```csharp
// File: LinhGo.ERP.Infrastructure/Repositories/CustomerRepository.cs

using Microsoft.EntityFrameworkCore;
using LinhGo.ERP.Domain.Customers.Entities;
using LinhGo.ERP.Domain.Customers.Interfaces;
using LinhGo.ERP.Infrastructure.Data;

namespace LinhGo.ERP.Infrastructure.Repositories;

public class CustomerRepository : TenantRepository<Customer>, ICustomerRepository
{
    public CustomerRepository(ErpDbContext context) : base(context) { }

    public async Task<Customer?> GetByCodeAsync(Guid companyId, string code, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Contacts)
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.CompanyId == companyId && c.Code == code, cancellationToken);
    }

    public async Task<IEnumerable<Customer>> GetActiveCustomersAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.CompanyId == companyId && c.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Customer>> SearchCustomersAsync(Guid companyId, string searchTerm, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.CompanyId == companyId &&
                (c.Name.Contains(searchTerm) ||
                 c.Code.Contains(searchTerm) ||
                 c.Email != null && c.Email.Contains(searchTerm)))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsCodeUniqueAsync(Guid companyId, string code, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(c => c.CompanyId == companyId && c.Code == code);

        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);

        return !await query.AnyAsync(cancellationToken);
    }
}

// Similar implementations for Product, Order, etc.
```

## 5. Tenant Context Implementation

```csharp
// File: LinhGo.ERP.Infrastructure/Services/TenantContext.cs

using LinhGo.ERP.Domain.Common;

namespace LinhGo.ERP.Infrastructure.Services;

public class TenantContext : ITenantContext
{
    private Guid? _currentCompanyId;

    public Guid? CurrentCompanyId => _currentCompanyId;

    public void SetCompanyId(Guid companyId)
    {
        _currentCompanyId = companyId;
    }
}
```

## 6. Dependency Injection Setup

```csharp
// File: LinhGo.ERP.Infrastructure/DependencyInjection.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LinhGo.ERP.Domain.Common;
using LinhGo.ERP.Domain.Common.Interfaces;
using LinhGo.ERP.Domain.Companies.Interfaces;
using LinhGo.ERP.Domain.Customers.Interfaces;
using LinhGo.ERP.Domain.Inventory.Interfaces;
using LinhGo.ERP.Domain.Orders.Interfaces;
using LinhGo.ERP.Infrastructure.Data;
using LinhGo.ERP.Infrastructure.Repositories;
using LinhGo.ERP.Infrastructure.Services;

namespace LinhGo.ERP.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ErpDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ErpDbContext).Assembly.FullName)
            )
        );

        // Tenant Context (Scoped - per request)
        services.AddScoped<ITenantContext, TenantContext>();

        // Repositories
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IStockRepository, StockRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        // Add more repositories...

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
```

## 7. Migrations

```bash
# In Package Manager Console or CLI

# Initial migration
dotnet ef migrations add InitialCreate --project LinhGo.ERP.Infrastructure --startup-project LinhGo.ERP.Web

# Update database
dotnet ef database update --project LinhGo.ERP.Infrastructure --startup-project LinhGo.ERP.Web
```

## 8. Usage in Application Layer

```csharp
// File: LinhGo.ERP.Application/Services/CustomerService.cs

using LinhGo.ERP.Domain.Customers.Entities;
using LinhGo.ERP.Domain.Customers.Interfaces;

namespace LinhGo.ERP.Application.Services;

public class CustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Customer> CreateCustomerAsync(Guid companyId, Customer customer)
    {
        // Validate unique code
        var isUnique = await _customerRepository.IsCodeUniqueAsync(companyId, customer.Code);
        if (!isUnique)
            throw new InvalidOperationException($"Customer code '{customer.Code}' already exists");

        return await _customerRepository.AddAsync(companyId, customer);
    }

    public async Task<IEnumerable<Customer>> GetActiveCustomersAsync(Guid companyId)
    {
        return await _customerRepository.GetActiveCustomersAsync(companyId);
    }
}
```

This implementation provides:
- ✅ Complete tenant isolation
- ✅ Soft delete functionality
- ✅ Audit trail (Created/Updated/Deleted timestamps)
- ✅ Global query filters
- ✅ Repository pattern
- ✅ Unit of Work pattern
- ✅ Clean separation of concerns

