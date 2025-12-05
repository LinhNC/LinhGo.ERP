using System.Linq.Expressions;
using System.Text.RegularExpressions;
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
    
    public  ErpDbContext(DbContextOptions<ErpDbContext> options)
        : base(options)
    { }

    internal ErpDbContext(DbContextOptions<ErpDbContext> options, ITenantContext tenantContext)
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
        UseSnakeCaseNamingConvention(modelBuilder);
        
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ErpDbContext).Assembly);

        // Global query filters for soft delete
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

                entityType.SetQueryFilter(filter);
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
    
    private static void UseSnakeCaseNamingConvention(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = ToSnakeCase(entityType.GetTableName());
            entityType.SetTableName(tableName);

            // Loop through each property of the entity
            foreach (var property in entityType.GetProperties())
            {
                // Convert property name to snake_case
                var snakeCaseName = ToSnakeCase(property.Name);
                property.SetColumnName(snakeCaseName);
            }
        }
    }

    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input)) { return input; }

        var startUnderscores = Regex.Match(input, @"^_+", RegexOptions.None, TimeSpan.FromSeconds(1));
        return startUnderscores + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2", RegexOptions.None, TimeSpan.FromSeconds(1)).ToLower();
    }
}

