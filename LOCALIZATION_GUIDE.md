# Multi-Language Error Messages Guide

## Overview

The LinhGo ERP system now supports multi-language error messages. API responses automatically return error descriptions in the client's preferred language based on the `Accept-Language` HTTP header.

## Supported Languages

Currently supported languages:
- **English (en)** - Default language
- **Vietnamese (vi)**

## How It Works

### 1. Request with Language Header

Clients send their preferred language using the standard HTTP `Accept-Language` header:

```http
GET /api/v1/companies/123
Accept-Language: vi
```

or

```http
GET /api/v1/companies/123
Accept-Language: en
```

### 2. Multi-Language Header Support

The system also supports multiple languages with quality values:

```http
Accept-Language: vi-VN,vi;q=0.9,en-US;q=0.8,en;q=0.7
```

The system will use the first supported language (in this case, `vi`).

### 3. Response with Localized Errors

**English Response:**
```json
{
  "type": "NotFound",
  "errors": [
    {
      "code": "COMPANY_NOTFOUND",
      "description": "Company with ID 123 not found"
    }
  ],
  "correlationId": "abc-123-def"
}
```

**Vietnamese Response:**
```json
{
  "type": "NotFound",
  "errors": [
    {
      "code": "COMPANY_NOTFOUND",
      "description": "Không tìm thấy công ty với ID 123"
    }
  ],
  "correlationId": "abc-123-def"
}
```

## Architecture

### Components

1. **IErrorMessageLocalizer** - Interface for error message localization
2. **ErrorMessageLocalizer** - Implementation with in-memory dictionary of error messages
3. **RequestLocalizationMiddleware** - Middleware to parse `Accept-Language` header
4. **BaseApiController** - Updated to use localizer for error responses

### Flow

```
Client Request
    ↓
RequestLocalizationMiddleware (Parse Accept-Language)
    ↓
Set Culture & Store in HttpContext
    ↓
Controller Action
    ↓
BaseApiController.ToResponse()
    ↓
ErrorMessageLocalizer.GetErrorMessage()
    ↓
Localized Response
```

## Error Codes

### Company Errors
- `COMPANY_NOTFOUND` - Company not found
- `COMPANY_CREATE_FAILED` - Failed to create company
- `COMPANY_UPDATE_FAILED` - Failed to update company
- `COMPANY_DELETE_FAILED` - Failed to delete company
- `COMPANY_NAME_REQUIRED` - Company name is required
- `COMPANY_NAME_TOO_LONG` - Company name too long
- `COMPANY_CODE_DUPLICATE` - Company code already exists

### User Errors
- `USER_NOTFOUND` - User not found
- `USER_CREATE_FAILED` - Failed to create user
- `USER_UPDATE_FAILED` - Failed to update user
- `USER_DELETE_FAILED` - Failed to delete user
- `USER_EMAIL_REQUIRED` - Email is required
- `USER_EMAIL_INVALID` - Email format is invalid
- `USER_EMAIL_DUPLICATE` - Email already exists
- `USER_PASSWORD_REQUIRED` - Password is required
- `USER_PASSWORD_TOO_SHORT` - Password too short

### Customer Errors
- `CUSTOMER_NOTFOUND` - Customer not found
- `CUSTOMER_CREATE_FAILED` - Failed to create customer
- `CUSTOMER_UPDATE_FAILED` - Failed to update customer
- `CUSTOMER_DELETE_FAILED` - Failed to delete customer
- `CUSTOMER_NAME_REQUIRED` - Customer name is required
- `CUSTOMER_CODE_DUPLICATE` - Customer code already exists

### Product Errors
- `PRODUCT_NOTFOUND` - Product not found
- `PRODUCT_CREATE_FAILED` - Failed to create product
- `PRODUCT_UPDATE_FAILED` - Failed to update product
- `PRODUCT_DELETE_FAILED` - Failed to delete product
- `PRODUCT_NAME_REQUIRED` - Product name is required
- `PRODUCT_SKU_DUPLICATE` - Product SKU already exists
- `PRODUCT_INSUFFICIENT_STOCK` - Insufficient stock

### Order Errors
- `ORDER_NOTFOUND` - Order not found
- `ORDER_CREATE_FAILED` - Failed to create order
- `ORDER_UPDATE_FAILED` - Failed to update order
- `ORDER_DELETE_FAILED` - Failed to delete order
- `ORDER_NUMBER_DUPLICATE` - Order number already exists
- `ORDER_ITEMS_REQUIRED` - Order must have at least one item

### Warehouse Errors
- `WAREHOUSE_NOTFOUND` - Warehouse not found
- `WAREHOUSE_CREATE_FAILED` - Failed to create warehouse
- `WAREHOUSE_UPDATE_FAILED` - Failed to update warehouse
- `WAREHOUSE_DELETE_FAILED` - Failed to delete warehouse
- `WAREHOUSE_NAME_REQUIRED` - Warehouse name is required
- `WAREHOUSE_CODE_DUPLICATE` - Warehouse code already exists

### General Errors
- `VALIDATION_FAILED` - Validation failed
- `UNAUTHORIZED` - Unauthorized access
- `FORBIDDEN` - Access forbidden
- `INTERNAL_ERROR` - Internal error occurred
- `INVALID_REQUEST` - Invalid request
- `RESOURCE_NOT_FOUND` - Resource not found

## Adding New Error Messages

To add new error messages:

1. **Add error code constant** (optional, for consistency):
```csharp
public static class ErrorCodes
{
    public const string InvoiceNotFound = "INVOICE_NOTFOUND";
}
```

2. **Add translations** in `ErrorMessageLocalizer.InitializeErrorMessages()`:
```csharp
// English
_localizedMessages["en"]["INVOICE_NOTFOUND"] = "Invoice with ID {0} not found";

// Vietnamese
_localizedMessages["vi"]["INVOICE_NOTFOUND"] = "Không tìm thấy hóa đơn với ID {0}";
```

3. **Use in service**:
```csharp
return Result<InvoiceDto>.Failure(
    Error.NotFound("INVOICE_NOTFOUND", $"Invoice with ID {id} not found")
);
```

## Adding New Languages

To add a new language (e.g., French):

1. **Update RequestLocalizationMiddleware**:
```csharp
private readonly string[] _supportedLanguages = { "en", "vi", "fr" };
```

2. **Add translations** in `ErrorMessageLocalizer.InitializeErrorMessages()`:
```csharp
_localizedMessages["fr"] = new Dictionary<string, string>
{
    ["COMPANY_NOTFOUND"] = "Entreprise avec ID {0} non trouvée",
    // ... add all error codes
};
```

## Using with Parameters

Error messages support string formatting with parameters:

```csharp
// In service
return Result<ProductDto>.Failure(
    Error.Validation("PRODUCT_INSUFFICIENT_STOCK", 
        $"Insufficient stock. Available: {available}, Required: {required}")
);

// The localizer will format it:
// EN: "Insufficient stock. Available: 10, Required: 20"
// VI: "Tồn kho không đủ. Có sẵn: 10, Yêu cầu: 20"
```

## Controller Implementation

Controllers must inject `IErrorMessageLocalizer` and pass it to the base controller:

```csharp
public class CompaniesController : BaseApiController
{
    private readonly ICompanyService _companyService;

    public CompaniesController(
        ICompanyService companyService, 
        IErrorMessageLocalizer localizer) 
        : base(localizer)
    {
        _companyService = companyService;
    }
    
    // ... controller actions
}
```

## Testing

### Test with curl

**English:**
```bash
curl -H "Accept-Language: en" http://localhost:5000/api/v1/companies/invalid-id
```

**Vietnamese:**
```bash
curl -H "Accept-Language: vi" http://localhost:5000/api/v1/companies/invalid-id
```

### Test with Postman

1. Open the request
2. Go to Headers tab
3. Add header: `Accept-Language` with value `vi` or `en`
4. Send request

### Test with JavaScript/Fetch

```javascript
fetch('http://localhost:5000/api/v1/companies/invalid-id', {
  headers: {
    'Accept-Language': 'vi'
  }
})
.then(response => response.json())
.then(data => console.log(data));
```

## Best Practices

1. **Always use error codes** - Use standardized error codes instead of hardcoded messages
2. **Keep codes consistent** - Use UPPERCASE with underscores (e.g., `ENTITY_ACTION_REASON`)
3. **Add all languages** - When adding new error codes, add translations for all supported languages
4. **Use parameters** - Use string formatting for dynamic values (IDs, names, etc.)
5. **Fallback to English** - The system automatically falls back to English if translation is not found
6. **Log missing translations** - The localizer logs warnings for missing translations

## Performance Considerations

- Error messages are stored in memory (ConcurrentDictionary)
- No database queries for localization
- Middleware runs early in pipeline
- Thread-safe implementation

## Future Enhancements

Possible improvements:
- Load translations from database or JSON files
- Support for resource files (.resx)
- Admin UI to manage translations
- Pluralization support
- Date/time localization
- Number formatting based on culture

