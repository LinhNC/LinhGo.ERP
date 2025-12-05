# User-Company Assignment & Permission Management

## Overview

The system now has complete repository support for managing user-company assignments and granular permissions. This enables:
- Assigning users to multiple companies
- Setting roles per user-company
- Granting/revoking granular permissions
- Checking user permissions for authorization

---

## New Repositories Added

### 1. IUserCompanyRepository
**Purpose**: Manage user-company assignments and roles

**Interface Location**: `Domain/Users/Interfaces/IUserCompanyRepository.cs`  
**Implementation Location**: `Infrastructure/Repositories/UserCompanyRepository.cs`

### 2. IUserPermissionRepository
**Purpose**: Manage granular permissions for user-company assignments

**Interface Location**: `Domain/Users/Interfaces/IUserPermissionRepository.cs`  
**Implementation Location**: `Infrastructure/Repositories/UserPermissionRepository.cs`

---

## IUserCompanyRepository Methods

### Query Methods

```csharp
// Get user-company assignment
Task<UserCompany?> GetByUserAndCompanyAsync(Guid userId, Guid companyId)

// Get all companies for a user
Task<IEnumerable<UserCompany>> GetByUserIdAsync(Guid userId)

// Get all users in a company
Task<IEnumerable<UserCompany>> GetByCompanyIdAsync(Guid companyId)

// Get active companies for a user
Task<IEnumerable<UserCompany>> GetActiveByUserIdAsync(Guid userId)

// Get active users in a company
Task<IEnumerable<UserCompany>> GetActiveByCompanyIdAsync(Guid companyId)

// Check if user is assigned to company
Task<bool> IsUserAssignedToCompanyAsync(Guid userId, Guid companyId)

// Get assignment with permissions loaded
Task<UserCompany?> GetWithPermissionsAsync(Guid userId, Guid companyId)
```

### Command Methods

```csharp
// Assign user to company with role
Task AssignUserToCompanyAsync(Guid userId, Guid companyId, string role)

// Remove user from company (soft delete)
Task RemoveUserFromCompanyAsync(Guid userId, Guid companyId)

// Update user's role in company
Task UpdateRoleAsync(Guid userId, Guid companyId, string role)
```

---

## IUserPermissionRepository Methods

### Query Methods

```csharp
// Get all permissions for a user-company
Task<IEnumerable<UserPermission>> GetByUserCompanyIdAsync(Guid userCompanyId)

// Get permissions by user and company
Task<IEnumerable<UserPermission>> GetByUserAndCompanyAsync(Guid userId, Guid companyId)

// Get specific permission
Task<UserPermission?> GetByPermissionKeyAsync(Guid userCompanyId, string permissionKey)

// Check if user has permission
Task<bool> HasPermissionAsync(Guid userId, Guid companyId, string permissionKey)

// Get all permission keys for user in company
Task<IEnumerable<string>> GetUserPermissionKeysAsync(Guid userId, Guid companyId)
```

### Command Methods

```csharp
// Grant single permission
Task GrantPermissionAsync(Guid userCompanyId, string permissionKey)

// Revoke single permission
Task RevokePermissionAsync(Guid userCompanyId, string permissionKey)

// Grant multiple permissions
Task GrantPermissionsAsync(Guid userCompanyId, IEnumerable<string> permissionKeys)

// Revoke all permissions
Task RevokeAllPermissionsAsync(Guid userCompanyId)
```

---

## Usage Examples

### 1. Assign User to Company

```csharp
public class UserManagementService
{
    private readonly IUserCompanyRepository _userCompanyRepo;

    public async Task AssignUserToCompany(Guid userId, Guid companyId, string role)
    {
        // Check if already assigned
        var existing = await _userCompanyRepo.IsUserAssignedToCompanyAsync(userId, companyId);
        
        if (!existing)
        {
            // Assign with role (Admin, Manager, User, Viewer)
            await _userCompanyRepo.AssignUserToCompanyAsync(userId, companyId, role);
        }
        else
        {
            // Update role if already assigned
            await _userCompanyRepo.UpdateRoleAsync(userId, companyId, role);
        }
    }
}
```

### 2. Grant Permissions to User

```csharp
public class PermissionService
{
    private readonly IUserCompanyRepository _userCompanyRepo;
    private readonly IUserPermissionRepository _permissionRepo;

    public async Task SetupUserPermissions(Guid userId, Guid companyId, string[] permissions)
    {
        // Get user-company assignment
        var userCompany = await _userCompanyRepo.GetByUserAndCompanyAsync(userId, companyId);
        
        if (userCompany != null)
        {
            // Grant multiple permissions
            await _permissionRepo.GrantPermissionsAsync(userCompany.Id, permissions);
        }
    }
}
```

### 3. Check User Permissions (Authorization)

```csharp
public class AuthorizationService
{
    private readonly IUserPermissionRepository _permissionRepo;

    public async Task<bool> CanAccessFeature(Guid userId, Guid companyId, string feature)
    {
        // Check if user has permission
        return await _permissionRepo.HasPermissionAsync(userId, companyId, feature);
    }

    public async Task<IEnumerable<string>> GetUserPermissions(Guid userId, Guid companyId)
    {
        // Get all permissions for authorization
        return await _permissionRepo.GetUserPermissionKeysAsync(userId, companyId);
    }
}
```

### 4. Get User's Companies

```csharp
public class UserService
{
    private readonly IUserCompanyRepository _userCompanyRepo;

    public async Task<IEnumerable<CompanyDto>> GetUserCompanies(Guid userId)
    {
        // Get all active companies user has access to
        var userCompanies = await _userCompanyRepo.GetActiveByUserIdAsync(userId);
        
        return userCompanies.Select(uc => new CompanyDto
        {
            Id = uc.Company.Id,
            Name = uc.Company.Name,
            Code = uc.Company.Code,
            Role = uc.Role,
            IsDefault = uc.IsDefault
        });
    }
}
```

### 5. Manage Company Users

```csharp
public class CompanyUserService
{
    private readonly IUserCompanyRepository _userCompanyRepo;
    private readonly IUserPermissionRepository _permissionRepo;

    public async Task<IEnumerable<UserDto>> GetCompanyUsers(Guid companyId)
    {
        // Get all active users in company
        var userCompanies = await _userCompanyRepo.GetActiveByCompanyIdAsync(companyId);
        
        return userCompanies.Select(uc => new UserDto
        {
            Id = uc.User.Id,
            Name = $"{uc.User.FirstName} {uc.User.LastName}",
            Email = uc.User.Email,
            Role = uc.Role,
            IsActive = uc.IsActive
        });
    }

    public async Task RemoveUserFromCompany(Guid userId, Guid companyId)
    {
        // Get user-company assignment
        var userCompany = await _userCompanyRepo.GetByUserAndCompanyAsync(userId, companyId);
        
        if (userCompany != null)
        {
            // Revoke all permissions first
            await _permissionRepo.RevokeAllPermissionsAsync(userCompany.Id);
            
            // Then remove user from company (soft delete)
            await _userCompanyRepo.RemoveUserFromCompanyAsync(userId, companyId);
        }
    }
}
```

### 6. Complete User Setup Workflow

```csharp
public class UserOnboardingService
{
    private readonly IUserRepository _userRepo;
    private readonly IUserCompanyRepository _userCompanyRepo;
    private readonly IUserPermissionRepository _permissionRepo;

    public async Task OnboardUser(CreateUserRequest request)
    {
        // 1. Create user
        var user = new User
        {
            Email = request.Email,
            UserName = request.UserName,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = HashPassword(request.Password)
        };
        user = await _userRepo.AddAsync(user);

        // 2. Assign to company
        await _userCompanyRepo.AssignUserToCompanyAsync(
            user.Id, 
            request.CompanyId, 
            request.Role
        );

        // 3. Get user-company assignment
        var userCompany = await _userCompanyRepo.GetByUserAndCompanyAsync(
            user.Id, 
            request.CompanyId
        );

        // 4. Grant default permissions based on role
        var permissions = GetDefaultPermissionsForRole(request.Role);
        await _permissionRepo.GrantPermissionsAsync(userCompany!.Id, permissions);
    }

    private string[] GetDefaultPermissionsForRole(string role)
    {
        return role switch
        {
            "Admin" => new[] 
            { 
                "users.view", "users.create", "users.edit", "users.delete",
                "customers.view", "customers.create", "customers.edit", "customers.delete",
                "products.view", "products.create", "products.edit", "products.delete",
                "orders.view", "orders.create", "orders.edit", "orders.delete",
                "inventory.view", "inventory.adjust",
                "reports.view"
            },
            "Manager" => new[] 
            { 
                "customers.view", "customers.create", "customers.edit",
                "products.view", "products.create", "products.edit",
                "orders.view", "orders.create", "orders.edit",
                "inventory.view",
                "reports.view"
            },
            "User" => new[] 
            { 
                "customers.view", "customers.create",
                "products.view",
                "orders.view", "orders.create",
                "inventory.view"
            },
            "Viewer" => new[] 
            { 
                "customers.view",
                "products.view",
                "orders.view",
                "inventory.view"
            },
            _ => Array.Empty<string>()
        };
    }
}
```

---

## Permission Key Conventions

Recommended permission key format: `{module}.{action}`

### Examples:

**Customer Module**
- `customers.view` - View customers
- `customers.create` - Create customers
- `customers.edit` - Edit customers
- `customers.delete` - Delete customers

**Product Module**
- `products.view` - View products
- `products.create` - Create products
- `products.edit` - Edit products
- `products.delete` - Delete products
- `products.import` - Import products

**Order Module**
- `orders.view` - View orders
- `orders.create` - Create orders
- `orders.edit` - Edit orders
- `orders.delete` - Delete orders
- `orders.confirm` - Confirm orders
- `orders.cancel` - Cancel orders

**Inventory Module**
- `inventory.view` - View inventory
- `inventory.adjust` - Adjust stock levels
- `inventory.transfer` - Transfer between warehouses

**User Module**
- `users.view` - View users
- `users.create` - Create users
- `users.edit` - Edit users
- `users.delete` - Delete users
- `users.assign` - Assign users to companies

**Report Module**
- `reports.view` - View reports
- `reports.export` - Export reports
- `reports.sales` - View sales reports
- `reports.inventory` - View inventory reports

---

## Authorization Middleware Example

```csharp
public class PermissionAuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public async Task InvokeAsync(
        HttpContext context, 
        IUserPermissionRepository permissionRepo)
    {
        // Get current user and company from context
        var userId = context.User.GetUserId();
        var companyId = context.User.GetCompanyId();

        // Get required permission from endpoint metadata
        var endpoint = context.GetEndpoint();
        var requiredPermission = endpoint?.Metadata
            .GetMetadata<RequirePermissionAttribute>()?.Permission;

        if (!string.IsNullOrEmpty(requiredPermission))
        {
            // Check if user has permission
            var hasPermission = await permissionRepo.HasPermissionAsync(
                userId, 
                companyId, 
                requiredPermission
            );

            if (!hasPermission)
            {
                context.Response.StatusCode = 403; // Forbidden
                await context.Response.WriteAsync("Access denied");
                return;
            }
        }

        await _next(context);
    }
}

// Custom attribute
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequirePermissionAttribute : Attribute
{
    public string Permission { get; }
    
    public RequirePermissionAttribute(string permission)
    {
        Permission = permission;
    }
}

// Usage in controller
[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    [HttpGet]
    [RequirePermission("customers.view")]
    public async Task<IActionResult> GetCustomers() { }

    [HttpPost]
    [RequirePermission("customers.create")]
    public async Task<IActionResult> CreateCustomer() { }
}
```

---

## Database Relationships

The system maintains these relationships:

```
User (1) ←→ (N) UserCompany (N) ←→ (1) Company
                     ↓
                    (1)
                     ↓
                    (N)
              UserPermission
```

- **User** can have multiple **UserCompany** assignments
- **Company** can have multiple **UserCompany** assignments  
- **UserCompany** can have multiple **UserPermission** entries
- When **UserCompany** is deleted, all **UserPermission** are cascade deleted
- When **User** is deleted, all **UserCompany** are cascade deleted

---

## Complete Repository List

### ✅ Now Complete - 13 Repositories

1. **ICompanyRepository** - Company management
2. **IUserRepository** - User management
3. **IUserCompanyRepository** ⭐ NEW - User-company assignments
4. **IUserPermissionRepository** ⭐ NEW - Permission management
5. **ICustomerRepository** - Customer management
6. **IProductRepository** - Product management
7. **IStockRepository** - Stock levels
8. **IWarehouseRepository** - Warehouses
9. **IInventoryTransactionRepository** - Stock movements
10. **IOrderRepository** - Orders
11. **IUnitOfWork** - Transaction management
12. **IRepository<T>** - Generic repository
13. **ITenantRepository<T>** - Tenant-aware repository

---

## Summary

✅ **User-Company Assignment** - Fully supported  
✅ **Role Management** - Per company roles  
✅ **Permission Management** - Granular permissions  
✅ **Multi-Tenancy** - Users access multiple companies  
✅ **Authorization** - Permission-based access control  
✅ **Soft Delete** - Assignments can be deactivated  
✅ **Cascade Rules** - Automatic cleanup of permissions  

The system is now complete with full user-company assignment and permission management capabilities!

