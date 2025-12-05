# Correlation ID Implementation Guide

## Overview
The Correlation ID feature has been implemented to provide request tracing and logging across the ERP system. This allows you to track requests through the entire application lifecycle, making debugging and monitoring much easier.

## Components

### 1. CorrelationIdMiddleware
**Location:** `LinhGo.ERP.Api/Middleware/CorrelationIdMiddleware.cs`

This middleware:
- Extracts or generates a correlation ID for each request
- Adds the correlation ID to response headers (`X-Correlation-ID`)
- Stores the correlation ID in `HttpContext.Items` for easy access
- Integrates with `Activity` for distributed tracing
- Logs request start and completion with correlation ID

**Features:**
- If client sends `X-Correlation-ID` header, it will be preserved
- If no correlation ID is provided, a new GUID will be generated
- All log entries within a request scope include the correlation ID

### 2. CorrelationIdService
**Location:** `LinhGo.ERP.Api/Services/CorrelationIdService.cs`

This service provides a convenient way to access the correlation ID from anywhere in the application:

```csharp
public interface ICorrelationIdService
{
    string GetCorrelationId();
}
```

**Usage in Services:**
```csharp
public class MyService
{
    private readonly ICorrelationIdService _correlationIdService;
    
    public MyService(ICorrelationIdService correlationIdService)
    {
        _correlationIdService = correlationIdService;
    }
    
    public async Task DoSomething()
    {
        var correlationId = _correlationIdService.GetCorrelationId();
        _logger.LogInformation("Processing request {CorrelationId}", correlationId);
    }
}
```

### 3. BaseApiController Enhancement
**Location:** `LinhGo.ERP.Api/Controllers/BaseApiController.cs`

The base controller now includes:
- A `CorrelationId` property to easily access the current correlation ID
- All error responses now include the correlation ID in the response body

**Error Response Format:**
```json
{
    "code": "COMPANY_NOT_FOUND",
    "description": "Company with ID xyz not found",
    "type": "NotFound",
    "correlationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

### 4. Middleware Extension
**Location:** `LinhGo.ERP.Api/Extensions/MiddlewareExtensions.cs`

Provides a clean extension method to add the middleware:
```csharp
app.UseCorrelationId();
```

## Configuration

The middleware is registered in `Program.cs`:

```csharp
// Services registration
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICorrelationIdService, CorrelationIdService>();

// Middleware pipeline
app.UseCorrelationId(); // Add early in pipeline
```

**Important:** The middleware should be added early in the pipeline (before other middleware) to ensure all requests are tracked.

## Usage Examples

### 1. Client Sending Correlation ID
```bash
curl -H "X-Correlation-ID: my-custom-id-123" \
     https://api.example.com/api/companies
```

The API will use `my-custom-id-123` for tracking this request.

### 2. API Generating Correlation ID
```bash
curl https://api.example.com/api/companies
```

The API will generate a new GUID (e.g., `3fa85f64-5717-4562-b3fc-2c963f66afa6`).

### 3. Accessing in Controllers
```csharp
public class CustomersController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var correlationId = CorrelationId; // Access from base class
        _logger.LogInformation("Fetching customers {CorrelationId}", correlationId);
        
        // ... your logic
    }
}
```

### 4. Using in Application Services
```csharp
public class CompanyService : ICompanyService
{
    private readonly ICorrelationIdService _correlationIdService;
    private readonly ILogger<CompanyService> _logger;
    
    public CompanyService(
        ICorrelationIdService correlationIdService,
        ILogger<CompanyService> logger)
    {
        _correlationIdService = correlationIdService;
        _logger = logger;
    }
    
    public async Task<Result<CompanyDto>> GetByIdAsync(Guid id)
    {
        var correlationId = _correlationIdService.GetCorrelationId();
        
        _logger.LogInformation(
            "Fetching company {CompanyId} [CorrelationId: {CorrelationId}]",
            id,
            correlationId);
        
        // ... your logic
    }
}
```

## Response Headers

Every API response includes the correlation ID in the response headers:

```
HTTP/1.1 200 OK
Content-Type: application/json
X-Correlation-ID: 3fa85f64-5717-4562-b3fc-2c963f66afa6

{
    "id": "...",
    "name": "..."
}
```

## Error Responses

All error responses include the correlation ID in the response body:

```json
{
    "code": "VALIDATION_ERROR",
    "description": "Invalid input data",
    "type": "Validation",
    "correlationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "errors": [...]
}
```

## Logging Integration

The middleware automatically creates a logging scope with the correlation ID. All logs within a request will include:

```
[2024-12-05 10:30:00] [Information] Request started: GET /api/companies [CorrelationId: 3fa85f64-5717-4562-b3fc-2c963f66afa6]
[2024-12-05 10:30:00] [Information] Fetching all companies [CorrelationId: 3fa85f64-5717-4562-b3fc-2c963f66afa6]
[2024-12-05 10:30:00] [Information] Request completed: GET /api/companies [CorrelationId: 3fa85f64-5717-4562-b3fc-2c963f66afa6] - Status: 200
```

## Distributed Tracing

The correlation ID is also added to the `Activity` (part of .NET's distributed tracing):

```csharp
Activity.Current?.SetTag("correlation_id", correlationId);
```

This integrates with OpenTelemetry and Application Insights if you're using them.

## Best Practices

1. **Always pass correlation ID from client:** When making API calls from frontend, pass the correlation ID in the `X-Correlation-ID` header.

2. **Include in logs:** Use the correlation ID in all log statements to make debugging easier.

3. **Chain across services:** If you have multiple services, pass the correlation ID between them to maintain traceability.

4. **Store in frontend:** Store the correlation ID in your frontend application to help with support requests.

5. **Show to users on errors:** Display the correlation ID to users when errors occur so they can report issues with the ID.

## Frontend Integration Example

### React/TypeScript
```typescript
const api = axios.create({
  baseURL: 'https://api.example.com'
});

// Add correlation ID to all requests
api.interceptors.request.use(config => {
  const correlationId = crypto.randomUUID();
  config.headers['X-Correlation-ID'] = correlationId;
  
  // Store for error handling
  sessionStorage.setItem('last-correlation-id', correlationId);
  
  return config;
});

// Show correlation ID on errors
api.interceptors.response.use(
  response => response,
  error => {
    const correlationId = error.response?.data?.correlationId;
    console.error('Request failed with correlation ID:', correlationId);
    alert(`An error occurred. Please report this ID: ${correlationId}`);
    return Promise.reject(error);
  }
);
```

## Monitoring and Troubleshooting

### Finding Logs by Correlation ID

If using structured logging (e.g., Serilog, Application Insights):

```csharp
// Application Insights Query
requests
| where customDimensions.CorrelationId == "3fa85f64-5717-4562-b3fc-2c963f66afa6"
| union (
    traces
    | where customDimensions.CorrelationId == "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  )
| order by timestamp asc
```

### Common Scenarios

1. **User reports an error:** Get the correlation ID from the error message and search logs.
2. **Performance issue:** Track a specific request through the entire pipeline using its correlation ID.
3. **Debugging API integration:** Pass correlation ID from client to server and track both sides.

## Limitations

- Correlation ID is request-scoped (does not persist across multiple requests)
- For multi-service architectures, you need to manually propagate the correlation ID
- Correlation ID is not authenticated or validated (clients can send any value)

## Future Enhancements

Consider these improvements:
1. Add correlation ID to database operations (e.g., EF Core interceptors)
2. Create a dedicated logging sink that groups by correlation ID
3. Add correlation ID to email notifications
4. Implement request/response logging middleware that includes full details
5. Add correlation ID to SignalR messages for real-time applications

