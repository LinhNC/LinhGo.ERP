# Optimistic Concurrency Control - Quick Reference

## ✅ Đã Implement

### 1. Thêm RowVersion vào BaseEntity
```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public byte[]? RowVersion { get; set; }  // ← NEW
}
```

### 2. Configure EF Core
```csharp
// ErpDbContext.cs - Tự động set làm concurrency token
rowVersionProperty.IsConcurrencyToken = true;
rowVersionProperty.ValueGenerated = ValueGenerated.OnAddOrUpdate;
```

### 3. DTOs Bao Gồm RowVersion
```csharp
public class CompanyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public byte[]? RowVersion { get; set; }  // ← Trả về cho client
}

public class UpdateCompanyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public byte[]? RowVersion { get; set; }  // ← Client gửi lại
}
```

### 4. Service Xử Lý Conflict
```csharp
catch (DbUpdateConcurrencyException ex)
{
    return Error.WithConflictCode(CompanyErrors.ConcurrencyConflict);
}
```

### 5. Error Code & Localization
- **Code:** `COMPANY_CONCURRENCY_CONFLICT`
- **EN:** "The company has been modified by another user. Please refresh and try again"
- **VI:** "Công ty đã được chỉnh sửa bởi người dùng khác. Vui lòng làm mới và thử lại"

## 🔄 Cách Hoạt Động

### Flow Thành Công
```
1. Client GET /companies/1
   ← { id: 1, name: "ABC", rowVersion: [v1] }

2. Client PUT /companies/1
   → { id: 1, name: "XYZ", rowVersion: [v1] }
   
3. Database kiểm tra: rowVersion == [v1]? ✅ YES
   → UPDATE và tăng version lên [v2]
   
4. Response 200 OK
   ← { id: 1, name: "XYZ", rowVersion: [v2] }
```

### Flow Conflict
```
User A                          User B
├─ GET (rowVersion: [v1])      ├─ GET (rowVersion: [v1])
│                               │
├─ PUT (rowVersion: [v1])      │
│  ✅ SUCCESS → [v2]            │
│                               ├─ PUT (rowVersion: [v1])
│                               │  ❌ 409 CONFLICT
│                               │  Database có [v2], không khớp [v1]
```

## 📋 Next Steps

### 1. Tạo Migration
```bash
cd LinhGo.ERP.Infrastructure
dotnet ef migrations add AddRowVersionForConcurrency
dotnet ef database update
```

### 2. Test API
```bash
# Terminal 1
curl -X GET http://localhost:5001/api/v1/companies/123

# Lưu rowVersion từ response

# Terminal 2 - Update 1
curl -X PUT http://localhost:5001/api/v1/companies/123 \
  -d '{"id":"123","name":"Test1","rowVersion":"..."}'

# Terminal 3 - Update 2 (cùng rowVersion)
curl -X PUT http://localhost:5001/api/v1/companies/123 \
  -d '{"id":"123","name":"Test2","rowVersion":"..."}'
# → Should return 409 Conflict
```

### 3. Client Handling
```typescript
try {
    await updateCompany(id, data);
} catch (error) {
    if (error.status === 409) {
        alert('Data đã được người khác chỉnh sửa. Vui lòng refresh!');
        // Reload data
        const fresh = await getCompany(id);
        // Show merge UI
    }
}
```

## ⚠️ Quan Trọng

1. **Always include RowVersion** trong update DTOs
2. **Map RowVersion** trong AutoMapper profiles
3. **Handle 409 Conflict** ở client side
4. **Don't retry** tự động khi có conflict
5. **Refresh data** sau khi conflict

## 📊 Response Examples

### Success (200 OK)
```json
{
  "id": "123e4567-...",
  "name": "Updated Name",
  "rowVersion": "AAAAAAAAB9I="
}
```

### Conflict (409)
```json
{
  "type": "Conflict",
  "errors": [{
    "code": "COMPANY_CONCURRENCY_CONFLICT",
    "description": "The company has been modified by another user..."
  }],
  "correlationId": "..."
}
```

---

**Tóm tắt:** Optimistic Concurrency Control ngăn chặn data race bằng cách kiểm tra version trước khi update. Nếu không khớp → 409 Conflict! 🎯

