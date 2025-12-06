using System.Linq.Expressions;
using System.Text.Json;
using System.Text.RegularExpressions;
using LinhGo.ERP.Domain.Audit.Entities;
using Microsoft.EntityFrameworkCore;
using LinhGo.ERP.Domain.Common;
using LinhGo.ERP.Domain.Companies.Entities;
using LinhGo.ERP.Domain.Users.Entities;
using LinhGo.ERP.Domain.Customers.Entities;
using LinhGo.ERP.Domain.Inventory.Entities;
using LinhGo.ERP.Domain.Orders.Entities;
using LinhGo.ERP.Infrastructure.Data.Audit;

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
    
    // Audit
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var isPostgreSQL = Database.IsNpgsql();
        
        UseSnakeCaseNamingConvention(modelBuilder);
        
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ErpDbContext).Assembly);

        // Global query filters for soft delete and configure concurrency token
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                // Configure soft delete query filter
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var filter = Expression.Lambda(
                    Expression.Equal(property, Expression.Constant(false)),
                    parameter
                );

                entityType.SetQueryFilter(filter);
                
                // Configure Version as concurrency token (database-specific)
                if (isPostgreSQL)
                {
                    // PostgreSQL: Use xmin system column (read-only, auto-updated by PostgreSQL)
                    modelBuilder.Entity(entityType.ClrType)
                        .Property<uint>(nameof(BaseEntity.Version))
                        .HasColumnName("xmin")
                        .HasColumnType("xid")
                        .ValueGeneratedOnAddOrUpdate()
                        .IsConcurrencyToken();
                }
                else
                {
                    // SQL Server: Use rowversion (timestamp)
                    modelBuilder.Entity(entityType.ClrType)
                        .Property<byte[]>(nameof(BaseEntity.Version))
                        .IsRowVersion()
                        .ValueGeneratedOnAddOrUpdate()
                        .IsConcurrencyToken();
                }
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Capture audit entries before saving
        var auditEntries = BeforeSaveChanges();
        
        // Update audit fields
        UpdateAuditFields();
        
        // Save changes to database
        var result = await base.SaveChangesAsync(cancellationToken);
        
        // Save audit logs after successful save
        await AfterSaveChanges(auditEntries, cancellationToken);
        
        return result;
    }

    private List<AuditEntry> BeforeSaveChanges()
    {
        ChangeTracker.DetectChanges();
        var auditEntries = new List<AuditEntry>();

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            // Skip audit log entries themselves and unchanged entities
            if (entry.Entity is AuditLog || entry.State == EntityState.Unchanged || entry.State == EntityState.Detached)
                continue;

            var auditEntry = new AuditEntry(entry)
            {
                EntityId = entry.Entity.Id.ToString(),
                // TODO: Get from current user context
                UserId = entry.Entity.CreatedBy,
                UserName = entry.Entity.CreatedBy,
                CompanyId = GetCompanyIdFromEntity(entry.Entity)
            };

            auditEntries.Add(auditEntry);

            foreach (var property in entry.Properties)
            {
                string propertyName = property.Metadata.Name;
                
                // Skip these properties from audit
                if (propertyName is nameof(BaseEntity.CreatedAt) 
                    or nameof(BaseEntity.UpdatedAt) 
                    or nameof(BaseEntity.CreatedBy) 
                    or nameof(BaseEntity.UpdatedBy)
                    or nameof(BaseEntity.DeletedBy)
                    or nameof(BaseEntity.DeletedAt)
                    or nameof(BaseEntity.IsDeleted)
                    or nameof(BaseEntity.Version))
                    continue;

                if (entry.State == EntityState.Added)
                {
                    auditEntry.Action = "Create";
                    auditEntry.NewValues[propertyName] = property.CurrentValue;
                    auditEntry.AffectedColumns.Add(propertyName);
                }
                else if (entry.State == EntityState.Modified)
                {
                    if (property.IsModified)
                    {
                        auditEntry.Action = entry.Entity.IsDeleted ? "Delete" : "Update";
                        auditEntry.OldValues[propertyName] = property.OriginalValue;
                        auditEntry.NewValues[propertyName] = property.CurrentValue;
                        auditEntry.AffectedColumns.Add(propertyName);
                    }
                }
                else if (entry.State == EntityState.Deleted)
                {
                    auditEntry.Action = "Delete";
                    auditEntry.OldValues[propertyName] = property.OriginalValue;
                    auditEntry.AffectedColumns.Add(propertyName);
                }
            }
        }

        return auditEntries;
    }

    private async Task AfterSaveChanges(List<AuditEntry> auditEntries, CancellationToken cancellationToken)
    {
        if (auditEntries.Count == 0)
            return;

        foreach (var auditEntry in auditEntries)
        {
            var auditLog = new AuditLog
            {
                EntityName = auditEntry.EntityName,
                EntityId = auditEntry.EntityId,
                Action = auditEntry.Action,
                Timestamp = DateTime.UtcNow,
                UserId = auditEntry.UserId,
                UserName = auditEntry.UserName,
                CompanyId = auditEntry.CompanyId,
                OldValues = auditEntry.OldValues.Count > 0 ? JsonSerializer.Serialize(auditEntry.OldValues) : null,
                NewValues = auditEntry.NewValues.Count > 0 ? JsonSerializer.Serialize(auditEntry.NewValues) : null,
                AffectedColumns = auditEntry.AffectedColumns.Count > 0 ? string.Join(", ", auditEntry.AffectedColumns) : null,
                PrimaryKey = auditEntry.EntityId,
                IpAddress = auditEntry.IpAddress,
                UserAgent = auditEntry.UserAgent
            };

            AuditLogs.Add(auditLog);
        }

        await base.SaveChangesAsync(cancellationToken);
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
                    // TODO: Set CreatedBy from current user context
                    // entry.Entity.CreatedBy = _currentUserService.UserId;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    // TODO: Set UpdatedBy from current user context
                    // entry.Entity.UpdatedBy = _currentUserService.UserId;
                    break;

                case EntityState.Deleted:
                    // Implement soft delete
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    // TODO: Set DeletedBy from current user context
                    // entry.Entity.DeletedBy = _currentUserService.UserId;
                    break;
            }
        }
    }

    private Guid? GetCompanyIdFromEntity(object entity)
    {
        // Try to get CompanyId property using reflection
        var companyIdProperty = entity.GetType().GetProperty("CompanyId");
        if (companyIdProperty != null)
        {
            var value = companyIdProperty.GetValue(entity);
            return value as Guid?;
        }
        return null;
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

