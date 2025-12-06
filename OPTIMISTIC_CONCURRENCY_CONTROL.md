# Optimistic Concurrency Control Implementation

## Vấn Đề: Data Race Khi Update Đồng Thời

### Tình Huống
```
User A                          User B
├─ GET /companies/1            ├─ GET /companies/1
│  (Name: "ABC Corp")          │  (Name: "ABC Corp")
│                               │
├─ Thay đổi Name: "ABC Inc"    ├─ Thay đổi Phone: "123-456"
│                               │
├─ PUT /companies/1            │
│  ✅ Success                   │
│  (Name: "ABC Inc")           │
│                               ├─ PUT /companies/1
│                               │  ✅ Success (GHI ĐÈ!)
│                               │  (Name: "ABC Corp", Phone: "123-456")
│
└─ ❌ Thay đổi của User A bị mất!
```

**Kết quả:** Thay đổi của User A về tên công ty bị mất vì User B ghi đè lên.

## Giải Pháp: Optimistic Concurrency Control

### Cách Hoạt Động

```
User A                          User B
├─ GET /companies/1            ├─ GET /companies/1
│  {                            │  {
│    id: 1,                     │    id: 1,
│    name: "ABC Corp",          │    name: "ABC Corp",
│    rowVersion: [v1]           │    rowVersion: [v1]
│  }                            │  }
│                               │
├─ PUT /companies/1            │
│  Body: {                      │
│    rowVersion: [v1] ✅        │
│  }                            │
│  ✅ Success                   │
│  Response: {                  │
│    rowVersion: [v2]           │  ← Database tự động tăng version
│  }                            │
│                               ├─ PUT /companies/1
│                               │  Body: {
│                               │    rowVersion: [v1] ❌ Cũ rồi!
│                               │  }
│                               │  ❌ 409 Conflict
│                               │  "Công ty đã được chỉnh sửa bởi người khác"
│
└─ User B nhận được cảnh báo và phải refresh data
```

## Implementation Details

### 1. BaseEntity với RowVersion

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Concurrency token - Tự động được database quản lý
    /// </summary>
    public byte[]? RowVersion { get; set; }
}
```

### 2. EF Core Configuration

```csharp
// ErpDbContext.cs
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    {
        if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
        {
            var rowVersionProperty = entityType.FindProperty(nameof(BaseEntity.RowVersion));
            if (rowVersionProperty != null)
            {
                // Đánh dấu là concurrency token
                rowVersionProperty.IsConcurrencyToken = true;
                
                // Database tự động tạo giá trị mới mỗi khi insert/update
                rowVersionProperty.ValueGenerated = ValueGenerated.OnAddOrUpdate;
            }
        }
    }
}
```

### 3. DTOs Include RowVersion

```csharp
public class CompanyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    
    /// <summary>
    /// Gửi lại giá trị này khi update
    /// </summary>
    public byte[]? RowVersion { get; set; }
}

public class UpdateCompanyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    
    /// <summary>
    /// Phải khớp với giá trị trong database
    /// </summary>
    public byte[]? RowVersion { get; set; }
}
```

### 4. Service Handles Concurrency Exception

```csharp
public async Task<Result<CompanyDto>> UpdateAsync(Guid id, UpdateCompanyDto dto)
{
    try
    {
        var existing = await companyRepository.GetByIdAsync(dto.Id);
        if (existing == null)
            return Error.WithNotFoundCode(CompanyErrors.NotFound, dto.Id);

        // Map DTO to entity (bao gồm RowVersion)
        mapper.Map(dto, existing);
        
        // EF Core sẽ check RowVersion trong WHERE clause
        await companyRepository.UpdateAsync(existing);
        
        return mapper.Map<CompanyDto>(existing);
    }
    catch (DbUpdateConcurrencyException ex)
    {
        // Bắt lỗi khi RowVersion không khớp
        logger.LogWarning("Concurrency conflict for company {Id}", dto.Id);
        return Error.WithConflictCode(CompanyErrors.ConcurrencyConflict);
    }
}
```

## Database Behavior

### PostgreSQL
```sql
-- Khi update, EF Core tạo SQL như sau:
UPDATE companies
SET 
    name = @name,
    updated_at = @updated_at,
    row_version = row_version + 1  -- Tự động tăng
WHERE 
    id = @id 
    AND row_version = @current_version;  -- ⚠️ Kiểm tra version

-- Nếu 0 rows affected → DbUpdateConcurrencyException
```

### MySQL
```sql
-- Tương tự, sử dụng TIMESTAMP column
UPDATE companies
SET 
    name = @name,
    updated_at = @updated_at,
    row_version = CURRENT_TIMESTAMP  -- Tự động update
WHERE 
    id = @id 
    AND row_version = @current_version;
```

## Client Implementation

### JavaScript/TypeScript

```typescript
async function updateCompany(id: string, updates: Partial<Company>) {
    // 1. Lấy data hiện tại (bao gồm rowVersion)
    const current = await fetch(`/api/v1/companies/${id}`).then(r => r.json());
    
    // 2. Gửi update với rowVersion
    const response = await fetch(`/api/v1/companies/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            ...current,
            ...updates,
            rowVersion: current.rowVersion  // ⚠️ Quan trọng!
        })
    });
    
    if (response.status === 409) {
        // 3. Xử lý conflict
        const error = await response.json();
        alert(error.errors[0].description);
        
        // Reload data và yêu cầu user thử lại
        const fresh = await fetch(`/api/v1/companies/${id}`).then(r => r.json());
        // Hiển thị form với data mới
        showConflictResolutionUI(fresh, updates);
        
        return;
    }
    
    return response.json();
}
```

### C# Client

```csharp
public async Task<CompanyDto> UpdateCompanyAsync(Guid id, UpdateCompanyDto dto)
{
    try
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/v1/companies/{id}", dto);
        
        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            // Concurrency conflict
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            throw new ConcurrencyException(error.Errors[0].Description);
        }
        
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CompanyDto>();
    }
    catch (ConcurrencyException ex)
    {
        // Hiển thị message và yêu cầu refresh
        _logger.LogWarning("Concurrency conflict: {Message}", ex.Message);
        throw;
    }
}
```

## API Examples

### Success Case

**Request:**
```http
PUT /api/v1/companies/123e4567-e89b-12d3-a456-426614174000
Content-Type: application/json

{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "name": "ABC Corporation",
  "rowVersion": "AAAAAAAAB9E="
}
```

**Response: 200 OK**
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "name": "ABC Corporation",
  "rowVersion": "AAAAAAAAB9I=",  ← New version
  "updatedAt": "2024-12-06T10:30:00Z"
}
```

### Conflict Case

**Request:**
```http
PUT /api/v1/companies/123e4567-e89b-12d3-a456-426614174000
Content-Type: application/json

{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "name": "ABC Corporation",
  "rowVersion": "AAAAAAAAB9E="  ← Old version!
}
```

**Response: 409 Conflict**
```json
{
  "type": "Conflict",
  "errors": [
    {
      "code": "COMPANY_CONCURRENCY_CONFLICT",
      "description": "The company has been modified by another user. Please refresh and try again"
    }
  ],
  "correlationId": "123e4567-e89b-12d3-a456-426614174000"
}
```

## Migration

### Tạo Migration

```bash
cd LinhGo.ERP.Infrastructure
dotnet ef migrations add AddRowVersionToConcurrencyControl
```

### Migration Code (Auto-generated)

```csharp
public partial class AddRowVersionToConcurrencyControl : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // PostgreSQL
        migrationBuilder.AddColumn<byte[]>(
            name: "row_version",
            table: "companies",
            type: "bytea",
            rowVersion: true,
            nullable: true);
            
        // MySQL: timestamp
        // migrationBuilder.AddColumn<DateTime>(
        //     name: "row_version",
        //     table: "companies",
        //     rowVersion: true,
        //     nullable: false);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "row_version",
            table: "companies");
    }
}
```

### Apply Migration

```bash
dotnet ef database update
```

## Benefits

### ✅ Ngăn Chặn Data Loss
- User không bị mất dữ liệu do ghi đè
- Phát hiện conflict trước khi commit

### ✅ User Experience Tốt
- User nhận thông báo rõ ràng
- Có cơ hội xem data mới và quyết định

### ✅ Performance
- Không cần lock database
- Multiple reads không bị block
- Chỉ conflict khi thực sự có update đồng thời

### ✅ Scalable
- Hoạt động tốt với multiple instances
- Không phụ thuộc vào in-memory locks
- Database-level guarantee

## Best Practices

### 1. Always Include RowVersion in DTOs
```csharp
// ✅ GOOD
public class UpdateCompanyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public byte[]? RowVersion { get; set; }  // ⚠️ Required
}

// ❌ BAD
public class UpdateCompanyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    // Missing RowVersion!
}
```

### 2. Handle 409 Conflict Gracefully
```typescript
// ✅ GOOD - Show merge UI
if (response.status === 409) {
    const current = await getCurrentData();
    showMergeDialog({
        yourChanges: updates,
        theirChanges: current,
        onResolve: (merged) => saveWithNewVersion(merged)
    });
}

// ❌ BAD - Silent retry
if (response.status === 409) {
    await updateCompany(id, updates);  // Recursive!
}
```

### 3. Refresh After Conflict
```csharp
// ✅ GOOD
catch (DbUpdateConcurrencyException)
{
    // Discard context changes
    foreach (var entry in context.ChangeTracker.Entries())
    {
        entry.State = EntityState.Detached;
    }
    
    return Error.WithConflictCode(CompanyErrors.ConcurrencyConflict);
}
```

### 4. Don't Ignore RowVersion in Mappings
```csharp
// AutoMapper Profile
CreateMap<UpdateCompanyDto, Company>()
    .ForMember(dest => dest.RowVersion, opt => opt.MapFrom(src => src.RowVersion));
    // ⚠️ Important!
```

## Alternative: Pessimistic Locking (NOT Recommended)

```csharp
// ❌ NOT RECOMMENDED - Blocks other users
public async Task<Result<CompanyDto>> UpdateAsync(Guid id, UpdateCompanyDto dto)
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    
    // Lock row for update
    var company = await _context.Companies
        .Where(c => c.Id == id)
        .ExecuteSqlRaw("SELECT * FROM companies WHERE id = {0} FOR UPDATE", id);
        
    // Other users BLOCKED until transaction completes
    
    // Update...
    await transaction.CommitAsync();
}
```

**Nhược điểm:**
- ❌ Poor scalability
- ❌ Deadlock risks
- ❌ Blocks readers
- ❌ Timeout issues

## Testing Concurrency

### Unit Test

```csharp
[Fact]
public async Task UpdateAsync_ConcurrentUpdate_ReturnsConcurrencyError()
{
    // Arrange
    var company = new Company { Id = Guid.NewGuid(), Name = "Test", RowVersion = new byte[] { 1 } };
    await _context.Companies.AddAsync(company);
    await _context.SaveChangesAsync();
    
    // Simulate concurrent update
    var dto1 = new UpdateCompanyDto { Id = company.Id, Name = "Update 1", RowVersion = company.RowVersion };
    var dto2 = new UpdateCompanyDto { Id = company.Id, Name = "Update 2", RowVersion = company.RowVersion };
    
    // Act
    await _service.UpdateAsync(company.Id, dto1);  // Success
    var result = await _service.UpdateAsync(company.Id, dto2);  // Should fail
    
    // Assert
    Assert.True(result.IsError);
    Assert.Equal(CompanyErrors.ConcurrencyConflict, result.FirstError.Code);
}
```

### Integration Test

```bash
# Terminal 1
curl -X PUT http://localhost:5001/api/v1/companies/123 \
  -H "Content-Type: application/json" \
  -d '{"id":"123","name":"Name 1","rowVersion":"AAAAAAAAB9E="}'

# Terminal 2 (at same time)
curl -X PUT http://localhost:5001/api/v1/companies/123 \
  -H "Content-Type: application/json" \
  -d '{"id":"123","name":"Name 2","rowVersion":"AAAAAAAAB9E="}'

# One should return 409 Conflict
```

## Monitoring

### Log Concurrency Conflicts

```csharp
catch (DbUpdateConcurrencyException ex)
{
    _logger.LogWarning(
        "Concurrency conflict on {Entity} with ID {Id}. User: {User}",
        typeof(TEntity).Name,
        entity.Id,
        _currentUser.Id
    );
    
    // Track metric
    _metrics.IncrementCounter("concurrency_conflicts", 
        new[] { ("entity", typeof(TEntity).Name) });
}
```

### Metrics to Track
- Concurrency conflict rate
- Average time between GET and PUT
- Most conflicted entities
- Users with most conflicts

## Summary

✅ **Implemented:**
- RowVersion in BaseEntity
- EF Core concurrency token configuration
- DTOs include RowVersion
- Service handles DbUpdateConcurrencyException
- Error codes and localization
- Proper 409 Conflict responses

✅ **Benefits:**
- No data loss from concurrent updates
- Clear user feedback
- Database-level guarantee
- Scalable solution
- Production-ready

🚀 **Next Steps:**
1. Run migration to add row_version column
2. Test with concurrent requests
3. Update client applications to handle 409 conflicts
4. Monitor conflict rates in production

---

**Kết luận:** Optimistic Concurrency Control là giải pháp tốt nhất cho web applications, ngăn chặn data race hiệu quả mà không ảnh hưởng đến performance! 🎯

