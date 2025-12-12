using LinhGo.SharedKernel.Cache;
using LinhGo.SharedKernel.Querier;

namespace LinhGo.ERP.Application.Common.Caching;

/// <summary>
/// Centralized cache key factory for all ERP entities
/// Provides type-safe cache key generation with consistent patterns
/// Best practice: Single source of truth for all cache keys
/// 
/// Organization Structure:
/// - Core Business Entities (Company, User, Customer)
/// - Product & Inventory (Product, Category, Warehouse, Inventory)
/// - Orders & Sales (Order, Invoice, Quote)
/// - System & Configuration (Settings, Permission, AuditLog)
/// - Generic Helpers (Custom methods for dynamic entities)
/// 
/// Pattern: {entity}:{operation}:{identifier}
/// Example: "company:id:550e8400-e29b-41d4-a716-446655440000"
/// 
/// Adding New Entities:
/// 1. Copy an existing entity class as template
/// 2. Update EntityName constant
/// 3. Add entity-specific methods if needed
/// 4. Group in appropriate region
/// </summary>
public static class CacheKeys
{
    #region Core Business Entities

    /// <summary>
    /// Cache keys for Company entity
    /// Common methods: ById, ByCode, All, Active, Pattern
    /// </summary>
    public static class Company
    {
        private const string EntityName = "company";

        public static string ById(Guid id) =>
            BuildKey(EntityName, "id", id);

        public static string ByCode(string code) =>
            BuildKey(EntityName, "code", code);

        public static string All() =>
            BuildKey(EntityName, "all");

        public static string Active() =>
            BuildKey(EntityName, "active");

        public static string Queries(QuerierParams queryParams) =>
            QuerierKeyGenerator.GenerateKey(EntityName, queryParams);

        public static string Pattern() =>
            BuildPattern(EntityName);

        public static string PatternById() =>
            BuildPattern(EntityName, "id");

        public static string PatternSearch() =>
            QuerierKeyGenerator.GeneratePattern(EntityName);
    }

    /// <summary>
    /// Cache keys for User entity
    /// Additional methods: ByEmail, ByUsername
    /// </summary>
    public static class User
    {
        private const string EntityName = "user";

        public static string ById(Guid id) =>
            BuildKey(EntityName, "id", id);

        public static string ByEmail(string email) =>
            BuildKey(EntityName, "email", email.ToLowerInvariant());

        public static string ByUsername(string username) =>
            BuildKey(EntityName, "username", username.ToLowerInvariant());

        public static string All() =>
            BuildKey(EntityName, "all");

        public static string Active() =>
            BuildKey(EntityName, "active");
        
        public static string Queries(QuerierParams queryParams) =>
            QuerierKeyGenerator.GenerateKey(EntityName, queryParams);

        public static string Pattern() =>
            BuildPattern(EntityName);
    }
    
    /// <summary>
    /// Cache keys for CompanyUser entity
    /// Additional methods: ByEmail, ByUsername
    /// </summary>
    public static class CompanyUser
    {
        private const string EntityName = "company-user";

        public static string ById(Guid id) =>
            BuildKey(EntityName, "id", id);

        public static string ByUserAndCompany(string userId, string companyId) =>
            BuildKey(EntityName, "user-company", $"{userId}:{companyId}");

        public static string ByCompany(string companyId) =>
            BuildKey(EntityName, "company", companyId);
        
        public static string ByUser(string userId) =>
            BuildKey(EntityName, "user", userId);

        public static string All() =>
            BuildKey(EntityName, "all");

        public static string Active() =>
            BuildKey(EntityName, "active");
        
        public static string Queries(QuerierParams queryParams) =>
            QuerierKeyGenerator.GenerateKey(EntityName, queryParams);

        public static string Pattern() =>
            BuildPattern(EntityName);
    }

    /// <summary>
    /// Cache keys for Customer entity
    /// Additional methods: ByCode, ByEmail, ByPhone
    /// </summary>
    public static class Customer
    {
        private const string EntityName = "customer";

        public static string ById(Guid id) =>
            BuildKey(EntityName, "id", id);

        public static string ByCode(string code) =>
            BuildKey(EntityName, "code", code);

        public static string ByEmail(string email) =>
            BuildKey(EntityName, "email", email.ToLowerInvariant());

        public static string ByPhone(string phone) =>
            BuildKey(EntityName, "phone", phone);

        public static string All() =>
            BuildKey(EntityName, "all");

        public static string Active() =>
            BuildKey(EntityName, "active");

        public static string Pattern() =>
            BuildPattern(EntityName);
    }

    #endregion

    #region Product & Inventory

    /// <summary>
    /// Cache keys for Product entity
    /// Additional methods: BySku, ByBarcode, ByCategory
    /// </summary>
    public static class Product
    {
        private const string EntityName = "product";

        public static string ById(Guid id) =>
            BuildKey(EntityName, "id", id);

        public static string BySku(string sku) =>
            BuildKey(EntityName, "sku", sku);

        public static string ByBarcode(string barcode) =>
            BuildKey(EntityName, "barcode", barcode);

        public static string ByCategory(Guid categoryId) =>
            BuildKey(EntityName, "category", categoryId);

        public static string All() =>
            BuildKey(EntityName, "all");

        public static string Active() =>
            BuildKey(EntityName, "active");

        public static string Pattern() =>
            BuildPattern(EntityName);
    }

    /// <summary>
    /// Cache keys for Category entity
    /// Additional methods: BySlug, Tree
    /// </summary>
    public static class Category
    {
        private const string EntityName = "category";

        public static string ById(Guid id) =>
            BuildKey(EntityName, "id", id);

        public static string BySlug(string slug) =>
            BuildKey(EntityName, "slug", slug.ToLowerInvariant());

        public static string All() =>
            BuildKey(EntityName, "all");

        public static string Tree() =>
            BuildKey(EntityName, "tree");

        public static string Pattern() =>
            BuildPattern(EntityName);
    }

    /// <summary>
    /// Cache keys for Warehouse entity
    /// </summary>
    public static class Warehouse
    {
        private const string EntityName = "warehouse";

        public static string ById(Guid id) =>
            BuildKey(EntityName, "id", id);

        public static string ByCode(string code) =>
            BuildKey(EntityName, "code", code);

        public static string All() =>
            BuildKey(EntityName, "all");

        public static string Active() =>
            BuildKey(EntityName, "active");

        public static string Pattern() =>
            BuildPattern(EntityName);
    }

    /// <summary>
    /// Cache keys for Inventory/Stock operations
    /// Complex keys for multi-entity relationships
    /// </summary>
    public static class Inventory
    {
        private const string EntityName = "inventory";

        public static string StockByProduct(Guid productId) =>
            BuildKey(EntityName, "stock:product", productId);

        public static string StockByWarehouse(Guid warehouseId) =>
            BuildKey(EntityName, "stock:warehouse", warehouseId);

        public static string StockByProductAndWarehouse(Guid productId, Guid warehouseId) =>
            BuildKey(EntityName, "stock", $"p:{productId}:w:{warehouseId}");

        public static string Pattern() =>
            BuildPattern(EntityName);
    }

    #endregion

    #region Orders & Sales

    /// <summary>
    /// Cache keys for Order entity
    /// Additional methods: ByNumber, ByCustomer, Pending, Completed
    /// </summary>
    public static class Order
    {
        private const string EntityName = "order";

        public static string ById(Guid id) =>
            BuildKey(EntityName, "id", id);

        public static string ByNumber(string orderNumber) =>
            BuildKey(EntityName, "number", orderNumber);

        public static string ByCustomer(Guid customerId) =>
            BuildKey(EntityName, "customer", customerId);

        public static string Pending() =>
            BuildKey(EntityName, "pending");

        public static string Completed() =>
            BuildKey(EntityName, "completed");

        public static string All() =>
            BuildKey(EntityName, "all");

        public static string Pattern() =>
            BuildPattern(EntityName);
    }

    #endregion

    #region System & Configuration

    /// <summary>
    /// Cache keys for Settings/Configuration
    /// </summary>
    public static class Settings
    {
        private const string EntityName = "settings";

        public static string ByCompany(Guid companyId) =>
            BuildKey(EntityName, "company", companyId);

        public static string ByKey(string key) =>
            BuildKey(EntityName, "key", key);

        public static string All() =>
            BuildKey(EntityName, "all");

        public static string Pattern() =>
            BuildPattern(EntityName);
    }

    /// <summary>
    /// Cache keys for Permission/Authorization
    /// </summary>
    public static class Permission
    {
        private const string EntityName = "permission";

        public static string ByUser(Guid userId) =>
            BuildKey(EntityName, "user", userId);

        public static string ByRole(Guid roleId) =>
            BuildKey(EntityName, "role", roleId);

        public static string All() =>
            BuildKey(EntityName, "all");

        public static string Pattern() =>
            BuildPattern(EntityName);
    }

    #endregion

    #region Generic Helpers

    /// <summary>
    /// Create a custom cache key for entities not predefined
    /// Use this for dynamic/new entities until added to CacheKeyFactory
    /// </summary>
    /// <example>
    /// var key = CacheKeyFactory.Custom("invoice", "id", invoiceId);
    /// </example>
    public static string Custom(string entity, string operation, string? identifier = null)
    {
        return identifier != null
            ? BuildKey(entity, operation, identifier)
            : BuildKey(entity, operation);
    }

    /// <summary>
    /// Create a custom cache key with GUID identifier
    /// </summary>
    /// <example>
    /// var key = CacheKeyFactory.Custom("invoice", "id", invoiceId);
    /// </example>
    public static string Custom(string entity, string operation, Guid id)
    {
        return BuildKey(entity, operation, id);
    }

    /// <summary>
    /// Create a pattern for custom entity
    /// </summary>
    /// <example>
    /// var pattern = CacheKeyFactory.CustomPattern("invoice");
    /// // Result: "invoice:*"
    /// </example>
    public static string CustomPattern(string entity)
    {
        return BuildPattern(entity);
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Build cache key with operation only (for lists)
    /// Pattern: {entity}:{operation}
    /// </summary>
    private static string BuildKey(string entity, string operation)
    {
        return CacheKeyBuilder.ForEntity(entity)
            .WithOperation(operation)
            .Build();
    }

    /// <summary>
    /// Build cache key with string identifier
    /// Pattern: {entity}:{operation}:{identifier}
    /// </summary>
    private static string BuildKey(string entity, string operation, string identifier)
    {
        return CacheKeyBuilder.ForEntity(entity)
            .WithOperation(operation)
            .WithIdentifier(identifier)
            .Build();
    }

    /// <summary>
    /// Build cache key with GUID identifier
    /// Pattern: {entity}:{operation}:{guid}
    /// </summary>
    private static string BuildKey(string entity, string operation, Guid id)
    {
        return CacheKeyBuilder.ForEntity(entity)
            .WithOperation(operation)
            .WithId(id)
            .Build();
    }

    /// <summary>
    /// Build pattern for entity (all keys)
    /// Pattern: {entity}:*
    /// </summary>
    private static string BuildPattern(string entity)
    {
        return CacheKeyBuilder.ForEntity(entity).BuildPattern();
    }

    /// <summary>
    /// Build pattern for entity with operation
    /// Pattern: {entity}:{operation}:*
    /// </summary>
    private static string BuildPattern(string entity, string operation)
    {
        return CacheKeyBuilder.ForEntity(entity)
            .WithOperation(operation)
            .BuildPattern();
    }

    #endregion
}

