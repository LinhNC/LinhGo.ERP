# ✅ CompanyErrors.SearchFailed Localization Added

## Summary

Successfully added localization support for the `COMPANY_SEARCH_FAILED` error code in both English and Vietnamese.

## Changes Made

### 1. English Localization (`en.json`)
**File:** `/LinhGo.ERP.Application/Resources/Localization/en.json`

**Added:**
```json
"COMPANY_SEARCH_FAILED": "Error searching companies. Please try again"
```

**Location:** After `COMPANY_CONCURRENCY_CONFLICT` in the Company errors section

### 2. Vietnamese Localization (`vi.json`)
**File:** `/LinhGo.ERP.Application/Resources/Localization/vi.json`

**Added:**
```json
"COMPANY_SEARCH_FAILED": "Lỗi khi tìm kiếm công ty. Vui lòng thử lại"
```

**Location:** After `COMPANY_CONCURRENCY_CONFLICT` in the Company errors section

### 3. Fixed CompanyService.cs
**File:** `/LinhGo.ERP.Application/Services/CompanyService.cs`

**Before:**
```csharp
return Error.WithFailureCode("CompanyErrors.SearchFailed");  // ❌ String literal
```

**After:**
```csharp
return Error.WithFailureCode(CompanyErrors.SearchFailed);  // ✅ Constant reference
```

## Error Code Details

### Error Code Definition
**File:** `/LinhGo.ERP.Application/Common/Errors/CompanyErrors.cs`

```csharp
public const string SearchFailed = "COMPANY_SEARCH_FAILED";
```

### Usage in CompanyService
```csharp
public async Task<Result<PagedResult<CompanyDto>>> SearchAsync(SearchQueryParams queries, CancellationToken ctx)
{
    try
    {
        var result = await companyRepository.SearchAsync(queries, ctx);
        var mappedResult = new PagedResult<CompanyDto>
        {
            Items = mapper.Map<IEnumerable<CompanyDto>>(result.Items),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
        
        return mappedResult;
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error searching companies with queries {@Queries}", queries);
        return Error.WithFailureCode(CompanyErrors.SearchFailed);  // ✅ Uses localized error
    }
}
```

## How It Works

### 1. Error Code Flow
```
CompanyService.SearchAsync()
    ↓ (exception occurs)
Error.WithFailureCode(CompanyErrors.SearchFailed)
    ↓
Looks up "COMPANY_SEARCH_FAILED" in localization files
    ↓
Returns localized message based on user's language
```

### 2. Localization Resolution
- **English (en-US):** "Error searching companies. Please try again"
- **Vietnamese (vi-VN):** "Lỗi khi tìm kiếm công ty. Vui lòng thử lại"

### 3. API Response Example

**English Response:**
```json
{
  "success": false,
  "error": {
    "code": "COMPANY_SEARCH_FAILED",
    "message": "Error searching companies. Please try again"
  }
}
```

**Vietnamese Response:**
```json
{
  "success": false,
  "error": {
    "code": "COMPANY_SEARCH_FAILED",
    "message": "Lỗi khi tìm kiếm công ty. Vui lòng thử lại"
  }
}
```

## Complete Company Error Codes

All company-related error codes now have localization:

| Error Code | English | Vietnamese |
|------------|---------|------------|
| COMPANY_NOTFOUND | Company with ID {0} not found | Không tìm thấy công ty với ID {0} |
| COMPANY_CREATE_FAILED | Failed to create company | Tạo công ty thất bại |
| COMPANY_UPDATE_FAILED | Failed to update company | Cập nhật công ty thất bại |
| COMPANY_DELETE_FAILED | Failed to delete company | Xóa công ty thất bại |
| COMPANY_GET_ID_FAILED | Error retrieving company by ID | Lỗi khi truy xuất công ty theo ID |
| COMPANY_GET_ALL_FAILED | Error retrieving companies | Lỗi khi truy xuất danh sách công ty |
| COMPANY_GET_ACTIVE_FAILED | Error retrieving active companies | Lỗi khi truy xuất công ty đang hoạt động |
| COMPANY_GET_CODE_FAILED | Error retrieving company by code | Lỗi khi truy xuất công ty theo mã |
| COMPANY_NAME_REQUIRED | Company name is required | Tên công ty là bắt buộc |
| COMPANY_NAME_TOO_LONG | Company name must not exceed {0} characters | Tên công ty không được vượt quá {0} ký tự |
| COMPANY_CODE_DUPLICATE | Company code already exists | Mã công ty đã tồn tại |
| COMPANY_DUPLICATE_CODE | Company code '{0}' already exists | Mã công ty '{0}' đã tồn tại |
| COMPANY_ID_MISMATCH | ID mismatch. Route {0} vs Body {1} | ID không khớp. Đường dẫn {0} vs Nội dung {1} |
| COMPANY_CONCURRENCY_CONFLICT | Modified by another user | Đã được chỉnh sửa bởi người dùng khác |
| **COMPANY_SEARCH_FAILED** | **Error searching companies** | **Lỗi khi tìm kiếm công ty** |

## Testing

### Test English Localization
```bash
# Set Accept-Language header to English
curl -H "Accept-Language: en-US" \
     http://localhost:5000/api/companies/search?filter[invalid]=value

# Expected response:
{
  "success": false,
  "error": {
    "code": "COMPANY_SEARCH_FAILED",
    "message": "Error searching companies. Please try again"
  }
}
```

### Test Vietnamese Localization
```bash
# Set Accept-Language header to Vietnamese
curl -H "Accept-Language: vi-VN" \
     http://localhost:5000/api/companies/search?filter[invalid]=value

# Expected response:
{
  "success": false,
  "error": {
    "code": "COMPANY_SEARCH_FAILED",
    "message": "Lỗi khi tìm kiếm công ty. Vui lòng thử lại"
  }
}
```

## Files Modified

1. ✅ `/LinhGo.ERP.Application/Resources/Localization/en.json` - Added English translation
2. ✅ `/LinhGo.ERP.Application/Resources/Localization/vi.json` - Added Vietnamese translation
3. ✅ `/LinhGo.ERP.Application/Services/CompanyService.cs` - Fixed error code reference
4. ✅ `/LinhGo.ERP.Application/Common/Errors/CompanyErrors.cs` - Error code already defined (no change needed)

## Benefits

✅ **Internationalized** - Supports English and Vietnamese  
✅ **User-friendly** - Clear error messages in user's language  
✅ **Consistent** - Follows same pattern as other error codes  
✅ **Maintainable** - Centralized error message management  
✅ **Professional** - Production-ready error handling  

## Summary

✅ **Added COMPANY_SEARCH_FAILED to en.json** - English translation  
✅ **Added COMPANY_SEARCH_FAILED to vi.json** - Vietnamese translation  
✅ **Fixed CompanyService.cs** - Uses constant instead of string literal  
✅ **No compilation errors** - Code builds successfully  
✅ **Complete localization** - All company errors now localized  

**The CompanyErrors.SearchFailed error code is now fully localized!** 🌍✨

