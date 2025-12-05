# Correlation ID Logging Configuration Guide

## Overview
The Correlation ID is automatically included in all logs through the logging scope mechanism in ASP.NET Core. You don't need to manually add it to each log statement - the middleware handles this automatically.

## How It Works

### 1. Middleware Logging Scope
The `CorrelationIdMiddleware` creates a logging scope at the beginning of each request:

```csharp
using (_logger.BeginScope(new Dictionary<string, object>
{
    ["CorrelationId"] = correlationId,
    ["RequestPath"] = context.Request.Path,
    ["RequestMethod"] = context.Request.Method
}))
{
    // All logs within this scope will include CorrelationId
    await _next(context);
}
```

### 2. Automatic Inclusion
Any logger used within the request pipeline will automatically include the correlation ID in its scope:

```csharp
// In your service
public class CompanyService
{
    private readonly ILogger<CompanyService> _logger;
    
    public async Task<Result<CompanyDto>> GetByIdAsync(Guid id)
    {
        // This log will automatically include CorrelationId from scope
        _logger.LogInformation("Fetching company by ID {CompanyId}", id);
        
        // No need to add: [CorrelationId: {CorrelationId}]
        // It's already in the logging context!
    }
}
```

## Configuration

### appsettings.Development.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    },
    "Console": {
      "FormatterName": "json",
      "FormatterOptions": {
        "SingleLine": false,
        "IncludeScopes": true,  // Important: must be true
        "TimestampFormat": "yyyy-MM-dd HH:mm:ss ",
        "UseUtcTimestamp": false
      }
    }
  }
}
```

**Key Setting:** `"IncludeScopes": true` - This ensures scope data (including CorrelationId) is included in logs.

## Log Output Examples

### Console Output (JSON Format)
```json
{
  "Timestamp": "2024-12-05 10:30:00",
  "EventId": 0,
  "LogLevel": "Information",
  "Category": "LinhGo.ERP.Application.Services.CompanyService",
  "Message": "Fetching company by ID 3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "State": {
    "CompanyId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "{OriginalFormat}": "Fetching company by ID {CompanyId}"
  },
  "Scopes": [
    {
      "Message": "System.Collections.Generic.Dictionary`2[System.String,System.Object]",
      "CorrelationId": "abc123-def456-ghi789",
      "RequestPath": "/api/companies/3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "RequestMethod": "GET"
    }
  ]
}
```

### Console Output (Simple Format)
```
info: LinhGo.ERP.Application.Services.CompanyService[0]
      => CorrelationId:abc123-def456-ghi789 RequestPath:/api/companies/... RequestMethod:GET
      Fetching company by ID 3fa85f64-5717-4562-b3fc-2c963f66afa6
```

## Using with Different Logging Providers

### 1. Serilog (Recommended)
Install packages:
```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File
```

Update `Program.cs`:
```csharp
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()  // Important: enables scope enrichment
    .WriteTo.Console(outputTemplate: 
        "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/log-.txt", 
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();
```

### 2. Application Insights
```csharp
builder.Services.AddApplicationInsightsTelemetry();

// Correlation ID will automatically be included in all telemetry
```

### 3. Custom Logging Provider
Ensure your custom provider supports `IExternalScopeProvider` to capture scope data.

## Best Practices

### ✅ DO
```csharp
// Simple, clean logging
_logger.LogInformation("Fetching company by ID {CompanyId}", id);
_logger.LogError(ex, "Error retrieving company {CompanyId}", id);
```

### ❌ DON'T
```csharp
// Don't manually add correlation ID - it's already in scope!
var correlationId = _correlationIdService.GetCorrelationId();
_logger.LogInformation("Fetching company {CompanyId} [CorrelationId: {CorrelationId}]", id, correlationId);
```

## Querying Logs

### Application Insights (Kusto Query)
```kusto
traces
| where customDimensions.CorrelationId == "abc123-def456-ghi789"
| order by timestamp asc
| project timestamp, message, severityLevel, customDimensions
```

### File Logs (grep)
```bash
# Linux/Mac
grep "abc123-def456-ghi789" logs/*.txt

# Windows PowerShell
Select-String -Pattern "abc123-def456-ghi789" -Path "logs\*.txt"
```

### Structured Log Viewers
Use tools like:
- **Seq**: `docker run -d -p 5341:80 -e ACCEPT_EULA=Y datalust/seq`
- **Elasticsearch + Kibana**
- **Splunk**

## Troubleshooting

### Correlation ID Not Appearing in Logs

1. **Check Scopes Are Enabled**
   ```json
   "IncludeScopes": true  // in appsettings.json
   ```

2. **Verify Middleware Order**
   ```csharp
   app.UseCorrelationId();  // Must be early in pipeline
   ```

3. **Check Logger Configuration**
   ```csharp
   // Ensure using standard ILogger<T>
   private readonly ILogger<CompanyService> _logger;
   ```

### Correlation ID Empty in Application Layer

The `ICorrelationIdService` is only available in the API layer (requires HttpContext). For the Application layer, rely on logging scopes which work automatically.

## Performance Considerations

- **Logging Scopes**: Minimal overhead (~microseconds per request)
- **Structured Logging**: More expensive than simple string logging but provides better queryability
- **Recommendation**: Use `Information` level in production, `Debug` in development

## Summary

The key advantage of this approach:
- ✅ **Automatic**: No manual correlation ID passing needed
- ✅ **Clean Code**: Services remain simple without correlation ID clutter
- ✅ **Consistent**: All logs within a request automatically tagged
- ✅ **Standard**: Uses built-in ASP.NET Core logging features
- ✅ **Flexible**: Works with any logging provider that supports scopes

Just write your logs normally, and the correlation ID will be there automatically! 🎉

