# 🚀 LinhGo.ERP - Developer Quick Start Guide

## Prerequisites
- .NET 9.0 SDK
- SQL Server (LocalDB or full version)
- Visual Studio 2022 or JetBrains Rider

---

## Getting Started

### 1. Clone & Build
```powershell
cd E:\Projects\NET\LinhGo.ERP
dotnet restore
dotnet build
```

### 2. Database Setup
```powershell
# Update connection string in appsettings.json
# Then create migration and database
dotnet ef migrations add InitialCreate --project LinhGo.ERP.Infrastructure --startup-project LinhGo.ERP.Web
dotnet ef database update --project LinhGo.ERP.Infrastructure --startup-project LinhGo.ERP.Web
```

### 3. Register Services in Program.cs
```csharp
using LinhGo.ERP.Infrastructure;
using LinhGo.ERP.Application;

var builder = WebApplication.CreateBuilder(args);

// Add layers
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// Add controllers
builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();
app.Run();
```

---

## Creating Your First API Controller

### Example: CustomersController
```csharp
using Microsoft.AspNetCore.Mvc;
using LinhGo.ERP.Application.Services;
using LinhGo.ERP.Application.DTOs.Customers;

namespace LinhGo.ERP.Api.Controllers;

[ApiController]
[Route("api/companies/{companyId}/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _customerService.GetPagedAsync(companyId, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid companyId, Guid id)
    {
        var result = await _customerService.GetByIdAsync(companyId, id);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    [HttpGet("{id}/details")]
    public async Task<IActionResult> GetDetails(Guid companyId, Guid id)
    {
        var result = await _customerService.GetDetailsAsync(companyId, id);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(Guid companyId, [FromQuery] string term)
    {
        var result = await _customerService.SearchAsync(companyId, term);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid companyId, CreateCustomerDto dto)
    {
        var result = await _customerService.CreateAsync(companyId, dto);
        if (!result.Success) return BadRequest(result);
        return CreatedAtAction(nameof(GetById), new { companyId, id = result.Data!.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid companyId, Guid id, UpdateCustomerDto dto)
    {
        if (id != dto.Id) return BadRequest("ID mismatch");
        var result = await _customerService.UpdateAsync(companyId, dto);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid companyId, Guid id)
    {
        var result = await _customerService.DeleteAsync(companyId, id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}
```

---

## Common Patterns

### 1. Result Pattern
```csharp
var result = await _service.CreateAsync(dto);

if (result.Success)
{
    // Success case
    return Ok(result);
}
else
{
    // Error case
    return BadRequest(result);
}
```

### 2. Pagination
```csharp
var result = await _service.GetPagedAsync(companyId, page, pageSize);

// Result contains:
// - Items (current page data)
// - TotalCount
// - Page
// - PageSize
// - TotalPages
// - HasNextPage
// - HasPreviousPage
```

### 3. Search
```csharp
var result = await _service.SearchAsync(companyId, searchTerm);
// Searches across code, name, email, etc.
```

### 4. Multi-Tenancy
```csharp
// Always pass companyId
var customers = await _repository.GetAllAsync(companyId);
var customer = await _repository.GetByIdAsync(companyId, customerId);
```

### 5. User-Company Assignment
```csharp
// Assign user to company
await _userManagementService.AssignToCompanyAsync(new AssignUserToCompanyDto
{
    UserId = userId,
    CompanyId = companyId,
    Role = "Manager"
});

// Grant permissions
await _userManagementService.GrantPermissionsAsync(new GrantPermissionsDto
{
    UserId = userId,
    CompanyId = companyId,
    PermissionKeys = new List<string> { "customers.view", "customers.create" }
});

// Check permission
var result = await _userManagementService.HasPermissionAsync(userId, companyId, "customers.edit");
if (result.Data)
{
    // User has permission
}
```

---

## Testing Examples

### Unit Test
```csharp
public class CustomerServiceTests
{
    [Fact]
    public async Task CreateCustomer_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var mockRepo = new Mock<ICustomerRepository>();
        var mockMapper = new Mock<IMapper>();
        var service = new CustomerService(mockRepo.Object, mockMapper.Object);

        mockRepo.Setup(r => r.IsCodeUniqueAsync(It.IsAny<Guid>(), It.IsAny<string>(), default))
                .ReturnsAsync(true);

        var dto = new CreateCustomerDto { Code = "C001", Name = "Test Customer" };

        // Act
        var result = await service.CreateAsync(Guid.NewGuid(), dto);

        // Assert
        Assert.True(result.Success);
    }
}
```

### Integration Test
```csharp
public class CustomersControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CustomersControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCustomers_ReturnsOk()
    {
        var companyId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/companies/{companyId}/customers");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
```

---

## Useful Commands

### Build
```powershell
dotnet build
dotnet build --no-incremental
```

### Run
```powershell
dotnet run --project LinhGo.ERP.Web
```

### Test
```powershell
dotnet test
dotnet test --logger "console;verbosity=detailed"
```

### Database
```powershell
# Create migration
dotnet ef migrations add MigrationName --project LinhGo.ERP.Infrastructure --startup-project LinhGo.ERP.Web

# Update database
dotnet ef database update --project LinhGo.ERP.Infrastructure --startup-project LinhGo.ERP.Web

# Remove last migration
dotnet ef migrations remove --project LinhGo.ERP.Infrastructure --startup-project LinhGo.ERP.Web

# List migrations
dotnet ef migrations list --project LinhGo.ERP.Infrastructure --startup-project LinhGo.ERP.Web
```

---

## Project Structure

```
LinhGo.ERP/
├── LinhGo.ERP.Domain/           # Entities, Enums, Interfaces
│   ├── Common/
│   ├── Companies/
│   ├── Users/
│   ├── Customers/
│   ├── Inventory/
│   └── Orders/
├── LinhGo.ERP.Infrastructure/   # Data Access, Repositories
│   ├── Data/
│   │   ├── Context/
│   │   └── Configurations/
│   └── Repositories/
├── LinhGo.ERP.Application/      # Business Logic, Services
│   ├── Common/
│   ├── DTOs/
│   ├── Services/
│   ├── Validators/
│   └── Mappings/
└── LinhGo.ERP.Api/              # API Controllers (TODO)
    └── Controllers/
```

---

## Key Files

- `ErpDbContext.cs` - EF Core DbContext
- `TenantContext.cs` - Multi-tenancy context
- `MappingProfile.cs` - AutoMapper configuration
- `DependencyInjection.cs` - Service registration (both Infrastructure & Application)

---

## Common Issues & Solutions

### Issue: DbContext not found
**Solution**: Ensure connection string is configured in appsettings.json

### Issue: Migration fails
**Solution**: Check entity configurations and foreign key relationships

### Issue: AutoMapper mapping error
**Solution**: Verify mapping configuration in MappingProfile.cs

### Issue: Validation fails
**Solution**: Check FluentValidation rules in AllValidators.cs

---

## Next Steps

1. ✅ Domain Layer - Complete
2. ✅ Infrastructure Layer - Complete
3. ✅ Application Layer - Complete
4. 🔜 API Controllers - Create RESTful endpoints
5. 🔜 Authentication - JWT implementation
6. 🔜 Authorization - Permission-based
7. 🔜 Swagger - API documentation
8. 🔜 Tests - Unit & Integration
9. 🔜 Docker - Containerization
10. 🔜 CI/CD - Automated deployment

---

## Resources

- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [EF Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [AutoMapper Documentation](https://docs.automapper.org/)
- [FluentValidation Documentation](https://docs.fluentvalidation.net/)

---

**Happy Coding! 🚀**

