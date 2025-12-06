# Concurrency Control Fix Summary

## Problem
You encountered the error: **"null value in column 'code' of relation 'companies' violates not-null constraint"**

This happened because the update logic was creating a new entity from the DTO, which didn't include the `Code` field (it's excluded from UpdateCompanyDto). When trying to update, it would overwrite the Code with null.

## Root Cause
1. The `UpdateCompanyDto` doesn't include `Code` (intentionally, as it shouldn't be changeable)
2. Mapping `UpdateCompanyDto` to a new `Company` entity left `Code` as null
3. When updating, this null value violated the database NOT NULL constraint

## Solution

### 1. Fixed `GenericRepository.UpdateAsync`
**File**: `/LinhGo.ERP.Infrastructure/Repositories/GenericRepository.cs`

```csharp
public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
{
    // Attach the entity if it's not tracked
    var entry = Context.Entry(entity);
    if (entry.State == EntityState.Detached)
    {
        DbSet.Attach(entity);
        entry.State = EntityState.Modified;
    }
    
    // For concurrency check: Set the OriginalValue of Version to what client sent
    // When SaveChanges executes, EF Core will compare this with the database value
    // If they don't match, DbUpdateConcurrencyException is thrown
    entry.Property(nameof(BaseEntity.Version)).OriginalValue = entity.Version;
    
    await Context.SaveChangesAsync(cancellationToken);
}
```

**Key Points**:
- The entity passed in must already have all required fields populated
- Sets `OriginalValue` for Version to enable concurrency checking
- EF Core compares `OriginalValue` with database value on save

### 2. Fixed `CompanyService.UpdateAsync`
**File**: `/LinhGo.ERP.Application/Services/CompanyService.cs`

```csharp
public async Task<Result<CompanyDto>> UpdateAsync(Guid id, UpdateCompanyDto dto)
{
    try
    {
        if (id != dto.Id)
        {
            logger.LogWarning("Mismatched company ID in update request: {RouteId} vs {DtoId}", id, dto.Id);
            return Error.WithValidationCode(CompanyErrors.IdMismatch, id, dto.Id);
        }
        
        // Fetch existing entity from database
        var existing = await companyRepository.GetByIdAsync(dto.Id);
        if (existing == null)
        {
            logger.LogWarning("Attempt to update non-existent company with ID {CompanyId}", dto.Id);
            return Error.WithNotFoundCode(CompanyErrors.NotFound, dto.Id);
        }

        // Map DTO to existing entity (preserves Code and other unmapped fields)
        // Version from DTO will be used for concurrency check
        mapper.Map(dto, existing);
        
        // Update entity - will throw DbUpdateConcurrencyException if Version doesn't match database
        await companyRepository.UpdateAsync(existing);
        
        var result = mapper.Map<CompanyDto>(existing);
        return result;
    }
    catch (DbUpdateConcurrencyException ex)
    {
        logger.LogWarning(ex, "Concurrency conflict updating company with ID {CompanyId}", dto.Id);
        return Error.WithConflictCode(CompanyErrors.ConcurrencyConflict);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error attempting to update company with ID {CompanyId}", dto.Id);
        return Error.WithFailureCode(CompanyErrors.UpdateFailed);
    }
}
```

**Key Changes**:
1. **Fetch existing entity first** - Gets the complete entity with all fields from database
2. **Map DTO onto existing entity** - Only updates fields present in DTO, preserves others (like Code)
3. **Pass tracked entity to repository** - Repository can now properly check concurrency

### 3. Updated `BaseEntity`
**File**: `/LinhGo.ERP.Domain/Common/BaseEntity.cs`

```csharp
/// <summary>
/// Concurrency token for optimistic locking.
/// Maps to xmin (PostgreSQL) or rowversion (SQL Server).
/// Automatically managed by database to prevent concurrent update conflicts.
/// </summary>
public uint Version { get; set; }
```

**Changes**:
- Removed `[Timestamp]` attribute (causes issues with PostgreSQL)
- Clean property definition, configuration is in DbContext

### 4. Updated `ErpDbContext`
**File**: `/LinhGo.ERP.Infrastructure/Data/ErpDbContext.cs`

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    var isPostgreSQL = Database.IsNpgsql();
    
    // ...existing code...
    
    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    {
        if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
        {
            // ...soft delete filter...
            
            // Configure Version as concurrency token (database-specific)
            if (isPostgreSQL)
            {
                // PostgreSQL: Use xmin system column
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
                    .IsConcurrencyToken();
            }
        }
    }
}
```

**Key Points**:
- Database-agnostic approach
- PostgreSQL uses `xmin` (automatic transaction ID)
- SQL Server uses `rowversion` (automatic timestamp)
- Both provide optimistic concurrency control

## How It Works Now

### Update Flow:
1. **Client sends update request** with `Version` from when they fetched the data
   ```json
   {
     "id": "123",
     "name": "New Name",
     "version": 5
   }
   ```

2. **Service fetches existing entity** from database
   - Gets complete entity including `Code`, `CreatedAt`, current `Version`, etc.
   - If database Version is now `6`, entity has `Version: 6`

3. **Service maps DTO to existing entity**
   - Updates `Name` to "New Name"
   - Preserves `Code` (not in DTO)
   - Overwrites `Version` to `5` (from DTO)

4. **Repository sets OriginalValue**
   - `entry.Property("Version").OriginalValue = 5` (client's version)
   - `entry.Property("Version").CurrentValue = 5` (also client's version)

5. **SaveChanges compares versions**
   - Database has `Version = 6`
   - OriginalValue is `5`
   - **Mismatch detected!** → Throws `DbUpdateConcurrencyException`

6. **Service catches exception**
   - Returns 409 Conflict error to client
   - Client must reload and retry

### If No Concurrent Update:
- Database has `Version = 5`
- OriginalValue is `5`
- **Match!** → Update succeeds
- Database automatically increments to `Version = 6`

## Benefits

✅ **Prevents data loss** from concurrent updates
✅ **Preserves required fields** like `Code` that aren't in update DTOs
✅ **Database-agnostic** solution (PostgreSQL & SQL Server)
✅ **Automatic versioning** by database (no manual increment needed)
✅ **Clear error handling** with proper HTTP status codes

## Testing

To test concurrency:

```bash
# Terminal 1: Get company
GET /api/companies/123
Response: { "id": "123", "code": "COMP001", "name": "Company A", "version": 5 }

# Terminal 2: Update (succeeds)
PUT /api/companies/123
Body: { "id": "123", "name": "Company B", "version": 5 }
Response: 200 OK, { "version": 6 }

# Terminal 1: Try update with old version (fails)
PUT /api/companies/123
Body: { "id": "123", "name": "Company C", "version": 5 }
Response: 409 Conflict - "COMPANY_CONCURRENCY_CONFLICT"
```

## Files Changed

1. ✅ `LinhGo.ERP.Domain/Common/BaseEntity.cs` - Cleaned up Version property
2. ✅ `LinhGo.ERP.Infrastructure/Data/ErpDbContext.cs` - Added database-specific concurrency configuration
3. ✅ `LinhGo.ERP.Infrastructure/Repositories/GenericRepository.cs` - Fixed UpdateAsync to handle concurrency
4. ✅ `LinhGo.ERP.Application/Services/CompanyService.cs` - Fixed update flow to preserve all entity fields

## Next Steps

1. **Create new migration** to apply the xmin configuration:
   ```bash
   cd LinhGo.ERP.Infrastructure
   dotnet ef migrations add ImplementOptimisticConcurrency --startup-project ../LinhGo.ERP.Api
   dotnet ef database update --startup-project ../LinhGo.ERP.Api
   ```

2. **Test the implementation** with concurrent updates

3. **Update API documentation** to mention Version field requirement in update requests

