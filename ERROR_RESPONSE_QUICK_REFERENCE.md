# Error Response Models - Quick Reference

## ✅ What Was Done

Created strongly-typed error response models so **OpenAPI/Swagger** can properly document error responses.

## New Classes

### Location: `/LinhGo.ERP.Api/Models/ErrorResponse.cs`

```csharp
public class ErrorResponse
{
    public required string Type { get; init; }
    public required List<ErrorDetail> Errors { get; init; }
    public required string CorrelationId { get; init; }
}

public class ErrorDetail
{
    public required string Code { get; init; }
    public required string Description { get; init; }
}
```

## JSON Output Example

```json
{
  "type": "NotFound",
  "errors": [
    {
      "code": "COMPANY_NOTFOUND",
      "description": "Company with ID 42 not found"
    }
  ],
  "correlationId": "123e4567-e89b-12d3-a456-426614174000"
}
```

## Benefits

✅ **OpenAPI Documentation** - Swagger UI now shows error response schema  
✅ **Type Safety** - Compile-time checking  
✅ **IntelliSense** - Code completion in IDE  
✅ **Client Generation** - Auto-generate typed clients  
✅ **No Breaking Changes** - Same JSON output as before  

## View Documentation

Navigate to: **`/scalar/v1`** to see the improved API documentation with error response schemas.

## Bonus: Added Error Code

**`CompanyErrors.IdMismatch`** - For when route ID doesn't match body ID

**Usage:**
```csharp
if (id != dto.Id)
{
    return Error.WithValidationCode(CompanyErrors.IdMismatch, id, dto.Id);
}
```

**Messages:**
- 🇺🇸 English: "Company ID mismatch. Route ID {0} does not match request body ID {1}"
- 🇻🇳 Vietnamese: "ID công ty không khớp. ID trong đường dẫn {0} không khớp với ID trong nội dung yêu cầu {1}"

---

**That's it!** Your API now has proper error response documentation. 🚀

