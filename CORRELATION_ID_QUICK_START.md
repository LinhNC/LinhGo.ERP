# Correlation ID - Quick Example

## How It Works Now (Automatic)

### 1. Client Request
```bash
curl -H "X-Correlation-ID: abc-123" https://localhost:5001/api/companies/123
```

### 2. Middleware Captures Correlation ID
The `CorrelationIdMiddleware` automatically:
- Extracts correlation ID from header (or generates new one)
- Creates a logging scope with the correlation ID
- Adds correlation ID to response headers

### 3. Service Logs (No Manual Work!)
```csharp
public class CompanyService
{
    private readonly ILogger<CompanyService> _logger;
    
    public async Task<Result<CompanyDto>> GetByIdAsync(Guid id)
    {
        // Just log normally - correlation ID is automatically included!
        _logger.LogInformation("Fetching company by ID {CompanyId}", id);
        
        // ... your business logic
        
        _logger.LogInformation("Successfully retrieved company {CompanyId}", id);
    }
}
```

### 4. Console Output
All logs automatically include the correlation ID in the scope:

```
info: LinhGo.ERP.Api.Middleware.CorrelationIdMiddleware[0]
      => CorrelationId:abc-123 RequestPath:/api/companies/123 RequestMethod:GET
      Request started: GET /api/companies/123 [CorrelationId: abc-123]

info: LinhGo.ERP.Application.Services.CompanyService[0]
      => CorrelationId:abc-123 RequestPath:/api/companies/123 RequestMethod:GET
      Fetching company by ID 123

info: LinhGo.ERP.Application.Services.CompanyService[0]
      => CorrelationId:abc-123 RequestPath:/api/companies/123 RequestMethod:GET
      Successfully retrieved company 123

info: LinhGo.ERP.Api.Middleware.CorrelationIdMiddleware[0]
      => CorrelationId:abc-123 RequestPath:/api/companies/123 RequestMethod:GET
      Request completed: GET /api/companies/123 [CorrelationId: abc-123] - Status: 200
```

### 5. API Response
```
HTTP/1.1 200 OK
X-Correlation-ID: abc-123
Content-Type: application/json

{
  "id": "123",
  "name": "My Company",
  ...
}
```

### 6. Error Response (with Correlation ID)
```json
{
  "code": "COMPANY_NOTFOUND",
  "description": "Company with ID 123 not found",
  "type": "NotFound",
  "correlationId": "abc-123"
}
```

## Key Benefits

✅ **Zero Manual Work**: No need to pass correlation ID around
✅ **Automatic**: Works for all logs in the request pipeline
✅ **Clean Code**: Services stay focused on business logic
✅ **Consistent**: Same pattern across all services
✅ **Standard**: Uses ASP.NET Core's built-in logging scopes

## Configuration Checklist

- [x] Middleware registered in `Program.cs`: `app.UseCorrelationId()`
- [x] Logging scopes enabled: `"IncludeScopes": true` in appsettings
- [x] Services use `ILogger<T>` (standard ASP.NET Core logger)
- [x] Correlation ID added to error responses in `BaseApiController`

## Testing

Start the API and make a request:

```bash
# PowerShell
Invoke-WebRequest -Uri "http://localhost:5000/api/companies" -Headers @{"X-Correlation-ID"="test-123"}

# The logs will show:
# => CorrelationId:test-123 RequestPath:/api/companies RequestMethod:GET
```

That's it! The correlation ID is automatically tracked through the entire request. 🎉

