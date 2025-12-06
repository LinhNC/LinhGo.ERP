# API Error Response Format

## Overview
All API error responses follow a consistent format to make error handling predictable and easy to implement in client applications.

## Error Response Structure

### ErrorResponse
```json
{
  "type": "string",
  "errors": [
    {
      "code": "string",
      "description": "string"
    }
  ],
  "correlationId": "string"
}
```

### Properties

#### `type` (string, required)
The type of error that occurred. Possible values:
- `NotFound` - Requested resource does not exist (HTTP 404)
- `Validation` - Request validation failed (HTTP 400)
- `Conflict` - Request conflicts with existing data (HTTP 409)
- `Failure` - General business logic failure (HTTP 400)
- `Unexpected` - Unexpected server error (HTTP 500)

#### `errors` (array, required)
List of error details. Each error contains:
- `code` (string, required) - Machine-readable error code for programmatic handling
- `description` (string, required) - Human-readable localized error message

#### `correlationId` (string, required)
Unique identifier for tracking this request across services and logs.
Format: UUID (e.g., `123e4567-e89b-12d3-a456-426614174000`)

## Examples

### Not Found Error (404)
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

**Request:**
```http
GET /api/v1/companies/42
Accept-Language: en
```

### Validation Error (400)
```json
{
  "type": "Validation",
  "errors": [
    {
      "code": "COMPANY_NAME_REQUIRED",
      "description": "Company name is required"
    },
    {
      "code": "COMPANY_NAME_TOO_LONG",
      "description": "Company name must not exceed 100 characters"
    }
  ],
  "correlationId": "234e5678-e89b-12d3-a456-426614174001"
}
```

**Request:**
```http
POST /api/v1/companies
Content-Type: application/json

{
  "name": "",
  "code": "TEST"
}
```

### Conflict Error (409)
```json
{
  "type": "Conflict",
  "errors": [
    {
      "code": "COMPANY_DUPLICATE_CODE",
      "description": "Company code 'ACME' already exists"
    }
  ],
  "correlationId": "345e6789-e89b-12d3-a456-426614174002"
}
```

**Request:**
```http
POST /api/v1/companies
Content-Type: application/json

{
  "name": "Acme Corp",
  "code": "ACME"
}
```

### Multiple Validation Errors (400)
```json
{
  "type": "Validation",
  "errors": [
    {
      "code": "PRODUCT_NAME_REQUIRED",
      "description": "Product name is required"
    },
    {
      "code": "PRODUCT_INSUFFICIENT_STOCK",
      "description": "Insufficient stock. Available: 5, Required: 10"
    },
    {
      "code": "VALIDATION_EMAIL_INVALID",
      "description": "Invalid email format"
    }
  ],
  "correlationId": "456e789a-e89b-12d3-a456-426614174003"
}
```

### ID Mismatch Error (400)
```json
{
  "type": "Validation",
  "errors": [
    {
      "code": "COMPANY_ID_MISMATCH",
      "description": "Company ID mismatch. Route ID 10 does not match request body ID 5"
    }
  ],
  "correlationId": "567e89ab-e89b-12d3-a456-426614174004"
}
```

**Request:**
```http
PUT /api/v1/companies/10
Content-Type: application/json

{
  "id": 5,
  "name": "Updated Company"
}
```

## Localization

Error messages are automatically localized based on the `Accept-Language` header:

### English (en)
```http
GET /api/v1/companies/42
Accept-Language: en
```
**Response:**
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

### Vietnamese (vi)
```http
GET /api/v1/companies/42
Accept-Language: vi
```
**Response:**
```json
{
  "type": "NotFound",
  "errors": [
    {
      "code": "COMPANY_NOTFOUND",
      "description": "Không tìm thấy công ty với ID 42"
    }
  ],
  "correlationId": "123e4567-e89b-12d3-a456-426614174000"
}
```

## Error Code Categories

### Company Errors (COMPANY_*)
- `COMPANY_NOTFOUND` - Company not found
- `COMPANY_CREATE_FAILED` - Failed to create company
- `COMPANY_UPDATE_FAILED` - Failed to update company
- `COMPANY_DELETE_FAILED` - Failed to delete company
- `COMPANY_NAME_REQUIRED` - Company name is required
- `COMPANY_NAME_TOO_LONG` - Company name exceeds maximum length
- `COMPANY_DUPLICATE_CODE` - Company code already exists
- `COMPANY_ID_MISMATCH` - ID in route doesn't match ID in body

### User Errors (USER_*)
- `USER_NOTFOUND` - User not found
- `USER_EMAIL_REQUIRED` - Email is required
- `USER_EMAIL_INVALID` - Email format is invalid
- `USER_EMAIL_DUPLICATE` - Email already exists
- `USER_PASSWORD_TOO_SHORT` - Password too short

### Product Errors (PRODUCT_*)
- `PRODUCT_NOTFOUND` - Product not found
- `PRODUCT_NAME_REQUIRED` - Product name is required
- `PRODUCT_SKU_DUPLICATE` - Product SKU already exists
- `PRODUCT_INSUFFICIENT_STOCK` - Insufficient stock

### Validation Errors (VALIDATION_*)
- `VALIDATION_FAILED` - General validation failure
- `VALIDATION_REQUIRED` - Required field missing
- `VALIDATION_INVALID_FORMAT` - Invalid format
- `VALIDATION_MAX_LENGTH` - Exceeds maximum length
- `VALIDATION_MIN_LENGTH` - Below minimum length

## HTTP Status Code Mapping

| Error Type | HTTP Status Code | Use Case |
|------------|-----------------|----------|
| `NotFound` | 404 Not Found | Resource doesn't exist |
| `Validation` | 400 Bad Request | Invalid input data |
| `Conflict` | 409 Conflict | Duplicate key, constraint violation |
| `Failure` | 400 Bad Request | Business logic failure |
| `Unexpected` | 500 Internal Server Error | Unexpected server error |

## Client Implementation

### JavaScript/TypeScript
```typescript
interface ErrorResponse {
  type: string;
  errors: ErrorDetail[];
  correlationId: string;
}

interface ErrorDetail {
  code: string;
  description: string;
}

async function getCompany(id: number): Promise<Company> {
  const response = await fetch(`/api/v1/companies/${id}`, {
    headers: {
      'Accept-Language': 'en'
    }
  });
  
  if (!response.ok) {
    const error: ErrorResponse = await response.json();
    console.error(`Error ${error.correlationId}:`, error.errors);
    throw new Error(error.errors[0].description);
  }
  
  return response.json();
}
```

### C# Client
```csharp
public class ErrorResponse
{
    public string Type { get; set; }
    public List<ErrorDetail> Errors { get; set; }
    public string CorrelationId { get; set; }
}

public class ErrorDetail
{
    public string Code { get; set; }
    public string Description { get; set; }
}

public async Task<Company> GetCompanyAsync(int id)
{
    var response = await _httpClient.GetAsync($"/api/v1/companies/{id}");
    
    if (!response.IsSuccessStatusCode)
    {
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        _logger.LogError("Request failed with correlation ID: {CorrelationId}", 
            error.CorrelationId);
        throw new ApiException(error.Errors[0].Description);
    }
    
    return await response.Content.ReadFromJsonAsync<Company>();
}
```

### Python Client
```python
from dataclasses import dataclass
from typing import List

@dataclass
class ErrorDetail:
    code: str
    description: str

@dataclass
class ErrorResponse:
    type: str
    errors: List[ErrorDetail]
    correlation_id: str

def get_company(id: int) -> dict:
    response = requests.get(
        f'/api/v1/companies/{id}',
        headers={'Accept-Language': 'en'}
    )
    
    if not response.ok:
        error = ErrorResponse(**response.json())
        print(f"Error {error.correlation_id}: {error.errors[0].description}")
        raise Exception(error.errors[0].description)
    
    return response.json()
```

## Best Practices

### For API Consumers

1. **Always check HTTP status code first**
   ```typescript
   if (!response.ok) {
     const error = await response.json();
     // Handle error
   }
   ```

2. **Use error codes for programmatic handling**
   ```typescript
   if (error.errors.some(e => e.code === 'COMPANY_NOTFOUND')) {
     // Show "not found" UI
   }
   ```

3. **Display descriptions to users**
   ```typescript
   error.errors.forEach(e => {
     showNotification(e.description); // Already localized
   });
   ```

4. **Log correlation IDs for support**
   ```typescript
   console.error(`Error correlation ID: ${error.correlationId}`);
   // Users can provide this ID to support team
   ```

5. **Handle multiple errors gracefully**
   ```typescript
   // Validation can return multiple errors
   error.errors.forEach(e => {
     markFieldInvalid(e.code, e.description);
   });
   ```

### For API Developers

1. **Always use error codes from constants**
   ```csharp
   return Error.WithNotFoundCode(CompanyErrors.NotFound, id);
   ```

2. **Include relevant parameters**
   ```csharp
   return Error.WithValidationCode(
       CompanyErrors.IdMismatch, 
       routeId, 
       bodyId
   );
   ```

3. **Use appropriate error types**
   - `NotFound` for missing resources
   - `Validation` for input errors
   - `Conflict` for duplicates
   - `Failure` for business logic

4. **Test localization**
   - Verify messages in all supported languages
   - Ensure parameters are formatted correctly

## OpenAPI/Swagger Documentation

The error response format is automatically documented in OpenAPI/Swagger with:
- Full schema definitions
- Example values
- Property descriptions
- Response type mapping

You can view the complete API documentation at `/scalar/v1` endpoint.

## Troubleshooting

### Missing Localization
If you see an error code instead of a message:
```json
{
  "description": "COMPANY_NOTFOUND"
}
```
This means the error code is not in the localization files. Add it to:
- `Resources/Localization/en.json`
- `Resources/Localization/vi.json`

### Correlation ID Tracking
To trace a request through logs:
```bash
# Search logs by correlation ID
grep "123e4567-e89b-12d3-a456-426614174000" logs/linhgo-erp-*.log
```

### Multiple Errors Not Showing
Ensure you're collecting all errors before returning:
```csharp
var errors = new List<Error>();
// Validate all fields
if (errors.Any())
    return errors;
```

## Related Documentation
- [Localization Guide](LOCALIZATION_GUIDE.md)
- [Error Codes Centralization](ERROR_CODES_CENTRALIZATION.md)
- [Correlation ID Guide](CORRELATION_ID_GUIDE.md)
- [API Documentation](SCALAR_API_DOCUMENTATION.md)

