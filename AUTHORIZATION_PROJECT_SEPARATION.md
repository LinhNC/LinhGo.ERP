# ✅ Authorization Project Separation - Architecture Decision

## Decision: YES - Separate Authorization into its own project

---

## 🎯 Why Separate Authorization?

### For Enterprise ERP Systems, authorization should be a separate project because:

1. **Multi-Client Architecture**
   - API uses it ✅
   - Web (Blazor) uses it ✅
   - Future: Mobile app, Desktop app, Background jobs ✅

2. **Domain-Driven Design**
   - Authorization is a **bounded context**
   - It has its own domain logic (multi-tenancy, permissions)
   - Should be independent from presentation layer

3. **Testability**
   - Unit test authorization logic without API/Web dependencies
   - Mock HTTP context for isolated testing
   - Test permission resolution independently

4. **Reusability**
   - Share between API and Web projects
   - Consistent authorization across all clients
   - Single source of truth for permissions

5. **Maintainability**
   - Clear separation of concerns
   - Easy to find authorization code
   - Changes don't affect other layers

---

## 📦 New Project Structure

```
LinhGo.ERP.sln
├── LinhGo.ERP.Domain/
├── LinhGo.ERP.Application/
├── LinhGo.ERP.Infrastructure/
├── LinhGo.ERP.Authorization/  ⭐ NEW
│   ├── Services/
│   │   ├── ITenantService.cs
│   │   ├── TenantService.cs
│   │   └── IPermissionService.cs
│   ├── Filters/
│   │   ├── RequireCompanyAccessAttribute.cs
│   │   ├── RequirePermissionAttribute.cs
│   │   └── RequireCompanyRoleAttribute.cs
│   ├── Requirements/
│   │   ├── CompanyAccessRequirement.cs
│   │   ├── PermissionRequirement.cs
│   │   └── CompanyRoleRequirement.cs
│   ├── Handlers/
│   │   ├── CompanyAccessHandler.cs
│   │   ├── PermissionHandler.cs
│   │   └── CompanyRoleHandler.cs
│   ├── Policies/
│   │   └── AuthorizationPolicyBuilder.cs
│   └── Extensions/
│       └── AuthorizationServiceCollectionExtensions.cs
├── LinhGo.ERP.Api/
│   └── (uses LinhGo.ERP.Authorization)
└── LinhGo.ERP.Web/
    └── (uses LinhGo.ERP.Authorization)
```

---

## 🏗️ Layered Architecture

```
┌─────────────────────────────────────────────────┐
│         Presentation Layer                       │
│  ┌──────────────┐      ┌──────────────┐        │
│  │ API          │      │ Web (Blazor) │        │
│  └──────┬───────┘      └──────┬───────┘        │
│         │                     │                  │
│         └──────────┬──────────┘                 │
└────────────────────┼──────────────────────────┘
                     │
        ┌────────────▼────────────┐
        │   Authorization Layer    │ ⭐ NEW
        │  Multi-Tenant Security   │
        │  Permission Resolution   │
        │  Role Management         │
        └────────────┬─────────────┘
                     │
        ┌────────────▼────────────┐
        │   Application Layer      │
        │  Business Logic          │
        └────────────┬─────────────┘
                     │
        ┌────────────▼────────────┐
        │   Infrastructure Layer   │
        │  Database, Cache, etc.   │
        └──────────────────────────┘
```

---

## 📋 What Gets Moved to Authorization Project

### From `LinhGo.ERP.Api/Services/`:
- ✅ `TenantService.cs` → `LinhGo.ERP.Authorization/Services/`
- ✅ `ITenantService.cs` → `LinhGo.ERP.Authorization/Services/`

### From `LinhGo.ERP.Api/Filters/`:
- ✅ `RequireCompanyAccessAttribute.cs` → `LinhGo.ERP.Authorization/Filters/`
- ✅ `RequirePermissionAttribute.cs` → `LinhGo.ERP.Authorization/Filters/`
- ✅ `RequireCompanyRoleAttribute.cs` → `LinhGo.ERP.Authorization/Filters/`

### New Components to Create:
- ✅ `IPermissionService.cs` - Permission resolution service
- ✅ Authorization policy builders
- ✅ Extension methods for DI registration

---

## 🎯 Benefits of Separation

### 1. **Shared Authorization Logic**
```csharp
// In API
[Authorize]
[RequireCompanyAccess]
public class ApiController : ControllerBase { }

// In Blazor Web
@attribute [Authorize]
@attribute [RequireCompanyAccess]
<AuthorizeView>
```

### 2. **Centralized Permission Management**
```csharp
// Single source of truth
public class PermissionService : IPermissionService
{
    public async Task<List<string>> GetPermissionsAsync(Guid userId, Guid companyId)
    {
        // Same logic for API, Web, Mobile, etc.
    }
}
```

### 3. **Independent Testing**
```csharp
// Unit test without API dependencies
public class TenantServiceTests
{
    [Fact]
    public void GetCurrentCompanyId_FromHeader_ReturnsCompanyId()
    {
        // Arrange
        var mockHttpContext = CreateMockHttpContext();
        var tenantService = new TenantService(mockHttpContext, ...);
        
        // Act & Assert
    }
}
```

### 4. **Clean Dependencies**
```
Authorization Project depends on:
  ├── Domain (entities, interfaces)
  └── ASP.NET Core abstractions (HTTP context)

API Project depends on:
  ├── Authorization (for security)
  ├── Application (for business logic)
  └── Infrastructure (for data access)

Web Project depends on:
  ├── Authorization (for security)
  └── Application (for business logic)
```

---

## 🔧 Implementation Plan

### Phase 1: Create Authorization Project ✅
```bash
dotnet new classlib -n LinhGo.ERP.Authorization
dotnet sln add LinhGo.ERP.Authorization
```

### Phase 2: Move Core Components
1. Move `TenantService` → Authorization/Services/
2. Move authorization filters → Authorization/Filters/
3. Update namespaces

### Phase 3: Create Extensions
```csharp
// Authorization/Extensions/AuthorizationServiceCollectionExtensions.cs
public static class AuthorizationServiceCollectionExtensions
{
    public static IServiceCollection AddMultiTenantAuthorization(
        this IServiceCollection services)
    {
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IPermissionService, PermissionService>();
        
        // Register authorization policies
        services.AddAuthorizationBuilder()
            .AddPolicy("RequireCompanyAccess", policy => { })
            .AddPolicy("CanManageUsers", policy => { });
            
        return services;
    }
}
```

### Phase 4: Update API and Web Projects
```csharp
// In API/Program.cs
builder.Services.AddMultiTenantAuthorization();

// In Web/Program.cs
builder.Services.AddMultiTenantAuthorization();
```

### Phase 5: Add Unit Tests
```bash
dotnet new xunit -n LinhGo.ERP.Authorization.Tests
```

---

## 📊 Before vs After Comparison

### Before (Coupled)
```
API Project (700 files)
├── Controllers/
├── Services/
│   ├── TenantService.cs (authorization logic) ❌
│   └── Other services
├── Filters/
│   ├── RequireCompanyAccessAttribute.cs ❌
│   └── Other filters
└── ...

Web Project
└── No authorization - must duplicate or call API ❌
```

### After (Separated)
```
Authorization Project (20 files) ⭐
├── Services/
│   ├── TenantService.cs ✅
│   └── PermissionService.cs ✅
├── Filters/
│   ├── RequireCompanyAccessAttribute.cs ✅
│   └── RequirePermissionAttribute.cs ✅
└── Extensions/

API Project
├── Reference: LinhGo.ERP.Authorization ✅
└── Uses: [RequireCompanyAccess] ✅

Web Project
├── Reference: LinhGo.ERP.Authorization ✅
└── Uses: @attribute [RequireCompanyAccess] ✅
```

---

## 🎨 Example Usage After Separation

### In API Controller
```csharp
using LinhGo.ERP.Authorization.Filters;
using LinhGo.ERP.Authorization.Services;

[Authorize]
[RequireCompanyAccess]
public class TransactionsController : BaseApiController
{
    private readonly ITenantService _tenantService;
    
    [RequirePermission("transactions.create")]
    public async Task<IActionResult> Create(CreateDto dto)
    {
        var companyId = _tenantService.GetCurrentCompanyId();
        // ...
    }
}
```

### In Blazor Web Page
```razor
@using LinhGo.ERP.Authorization.Filters
@using LinhGo.ERP.Authorization.Services
@inject ITenantService TenantService

@attribute [Authorize]
@attribute [RequireCompanyAccess]

<AuthorizeView Policy="CanManageUsers">
    <Authorized>
        <button @onclick="DeleteUser">Delete</button>
    </Authorized>
    <NotAuthorized>
        <p>You don't have permission</p>
    </NotAuthorized>
</AuthorizeView>

@code {
    protected override async Task OnInitializedAsync()
    {
        var companyId = TenantService.GetCurrentCompanyId();
        // Load data for current company
    }
}
```

### In Background Job
```csharp
using LinhGo.ERP.Authorization.Services;

public class ReportGenerationJob
{
    private readonly IPermissionService _permissionService;
    
    public async Task GenerateAsync(Guid userId, Guid companyId)
    {
        // Check if user has permission in company
        var permissions = await _permissionService
            .GetPermissionsAsync(userId, companyId);
            
        if (permissions.Contains("reports.generate"))
        {
            // Generate report
        }
    }
}
```

---

## ✅ Advantages Summary

| Aspect | Before (Coupled) | After (Separated) |
|--------|------------------|-------------------|
| **Reusability** | ❌ API only | ✅ API, Web, Mobile, Jobs |
| **Testability** | ❌ Needs full API stack | ✅ Unit test independently |
| **Maintainability** | ❌ Mixed concerns | ✅ Clear separation |
| **Scalability** | ❌ Hard to extend | ✅ Easy to add features |
| **Dependencies** | ❌ Circular risks | ✅ Clean layering |
| **Team Work** | ❌ Conflicts in API | ✅ Separate project |

---

## 🚀 Migration Strategy

### Step 1: Create Project (Done ✅)
```bash
dotnet new classlib -n LinhGo.ERP.Authorization
dotnet sln add LinhGo.ERP.Authorization
```

### Step 2: Move Services (Next)
- Move `TenantService.cs`
- Move `ITenantService.cs`
- Update namespaces to `LinhGo.ERP.Authorization.Services`

### Step 3: Move Filters (Next)
- Move all authorization filter attributes
- Update namespaces to `LinhGo.ERP.Authorization.Filters`

### Step 4: Create Extensions (Next)
- Create `AddMultiTenantAuthorization()` extension
- Register all services

### Step 5: Update References (Next)
- API: Add reference to Authorization
- Web: Add reference to Authorization
- Update `using` statements

### Step 6: Test Everything
- Ensure API still works
- Ensure all authorization checks pass
- Run integration tests

---

## 📝 Recommendation

**YES, absolutely separate authorization into its own project!**

For an enterprise ERP system with:
- ✅ Multiple clients (API, Web, future Mobile)
- ✅ Complex multi-tenant authorization
- ✅ Role and permission-based access control
- ✅ Growing team and codebase

Separation provides:
- 🎯 **Better Architecture** - Clean layers, clear boundaries
- 🎯 **Reusability** - Share across all projects
- 🎯 **Testability** - Unit test authorization logic
- 🎯 **Maintainability** - Easy to find and update
- 🎯 **Scalability** - Add features without affecting other layers

**Next Steps:**
1. ✅ Project created
2. ⏭️ Move TenantService
3. ⏭️ Move authorization filters
4. ⏭️ Create extension methods
5. ⏭️ Update API and Web to use new project
6. ⏭️ Add unit tests

Would you like me to proceed with the migration?

