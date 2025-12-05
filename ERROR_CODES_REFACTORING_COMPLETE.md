# ✅ Error Codes Refactored - Separated into Individual Files

## Changes Made

The error codes have been reorganized from a single monolithic `ErrorCodes.cs` file into **separate files**, one for each module, located in the `Common/Errors/` folder.

---

## New File Structure

```
LinhGo.ERP.Application/
├── Common/
│   ├── Errors/                           ← NEW FOLDER
│   │   ├── CompanyErrors.cs              ← Company error codes (12 codes)
│   │   ├── UserErrors.cs                 ← User error codes (9 codes)
│   │   ├── CustomerErrors.cs             ← Customer error codes (6 codes)
│   │   ├── ProductErrors.cs              ← Product error codes (7 codes)
│   │   ├── OrderErrors.cs                ← Order error codes (6 codes)
│   │   ├── WarehouseErrors.cs            ← Warehouse error codes (6 codes)
│   │   └── GeneralErrors.cs              ← General error codes (6 codes)
│   │
│   └── ErrorCodes.cs                     ← DELETED (no longer needed)
```

**Total: 7 separate error files, 52 error codes**

---

## Before (Single File) ❌

```csharp
// ErrorCodes.cs - One big file with all nested classes
public static class ErrorCodes
{
    public static class Company { /* 12 codes */ }
    public static class User { /* 9 codes */ }
    public static class Customer { /* 6 codes */ }
    public static class Product { /* 7 codes */ }
    public static class Order { /* 6 codes */ }
    public static class Warehouse { /* 6 codes */ }
    public static class General { /* 6 codes */ }
}
```

**Problems:**
- ❌ Single large file (100+ lines)
- ❌ Hard to navigate
- ❌ Merge conflicts when multiple developers add errors
- ❌ All modules in one file

---

## After (Separate Files) ✅

### CompanyErrors.cs
```csharp
namespace LinhGo.ERP.Application.Common.Errors;

public static class CompanyErrors
{
    public const string NotFound = "COMPANY_NOTFOUND";
    public const string CreateFailed = "COMPANY_CREATE_FAILED";
    public const string UpdateFailed = "COMPANY_UPDATE_FAILED";
    public const string DeleteFailed = "COMPANY_DELETE_FAILED";
    public const string GetByIdFailed = "COMPANY_GET_ID_FAILED";
    public const string GetAllFailed = "COMPANY_GET_ALL_FAILED";
    public const string GetActiveFailed = "COMPANY_GET_ACTIVE_FAILED";
    public const string GetByCodeFailed = "COMPANY_GET_CODE_FAILED";
    public const string NameRequired = "COMPANY_NAME_REQUIRED";
    public const string NameTooLong = "COMPANY_NAME_TOO_LONG";
    public const string CodeDuplicate = "COMPANY_CODE_DUPLICATE";
    public const string DuplicateCode = "COMPANY_DUPLICATE_CODE";
}
```

### UserErrors.cs
```csharp
namespace LinhGo.ERP.Application.Common.Errors;

public static class UserErrors
{
    public const string NotFound = "USER_NOTFOUND";
    public const string CreateFailed = "USER_CREATE_FAILED";
    public const string UpdateFailed = "USER_UPDATE_FAILED";
    public const string DeleteFailed = "USER_DELETE_FAILED";
    public const string EmailRequired = "USER_EMAIL_REQUIRED";
    public const string EmailInvalid = "USER_EMAIL_INVALID";
    public const string EmailDuplicate = "USER_EMAIL_DUPLICATE";
    public const string PasswordRequired = "USER_PASSWORD_REQUIRED";
    public const string PasswordTooShort = "USER_PASSWORD_TOO_SHORT";
}
```

### CustomerErrors.cs
```csharp
namespace LinhGo.ERP.Application.Common.Errors;

public static class CustomerErrors
{
    public const string NotFound = "CUSTOMER_NOTFOUND";
    public const string CreateFailed = "CUSTOMER_CREATE_FAILED";
    public const string UpdateFailed = "CUSTOMER_UPDATE_FAILED";
    public const string DeleteFailed = "CUSTOMER_DELETE_FAILED";
    public const string NameRequired = "CUSTOMER_NAME_REQUIRED";
    public const string CodeDuplicate = "CUSTOMER_CODE_DUPLICATE";
}
```

*...and so on for Product, Order, Warehouse, General*

---

## Benefits

### ✅ Better Organization
- Each module has its own file
- Easy to find error codes by module
- Clear separation of concerns

### ✅ Easier Maintenance
- Add new errors to specific file only
- No need to scroll through large file
- Focused changes per module

### ✅ Better for Teams
- Reduced merge conflicts
- Multiple developers can work on different error files simultaneously
- Clearer PR diffs

### ✅ Cleaner Code
- Each file is small and focused
- Easy to review
- Better code organization

### ✅ Scalability
- Easy to add new modules
- Just create new `{Module}Errors.cs` file
- No impact on other files

---

## Usage in Services

### Before
```csharp
return Error.NotFound(ErrorCodes.Company.NotFound, id.ToString());
return Error.Failure(ErrorCodes.Company.CreateFailed);
```

### After
```csharp
return Error.NotFound(CompanyErrors.NotFound, id.ToString());
return Error.Failure(CompanyErrors.CreateFailed);
```

**Simpler and more direct!**

---

## Updated Files

### 1. CompanyService.cs ✅
```csharp
using LinhGo.ERP.Application.Common.Errors;

// Old: ErrorCodes.Company.NotFound
// New: CompanyErrors.NotFound
return Error.NotFound(CompanyErrors.NotFound, id.ToString());
return Error.Failure(CompanyErrors.CreateFailed);
```

### 2. ErrorMessageLocalizer.cs ✅
```csharp
using LinhGo.ERP.Application.Common.Errors;

private static HashSet<string> GetAllErrorCodesFromConstants()
{
    var errorTypes = new[]
    {
        typeof(CompanyErrors),
        typeof(UserErrors),
        typeof(CustomerErrors),
        typeof(ProductErrors),
        typeof(OrderErrors),
        typeof(WarehouseErrors),
        typeof(GeneralErrors)
    };
    // ...scan all error classes
}
```

---

## Adding New Error Codes

### Step 1: Add to Appropriate Error File

**For Company Module:**
Edit `Common/Errors/CompanyErrors.cs`:
```csharp
public static class CompanyErrors
{
    // ...existing codes...
    
    public const string StatusInvalid = "COMPANY_STATUS_INVALID"; // NEW
}
```

**For New Module:**
Create `Common/Errors/InvoiceErrors.cs`:
```csharp
namespace LinhGo.ERP.Application.Common.Errors;

public static class InvoiceErrors
{
    public const string NotFound = "INVOICE_NOTFOUND";
    public const string CreateFailed = "INVOICE_CREATE_FAILED";
    // ...more codes
}
```

### Step 2: Add to ErrorMessageLocalizer Validation
Edit `ErrorMessageLocalizer.cs`:
```csharp
var errorTypes = new[]
{
    typeof(CompanyErrors),
    typeof(UserErrors),
    // ...existing types...
    typeof(InvoiceErrors)  // NEW
};
```

### Step 3: Add Translations
Update JSON files with new error codes (as before).

### Step 4: Use in Service
```csharp
using LinhGo.ERP.Application.Common.Errors;

return Error.NotFound(InvoiceErrors.NotFound, id);
```

---

## File Sizes

| File | Lines | Codes |
|------|-------|-------|
| CompanyErrors.cs | 20 | 12 |
| UserErrors.cs | 17 | 9 |
| CustomerErrors.cs | 14 | 6 |
| ProductErrors.cs | 15 | 7 |
| OrderErrors.cs | 14 | 6 |
| WarehouseErrors.cs | 14 | 6 |
| GeneralErrors.cs | 14 | 6 |
| **Total** | **108** | **52** |

**vs. Old ErrorCodes.cs: ~120 lines (all in one file)**

---

## Migration Checklist

- [x] Create individual error files in `Common/Errors/` folder
- [x] Delete `ErrorCodes.cs` file
- [x] Update `CompanyService.cs` to use `CompanyErrors`
- [x] Update `ErrorMessageLocalizer.cs` to scan individual error classes
- [x] Verify build succeeds
- [x] All error codes still accessible
- [x] No breaking changes

---

## Best Practices

### ✅ DO:
- Keep one error class per module
- Use consistent naming: `{Module}Errors.cs`
- Group related error codes together
- Add XML comments for each class
- Use descriptive constant names

### ❌ DON'T:
- Mix multiple modules in one file
- Create deeply nested structures
- Use abbreviations in file names
- Forget to add new error classes to ErrorMessageLocalizer validation

---

## Example: Adding Invoice Module

### 1. Create InvoiceErrors.cs
```csharp
namespace LinhGo.ERP.Application.Common.Errors;

/// <summary>
/// Invoice-related error codes
/// </summary>
public static class InvoiceErrors
{
    public const string NotFound = "INVOICE_NOTFOUND";
    public const string CreateFailed = "INVOICE_CREATE_FAILED";
    public const string UpdateFailed = "INVOICE_UPDATE_FAILED";
    public const string DeleteFailed = "INVOICE_DELETE_FAILED";
    public const string Overdue = "INVOICE_OVERDUE";
    public const string AlreadyPaid = "INVOICE_ALREADY_PAID";
}
```

### 2. Update ErrorMessageLocalizer.cs
```csharp
var errorTypes = new[]
{
    typeof(CompanyErrors),
    typeof(UserErrors),
    typeof(CustomerErrors),
    typeof(ProductErrors),
    typeof(OrderErrors),
    typeof(WarehouseErrors),
    typeof(GeneralErrors),
    typeof(InvoiceErrors)  // ADD THIS
};
```

### 3. Add Translations
**en.json:**
```json
{
  "INVOICE_NOTFOUND": "Invoice with ID {0} not found",
  "INVOICE_CREATE_FAILED": "Failed to create invoice",
  "INVOICE_OVERDUE": "Invoice is overdue",
  "INVOICE_ALREADY_PAID": "Invoice has already been paid"
}
```

**vi.json:**
```json
{
  "INVOICE_NOTFOUND": "Không tìm thấy hóa đơn với ID {0}",
  "INVOICE_CREATE_FAILED": "Tạo hóa đơn thất bại",
  "INVOICE_OVERDUE": "Hóa đơn đã quá hạn",
  "INVOICE_ALREADY_PAID": "Hóa đơn đã được thanh toán"
}
```

### 4. Use in Service
```csharp
using LinhGo.ERP.Application.Common.Errors;

public class InvoiceService
{
    public async Task<Result<InvoiceDto>> GetByIdAsync(Guid id)
    {
        var invoice = await repository.GetByIdAsync(id);
        if (invoice == null)
        {
            return Error.NotFound(InvoiceErrors.NotFound, id.ToString());
        }
        
        if (invoice.IsPaid)
        {
            return Error.Conflict(InvoiceErrors.AlreadyPaid);
        }
        
        return mapper.Map<InvoiceDto>(invoice);
    }
}
```

**Done! Clean and organized!** ✅

---

## Summary

### What Changed
- ✅ **Deleted:** `ErrorCodes.cs` (monolithic file)
- ✅ **Created:** 7 separate error files in `Common/Errors/` folder
- ✅ **Updated:** `CompanyService.cs` to use `CompanyErrors` directly
- ✅ **Updated:** `ErrorMessageLocalizer.cs` to scan individual error classes

### Benefits
1. ✅ **Better Organization** - Each module has its own file
2. ✅ **Easier Maintenance** - Small, focused files
3. ✅ **Reduced Conflicts** - Multiple developers can work simultaneously
4. ✅ **Cleaner Code** - Clear separation of concerns
5. ✅ **Scalable** - Easy to add new modules

### Impact
- ✅ **No Breaking Changes** - Error code values unchanged
- ✅ **Build Successful** - All references updated
- ✅ **Functionality Intact** - All features work as before
- ✅ **Better Structure** - More maintainable codebase

---

**Status: ✅ COMPLETE**  
**Files Created: 7**  
**Files Updated: 2**  
**Files Deleted: 1**  
**Build: SUCCESS**  

Your error codes are now properly organized with each module in its own file! 🎉

