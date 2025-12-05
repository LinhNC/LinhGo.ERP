# Scalar API Documentation - Implementation Guide

## ✅ Swagger Replaced with Scalar

Your LinhGo ERP API now uses **Scalar** instead of Swagger for API documentation. Scalar provides a modern, beautiful, and interactive API reference.

## What is Scalar?

Scalar is a next-generation API documentation tool that:
- ✨ Has a beautiful, modern UI
- 🚀 Is faster than Swagger UI
- 📱 Is mobile-friendly and responsive
- 🎨 Supports multiple themes
- 🔧 Has better customization options
- 💻 Generates code examples in multiple languages
- 📖 Provides better developer experience

## Changes Made

### 1. Packages Updated
**Removed:**
- ❌ `Swashbuckle.AspNetCore` (Swagger)

**Added:**
- ✅ `Scalar.AspNetCore` v2.11.1

### 2. Program.cs Configuration

```csharp
// Add OpenAPI/Scalar
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// ...

// Map OpenAPI endpoint
app.MapOpenApi();

// Use Scalar for API documentation
app.MapScalarApiReference(options =>
{
    options
        .WithTitle("LinhGo ERP API")
        .WithTheme(Scalar.AspNetCore.ScalarTheme.Purple)
        .WithDefaultHttpClient(Scalar.AspNetCore.ScalarTarget.CSharp, Scalar.AspNetCore.ScalarClient.HttpClient);
});
```

### 3. Files Deleted
- ❌ `Extensions/SwaggerExtensions.cs` (no longer needed)

## Accessing Scalar Documentation

### Development
Once you start your API:
```powershell
cd E:\Projects\NET\LinhGo.ERP\LinhGo.ERP.Api
dotnet run
```

Then open your browser to:
- **Scalar UI**: `https://localhost:5001/scalar/v1` (or your configured port)
- **OpenAPI JSON**: `https://localhost:5001/openapi/v1.json`

### URLs by Version
- **v1.0**: `https://localhost:5001/scalar/v1`
- **v2.0**: `https://localhost:5001/scalar/v2` (when you create v2)

## Scalar Features

### 1. Multiple Themes
Available themes in your configuration:
```csharp
options.WithTheme(Scalar.AspNetCore.ScalarTheme.Purple)  // Current
// Other options:
// ScalarTheme.Default
// ScalarTheme.Alternate
// ScalarTheme.Moon
// ScalarTheme.Solarized
// ScalarTheme.BluePlanet
```

### 2. Code Generation
Scalar automatically generates code examples in:
- C# (HttpClient) - Default for your project
- JavaScript/TypeScript
- Python
- PHP
- Ruby
- Go
- Java
- Shell (cURL)

### 3. Try It Out
Users can test your API directly from the documentation with:
- Request body editor
- Header customization
- Query parameter modification
- Real-time response preview

### 4. Search
Fast, client-side search across all endpoints.

### 5. Dark/Light Mode
Automatic theme switching based on user preference.

## Customization Options

### Change Title
```csharp
app.MapScalarApiReference(options =>
{
    options.WithTitle("Your Custom API Title");
});
```

### Change Theme
```csharp
options.WithTheme(Scalar.AspNetCore.ScalarTheme.Moon);
```

### Set Default HTTP Client
```csharp
options.WithDefaultHttpClient(
    Scalar.AspNetCore.ScalarTarget.CSharp,      // Language
    Scalar.AspNetCore.ScalarClient.HttpClient   // Client library
);

// Other options:
// ScalarTarget: CSharp, JavaScript, Python, PHP, etc.
// ScalarClient: HttpClient, RestSharp, Axios, Fetch, etc.
```

### Add Authentication
```csharp
options.WithAuthentication(authentication =>
{
    authentication.PreferredSecurityScheme = "Bearer";
});
```

### Custom Servers
```csharp
options.WithServers(new[]
{
    new ScalarServer("https://api.linhgo.com", "Production"),
    new ScalarServer("https://staging-api.linhgo.com", "Staging"),
    new ScalarServer("http://localhost:5001", "Local")
});
```

### Add Metadata
```csharp
options.WithMetadata(metadata =>
{
    metadata.Title = "LinhGo ERP API";
    metadata.Description = "Multi-company ERP system";
    metadata.Version = "1.0";
    metadata.Contact = new ScalarContact
    {
        Name = "Support",
        Email = "support@linhgo.com",
        Url = "https://linhgo.com"
    };
});
```

## OpenAPI Configuration

### Add XML Comments
To get better documentation, enable XML comments in your project:

1. **Update `.csproj`:**
```xml
<PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

2. **Add XML comments to controllers:**
```csharp
/// <summary>
/// Get all companies
/// </summary>
/// <remarks>
/// Returns a list of all companies in the system.
/// 
/// Sample request:
/// 
///     GET /api/v1/companies
///     
/// </remarks>
/// <returns>List of companies</returns>
/// <response code="200">Returns the list of companies</response>
/// <response code="500">If there was an internal server error</response>
[HttpGet]
[ProducesResponseType(typeof(IEnumerable<CompanyDto>), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public async Task<IActionResult> GetAll()
{
    // ...
}
```

## API Versioning with Scalar

Scalar automatically detects API versions and creates separate documentation pages:

### URL Pattern
- Version 1: `https://localhost:5001/scalar/v1`
- Version 2: `https://localhost:5001/scalar/v2`

### Controller Example
```csharp
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class CompaniesController : BaseApiController
{
    // Your endpoints
}

[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class CompaniesV2Controller : BaseApiController
{
    // New/updated endpoints for v2
}
```

## Comparison: Swagger vs Scalar

| Feature | Swagger UI | Scalar |
|---------|-----------|--------|
| **UI/UX** | Dated, cluttered | Modern, clean |
| **Performance** | Slower | Much faster |
| **Mobile** | Poor | Excellent |
| **Search** | Basic | Advanced |
| **Themes** | Limited | Multiple |
| **Code Examples** | Basic | Advanced |
| **Try It Out** | Yes | Better UX |
| **Bundle Size** | Larger | Smaller |

## Production Considerations

### Security
By default, Scalar is available in all environments. To restrict to development only:

```csharp
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
```

### Custom Route
```csharp
app.MapScalarApiReference(options =>
{
    options.WithEndpointPrefix("/docs");  // Access at /docs instead of /scalar
});
```

### Disable in Production
```csharp
if (!app.Environment.IsProduction())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
```

## Troubleshooting

### Issue: Scalar page shows "No API definition found"
**Solution:** Make sure `app.MapOpenApi()` is called before `app.MapScalarApiReference()`

### Issue: API version not showing
**Solution:** Ensure API versioning is properly configured and controllers have `[ApiVersion]` attribute

### Issue: XML comments not showing
**Solution:** 
1. Enable XML documentation in `.csproj`
2. Add XML comments to your methods
3. Rebuild the project

### Issue: Custom theme not applying
**Solution:** Clear browser cache or try incognito mode

## Testing Your Scalar Setup

1. **Start the API:**
   ```powershell
   cd E:\Projects\NET\LinhGo.ERP\LinhGo.ERP.Api
   dotnet run
   ```

2. **Open Scalar:**
   Navigate to `https://localhost:5001/scalar/v1`

3. **Test an Endpoint:**
   - Find the `GET /api/v1/companies` endpoint
   - Click "Send Request"
   - View the response

## Additional Resources

- **Scalar Documentation**: https://github.com/scalar/scalar
- **OpenAPI Specification**: https://swagger.io/specification/
- **ASP.NET Core OpenAPI**: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi

## Summary

✅ Swagger has been completely removed  
✅ Scalar is now your API documentation tool  
✅ Modern, beautiful UI  
✅ Better developer experience  
✅ API versioning fully integrated  
✅ Multiple themes available  
✅ Better code generation  

Your API documentation is now powered by Scalar! 🎉

