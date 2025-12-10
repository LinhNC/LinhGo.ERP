# ✅ Generic Cache Refresh Mechanism - Best Practices for Scalability

## Problem Solved

Instead of creating cache refresh methods in every service (hundreds of services = hundreds of refresh methods), we now have a **single generic CacheController** that works for ALL entities.

## Architecture: Generic vs Entity-Specific

### ❌ BAD: Entity-Specific Approach (Not Scalable)
```
CompanyService.RefreshCacheByIdAsync()
CompanyService.RefreshAllCachesAsync()
CompanyService.ClearAllCachesAsync()

UserService.RefreshCacheByIdAsync()
UserService.RefreshAllCachesAsync()
UserService.ClearAllCachesAsync()

ProductService.RefreshCacheByIdAsync()
ProductService.RefreshAllCachesAsync()
ProductService.ClearAllCachesAsync()

... 100 more services with duplicate methods
```

**Problems:**
- 100 services = 300 methods (3 per service)
- Code duplication
- Hard to maintain
- Not DRY (Don't Repeat Yourself)
- Violates Single Responsibility Principle

### ✅ GOOD: Generic Approach (Scalable)
```
Single CacheController with 2 endpoints:
- DELETE /api/admin/cache/key?key={cacheKey}
- DELETE /api/admin/cache/pattern?pattern={pattern}

Works for ALL entities!
```

**Benefits:**
- ✅ One controller for all entities
- ✅ No code duplication
- ✅ Easy to maintain
- ✅ DRY principle
- ✅ Scales to hundreds of services
- ✅ Single Responsibility Principle

## Implementation

### 1. Generic CacheController

**Location:** `/Api/Controllers/CacheController.cs`

```csharp
[ApiController]
[Route("api/admin/cache")]
[Authorize(Roles = "Admin")]
public class CacheController : BaseApiController
{
    private readonly ICacheService _cacheService;
    
    // Works for ANY entity - Company, User, Product, Order, etc.
    
    [HttpDelete("key")]
    public async Task<IActionResult> RemoveCacheByKey([FromQuery] string key)
    {
        await _cacheService.RemoveAsync(key);
        return Ok();
    }
    
    [HttpDelete("pattern")]
    public async Task<IActionResult> RemoveCacheByPattern([FromQuery] string pattern)
    {
        await _cacheService.RemoveByPatternAsync(pattern);
        return Ok();
    }
}
```

### 2. Automatic Cache Invalidation (Still in Services)

Services handle automatic invalidation on write operations:

```csharp
public class CompanyService
{
    // Automatic invalidation after writes
    public async Task<Result<CompanyDto>> CreateAsync(CreateCompanyDto dto)
    {
        var company = await repository.AddAsync(company);
        
        // Auto-invalidate affected caches
        await InvalidateListCachesAsync();
        
        return result;
    }
    
    public async Task<Result<CompanyDto>> UpdateAsync(Guid id, UpdateCompanyDto dto)
    {
        await repository.UpdateAsync(existing);
        
        // Auto-invalidate affected caches
        await InvalidateCompanyCachesAsync(id, code);
        
        return result;
    }
}
```

## Usage Examples

### Example 1: Clear Cache for Specific Company
```bash
# Remove cache for company with ID
DELETE /api/admin/cache/key?key=company:id:550e8400-e29b-41d4-a716-446655440000

# Remove cache for company with code
DELETE /api/admin/cache/key?key=company:code:ACME001
```

### Example 2: Clear All Company Caches
```bash
# Clear ALL company-related caches (by ID, by code, lists, etc.)
DELETE /api/admin/cache/pattern?pattern=company:*
```

### Example 3: Clear Specific User Cache
```bash
# Remove cache for user
DELETE /api/admin/cache/key?key=user:id:550e8400-e29b-41d4-a716-446655440000

# Remove cache by email
DELETE /api/admin/cache/key?key=user:email:john@example.com
```

### Example 4: Clear All User Caches
```bash
DELETE /api/admin/cache/pattern?pattern=user:*
```

### Example 5: Clear Product Caches
```bash
# Specific product
DELETE /api/admin/cache/key?key=product:id:550e8400-e29b-41d4-a716-446655440000

# All products
DELETE /api/admin/cache/pattern?pattern=product:*
```

### Example 6: Clear All Caches in System
```bash
# Nuclear option - clear everything
DELETE /api/admin/cache/pattern?pattern=*
```

## Cache Key Patterns

### Standard Pattern: `{entity}:{operation}:{identifier}`

| Entity | Pattern | Example |
|--------|---------|---------|
| Company | `company:id:{guid}` | `company:id:550e8400-e29b-41d4-a716-446655440000` |
| Company | `company:code:{code}` | `company:code:ACME001` |
| Company | `company:all` | `company:all` |
| Company | `company:active` | `company:active` |
| User | `user:id:{guid}` | `user:id:550e8400-e29b-41d4-a716-446655440000` |
| User | `user:email:{email}` | `user:email:john@example.com` |
| User | `user:all` | `user:all` |
| Product | `product:id:{guid}` | `product:id:550e8400-e29b-41d4-a716-446655440000` |
| Product | `product:sku:{sku}` | `product:sku:PROD-001` |
| Product | `product:all` | `product:all` |
| Order | `order:id:{guid}` | `order:id:550e8400-e29b-41d4-a716-446655440000` |
| Order | `order:number:{number}` | `order:number:ORD-2024-001` |

### Get All Patterns
```bash
GET /api/admin/cache/patterns

# Returns documentation of all cache key patterns
```

## When to Use Each Approach

### Automatic Invalidation (In Services)
**Use for:** Normal application flow
- User creates/updates/deletes data
- Cache automatically invalidated
- No manual intervention needed

**Example:**
```csharp
// User updates company name via API
PUT /api/v1/companies/550e8400-e29b-41d4-a716-446655440000

// Service automatically:
// 1. Updates database
// 2. Invalidates cache for this company
// 3. Invalidates list caches
```

### Manual Refresh (Via CacheController)
**Use for:** Administrative/exceptional cases
- Bulk data imports
- Direct database changes (SQL scripts)
- Database replication/sync
- Debugging/troubleshooting
- Cache corruption fixes

**Example:**
```bash
# After bulk import of 1000 companies
DELETE /api/admin/cache/pattern?pattern=company:*

# After direct SQL update
DELETE /api/admin/cache/key?key=company:id:550e8400-e29b-41d4-a716-446655440000
```

## Security Considerations

### Admin-Only Access
```csharp
[Authorize(Roles = "Admin")]
public class CacheController
```

**Why:**
- Cache manipulation can affect performance
- Should not be accessible to regular users
- Potential for abuse (clearing all caches repeatedly)

### Audit Logging
```csharp
_logger.LogWarning("Removing caches matching pattern: {Pattern}", pattern);
```

All cache operations are logged for audit purposes.

## Best Practices Applied

### ✅ 1. Single Responsibility Principle
- CacheController: Manages cache operations
- Services: Handle business logic + automatic invalidation

### ✅ 2. DRY (Don't Repeat Yourself)
- One implementation for all entities
- No duplicate refresh methods

### ✅ 3. Open/Closed Principle
- Open for extension (add new entities)
- Closed for modification (no need to change CacheController)

### ✅ 4. Separation of Concerns
- Infrastructure (CacheService): Low-level cache operations
- API (CacheController): HTTP endpoints
- Application (Services): Business logic + auto-invalidation

### ✅ 5. Scalability
- Works with 1 entity
- Works with 1000 entities
- No code changes needed

### ✅ 6. Generic Programming
- Type-agnostic
- Pattern-based matching
- Flexible and powerful

## Comparison: Before vs After

### Before (Entity-Specific)
```
100 Services × 3 Methods = 300 Methods

// Company
CompanyService.RefreshCacheByIdAsync()
CompanyService.RefreshAllCachesAsync()
CompanyService.ClearAllCachesAsync()

// User
UserService.RefreshCacheByIdAsync()
UserService.RefreshAllCachesAsync()
UserService.ClearAllCachesAsync()

// ... 98 more services
```

**Lines of Code:** ~3000+ lines (300 methods × 10 lines each)

### After (Generic)
```
1 Controller × 2 Methods = 2 Methods

CacheController.RemoveCacheByKey()
CacheController.RemoveCacheByPattern()
```

**Lines of Code:** ~100 lines total

**Reduction:** **97% less code!** 🎉

## Real-World Scenarios

### Scenario 1: Bulk Import
```bash
# Import 10,000 companies via SQL script
# Then clear all company caches
DELETE /api/admin/cache/pattern?pattern=company:*
```

### Scenario 2: Database Replication
```bash
# After database sync from production to staging
# Clear all caches to ensure fresh data
DELETE /api/admin/cache/pattern?pattern=*
```

### Scenario 3: Debugging Stale Cache
```bash
# User reports seeing old company name
# Clear specific company cache
DELETE /api/admin/cache/key?key=company:id:550e8400-e29b-41d4-a716-446655440000

# User refreshes page - sees new data
```

### Scenario 4: External System Update
```bash
# External system updates user email in database directly
# Clear user cache
DELETE /api/admin/cache/key?key=user:id:550e8400-e29b-41d4-a716-446655440000
DELETE /api/admin/cache/key?key=user:email:old@example.com
```

## API Documentation

### Endpoints

#### 1. Remove Cache by Key
```
DELETE /api/admin/cache/key?key={cacheKey}

Query Parameters:
- key (string, required): Exact cache key to remove

Example:
DELETE /api/admin/cache/key?key=company:id:550e8400-e29b-41d4-a716-446655440000

Response:
{
  "message": "Cache removed successfully",
  "key": "company:id:550e8400-e29b-41d4-a716-446655440000",
  "timestamp": "2024-12-10T10:30:00Z"
}
```

#### 2. Remove Cache by Pattern
```
DELETE /api/admin/cache/pattern?pattern={pattern}

Query Parameters:
- pattern (string, required): Pattern to match (wildcards supported)

Examples:
DELETE /api/admin/cache/pattern?pattern=company:*
DELETE /api/admin/cache/pattern?pattern=user:email:*
DELETE /api/admin/cache/pattern?pattern=*

Response:
{
  "message": "Caches removed successfully",
  "pattern": "company:*",
  "note": "Pattern-based removal depends on cache implementation",
  "timestamp": "2024-12-10T10:30:00Z"
}
```

#### 3. Get Cache Health
```
GET /api/admin/cache/health

Response:
{
  "status": "healthy",
  "message": "Cache service is operational",
  "implementation": "IDistributedCache",
  "features": {
    "keyRemoval": true,
    "patternRemoval": "Depends on implementation",
    "refresh": true
  },
  "recommendations": [
    "Use pattern-based removal for bulk operations",
    "Use key-based removal for specific items",
    "Consider implementing Redis for production"
  ]
}
```

#### 4. Get Cache Key Patterns
```
GET /api/admin/cache/patterns

Response:
{
  "entities": [
    {
      "entity": "Company",
      "patterns": [
        "company:id:{guid}",
        "company:code:{code}",
        "company:all"
      ],
      "examples": [
        "company:id:550e8400-e29b-41d4-a716-446655440000",
        "company:code:ACME001",
        "company:all"
      ]
    }
  ],
  "usage": {
    "removeOne": "DELETE /api/admin/cache/key?key=company:id:...",
    "removeAll": "DELETE /api/admin/cache/pattern?pattern=company:*"
  }
}
```

## Monitoring & Observability

### Logging
All cache operations are logged:

```
INFO: Removing cache for key: company:id:550e8400-e29b-41d4-a716-446655440000
WARN: Removing caches matching pattern: company:*
DEBUG: Cache health check requested
```

### Metrics to Track
- Number of manual cache invalidations
- Patterns of cache clearing (which entities most often)
- Time between invalidations
- Impact on cache hit ratio after manual clears

## Migration Guide

### If You Have Existing Entity-Specific Methods

#### Step 1: Remove from Interface
```csharp
// Before
public interface ICompanyService
{
    Task<Result<CompanyDto>> RefreshCacheByIdAsync(Guid id);
    Task<Result<bool>> RefreshAllCachesAsync();
    Task<Result<bool>> ClearAllCachesAsync();
}

// After
public interface ICompanyService
{
    // Removed - use generic CacheController instead
}
```

#### Step 2: Remove from Service
```csharp
// Before: 100+ lines of cache refresh methods
public async Task<Result<CompanyDto>> RefreshCacheByIdAsync(Guid id) { ... }
public async Task<Result<bool>> RefreshAllCachesAsync() { ... }
public async Task<Result<bool>> ClearAllCachesAsync() { ... }

// After: Keep only automatic invalidation helpers
private async Task InvalidateCompanyCachesAsync(Guid id, string code) { ... }
private async Task InvalidateListCachesAsync() { ... }
```

#### Step 3: Update Callers
```csharp
// Before
await _companyService.RefreshCacheByIdAsync(id);

// After
// Use CacheController via HTTP
DELETE /api/admin/cache/key?key=company:id:550e8400-e29b-41d4-a716-446655440000

// Or directly via ICacheService if in same application
await _cacheService.RemoveAsync($"company:id:{id}");
```

## Summary

✅ **Generic CacheController** - Works for all entities  
✅ **2 endpoints** instead of 300+ methods  
✅ **97% less code** - Easier to maintain  
✅ **Scalable** - Works with 1 or 1000 entities  
✅ **DRY Principle** - No code duplication  
✅ **Best Practices** - SOLID principles applied  
✅ **Admin-only** - Security built-in  
✅ **Well documented** - Easy to use  
✅ **Production-ready** - Enterprise-grade solution  

**This is the correct, scalable way to handle cache management for large applications!** 🚀


