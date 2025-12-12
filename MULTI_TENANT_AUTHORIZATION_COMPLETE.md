# ✅ Multi-Tenant Authorization Implementation Complete!

## Summary

Implemented comprehensive multi-tenant authorization system for the ERP where users can belong to multiple companies with different roles and permissions in each company.

---

## 🎯 Multi-Tenancy Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                        User Login                            │
│                                                               │
│  User: john@example.com                                      │
│  Companies:                                                   │
│    - Company A (Role: Admin)                                 │
│    - Company B (Role: Manager)                               │
│    - Company C (Role: Employee, Default)                     │
└───────────────────────┬─────────────────────────────────────┘
                        │
                        ↓
┌─────────────────────────────────────────────────────────────┐
│                    JWT Token Generated                       │
│                                                               │
│  Claims:                                                      │
│    - sub: userId                                             │
│    - email: john@example.com                                 │
│    - default_company_id: Company C ID                        │
│    - roles: [Admin, Manager, Employee]  (all companies)     │
└───────────────────────┬─────────────────────────────────────┘
                        │
                        ↓
┌─────────────────────────────────────────────────────────────┐
│                  API Request with Context                    │
│                                                               │
│  Option 1: Header                                            │
│    X-Company-Id: Company A ID                                │
│                                                               │
│  Option 2: Route Parameter                                   │
│    /api/v1/companies/{companyId}/users                      │
│                                                               │
│  Option 3: Default Company (from JWT)                        │
│    Uses default_company_id from token                        │
└───────────────────────┬─────────────────────────────────────┘
                        │
                        ↓
┌─────────────────────────────────────────────────────────────┐
│              TenantService Validates Access                  │
│                                                               │
│  1. Get company ID from header/route/default                 │
│  2. Query UserCompany table                                  │
│  3. Check: User has active assignment to company?            │
│  4. Get: User's role in that specific company                │
│  5. Map: Role → Permissions for that company                 │
│  6. Authorize: Permission check for action                   │
└─────────────────────────────────────────────────────────────┘
```

---

## 📦 Components Implemented

### 1. **TenantService**
**File:** `/Api/Services/TenantService.cs`

**Purpose:** Manage company-specific context and authorization

**Key Methods:**
```csharp
Guid? GetCurrentCompanyId()
  ↓ Checks in order:
    1. X-Company-Id header
    2. Route parameter (companyId)
    3. Default company from JWT claims
    
Task<bool> HasAccessToCompanyAsync(Guid companyId)
  ↓ Validates:
    - User has active UserCompany record
    - IsActive = true
    
Task<string?> GetUserRoleInCompanyAsync(Guid companyId)
  ↓ Returns user's role in specific company
  
Task<List<string>> GetUserPermissionsInCompanyAsync(Guid companyId)
  ↓ Returns permissions based on role in company
```

### 2. **Multi-Tenant Authorization Filters**
**File:** `/Api/Filters/TenantAuthorizationFilters.cs`

#### `[RequireCompanyAccess]`
Ensures user has access to the company in context

```csharp
[Authorize]
[RequireCompanyAccess]
public class CompaniesController : BaseApiController
{
    // All endpoints require company access
}
```

#### `[RequirePermission("permission.name")]`
Requires specific permission in current company

```csharp
[RequirePermission("transactions.create")]
public async Task<IActionResult> CreateTransaction(CreateTransactionDto dto)
{
    // User must have "transactions.create" permission in current company
}
```

#### `[RequireCompanyRole("Admin", "Manager")]`
Requires specific role in current company

```csharp
[RequireCompanyRole("Admin", "Manager")]
public async Task<IActionResult> DeleteUser(Guid id)
{
    // User must be Admin OR Manager in current company
}
```

### 3. **Updated JWT Token**
**File:** `/Api/Services/JwtTokenService.cs`

**New Claims:**
- `default_company_id` - User's default company for context
- Roles are aggregated from all companies
- Permissions are checked per-company at authorization time

---

## 🔐 Authorization Levels

### Level 1: Authentication
```csharp
[Authorize]
```
- User must be logged in
- Valid JWT token required

### Level 2: Company Access
```csharp
[Authorize]
[RequireCompanyAccess]
```
- User must be authenticated
- User must have active assignment to the company in context

### Level 3: Company-Specific Role
```csharp
[Authorize]
[RequireCompanyAccess]
[RequireCompanyRole("Admin")]
```
- User must be authenticated
- User must have access to company
- User must have Admin role **in that specific company**

### Level 4: Company-Specific Permission
```csharp
[Authorize]
[RequireCompanyAccess]
[RequirePermission("transactions.delete")]
```
- User must be authenticated
- User must have access to company
- User must have "transactions.delete" permission **in that company**

---

## 🚀 Usage Examples

### Example 1: User with Multiple Companies

**User Setup:**
```sql
-- User: john@example.com
-- UserCompanies:
INSERT INTO user_companies (user_id, company_id, role, is_default_company) VALUES
  (john_id, company_a_id, 'Admin', false),
  (john_id, company_b_id, 'Manager', false),
  (john_id, company_c_id, 'Employee', true);  -- Default
```

**Login Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "john_id",
    "email": "john@example.com",
    "roles": ["Admin", "Manager", "Employee"]
  }
}

// JWT Claims include:
// - default_company_id: company_c_id
// - roles: [Admin, Manager, Employee]
```

**API Requests:**

#### Request 1: To Company A (as Admin)
```bash
GET /api/v1/companies/company_a_id/users
Headers:
  Authorization: Bearer <token>
  X-Company-Id: company_a_id

# Authorization Flow:
# 1. ✅ Authenticated (JWT valid)
# 2. ✅ Has access (UserCompany exists for company_a)
# 3. ✅ Role in company_a: Admin
# 4. ✅ Permissions: [users.view, users.create, users.update, users.delete, ...]
# Result: 200 OK
```

#### Request 2: To Company B (as Manager)
```bash
POST /api/v1/companies/company_b_id/transactions
Headers:
  Authorization: Bearer <token>
Body: { "amount": 1000, ... }

# Authorization Flow:
# 1. ✅ Authenticated
# 2. ✅ Has access to company_b
# 3. ✅ Role in company_b: Manager
# 4. ✅ Has permission: transactions.create
# Result: 201 Created
```

#### Request 3: To Company D (No Access)
```bash
GET /api/v1/companies/company_d_id/users
Headers:
  Authorization: Bearer <token>

# Authorization Flow:
# 1. ✅ Authenticated
# 2. ❌ No access to company_d (no UserCompany record)
# Result: 403 Forbidden
```

#### Request 4: Delete Transaction in Company C (Employee - No Permission)
```bash
DELETE /api/v1/companies/company_c_id/transactions/transaction_id
Headers:
  Authorization: Bearer <token>

# Authorization Flow:
# 1. ✅ Authenticated
# 2. ✅ Has access to company_c
# 3. ✅ Role in company_c: Employee
# 4. ❌ Employee role doesn't have "transactions.delete" permission
# Result: 403 Forbidden
```

---

## 🎨 Controller Implementation Examples

### Example 1: Basic Multi-Tenant Controller
```csharp
[Authorize]
[RequireCompanyAccess]
[Route("api/v{version:apiVersion}/companies/{companyId:guid}/transactions")]
public class TransactionsController : BaseApiController
{
    private readonly ITenantService _tenantService;
    
    // All endpoints automatically scoped to company from route
    
    [HttpGet]
    public async Task<IActionResult> GetTransactions()
    {
        var companyId = _tenantService.GetCurrentCompanyId();
        // companyId is guaranteed to be set and user has access
        
        var transactions = await _service.GetTransactionsByCompanyAsync(companyId.Value);
        return Ok(transactions);
    }
    
    [HttpPost]
    [RequirePermission("transactions.create")]
    public async Task<IActionResult> CreateTransaction(CreateTransactionDto dto)
    {
        var companyId = _tenantService.GetCurrentCompanyId();
        dto.CompanyId = companyId.Value; // Enforce company context
        
        var result = await _service.CreateTransactionAsync(dto);
        return ToResponse(result);
    }
    
    [HttpDelete("{id:guid}")]
    [RequirePermission("transactions.delete")]
    public async Task<IActionResult> DeleteTransaction(Guid id)
    {
        var companyId = _tenantService.GetCurrentCompanyId();
        
        // Verify transaction belongs to this company before deleting
        var transaction = await _service.GetByIdAsync(id);
        if (transaction.CompanyId != companyId.Value)
        {
            return NotFound(); // Don't leak info about other companies
        }
        
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
```

### Example 2: Role-Specific Endpoints
```csharp
[Authorize]
[RequireCompanyAccess]
public class CompanySettingsController : BaseApiController
{
    // Anyone with company access can view settings
    [HttpGet]
    public async Task<IActionResult> GetSettings()
    {
        var companyId = _tenantService.GetCurrentCompanyId();
        var settings = await _service.GetSettingsAsync(companyId.Value);
        return Ok(settings);
    }
    
    // Only Admin and Manager can update settings
    [HttpPut]
    [RequireCompanyRole("Admin", "Manager")]
    public async Task<IActionResult> UpdateSettings(UpdateSettingsDto dto)
    {
        var companyId = _tenantService.GetCurrentCompanyId();
        var result = await _service.UpdateSettingsAsync(companyId.Value, dto);
        return ToResponse(result);
    }
    
    // Only Admin can delete company data
    [HttpDelete("data")]
    [RequireCompanyRole("Admin")]
    public async Task<IActionResult> DeleteCompanyData()
    {
        var companyId = _tenantService.GetCurrentCompanyId();
        await _service.DeleteCompanyDataAsync(companyId.Value);
        return NoContent();
    }
}
```

### Example 3: Mixed Authorization
```csharp
[Authorize]
public class ReportsController : BaseApiController
{
    // Generate report - requires company context and permission
    [HttpPost("companies/{companyId:guid}/reports")]
    [RequireCompanyAccess]
    [RequirePermission("reports.create")]
    public async Task<IActionResult> GenerateReport(
        Guid companyId,
        GenerateReportDto dto)
    {
        dto.CompanyId = companyId;
        var result = await _service.GenerateReportAsync(dto);
        return ToResponse(result);
    }
    
    // View own reports - no company access required
    [HttpGet("my-reports")]
    public async Task<IActionResult> GetMyReports()
    {
        var userId = GetCurrentUserId();
        var reports = await _service.GetReportsByUserAsync(userId);
        return Ok(reports);
    }
    
    // System-wide report - only for Super Admin (not company-specific)
    [HttpGet("system/stats")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetSystemStats()
    {
        var stats = await _service.GetSystemStatsAsync();
        return Ok(stats);
    }
}
```

---

## 📋 Permission Mapping

**Current Implementation (in TenantService):**

| Role | Permissions |
|------|-------------|
| **Admin** | users.*, companies.*, transactions.*, reports.*, settings.*, audit.view |
| **Manager** | users.view, companies.view, transactions.[view,create,update], reports.[view,export] |
| **Accountant** | transactions.[view,create,update], reports.[view,export] |
| **Employee** | transactions.view, reports.view |
| **Viewer** | companies.view, reports.view |

**TODO: Move to Database**
```sql
CREATE TABLE permissions (
    id UUID PRIMARY KEY,
    name VARCHAR(100) UNIQUE NOT NULL,
    description TEXT,
    resource VARCHAR(50),
    action VARCHAR(50)
);

CREATE TABLE role_permissions (
    role_id UUID REFERENCES roles(id),
    permission_id UUID REFERENCES permissions(id),
    company_id UUID REFERENCES companies(id), -- Company-specific permission override
    PRIMARY KEY (role_id, permission_id, company_id)
);
```

---

## 🔍 Company Context Resolution Order

```
1. X-Company-Id Header (Highest Priority)
   ↓ Allows client to explicitly specify company
   ↓ Example: X-Company-Id: 550e8400-e29b-41d4-a716-446655440001
   
2. Route Parameter (companyId)
   ↓ Automatic from route like /companies/{companyId}/...
   ↓ Example: /api/v1/companies/550e8400-.../users
   
3. Default Company from JWT
   ↓ Falls back to user's default company
   ↓ Claim: default_company_id
   
4. No Context → 401 Unauthorized
   ↓ If none of above available and company context required
```

---

## 🛡️ Security Best Practices Applied

### ✅ 1. **Data Isolation**
Every transaction/record belongs to a company:
```csharp
// Enforce company context on create
dto.CompanyId = _tenantService.GetCurrentCompanyId();

// Verify on read/update/delete
if (entity.CompanyId != currentCompanyId)
{
    return NotFound(); // Don't expose existence
}
```

### ✅ 2. **Least Privilege**
Users only get permissions they need in each company:
```csharp
// User is Admin in Company A, but Employee in Company B
// Company A: Can delete users
// Company B: Cannot delete users
```

### ✅ 3. **Explicit Authorization**
Never assume company access:
```csharp
// ❌ Bad
var data = await _service.GetDataAsync();

// ✅ Good
[RequireCompanyAccess]
var companyId = _tenantService.GetCurrentCompanyId();
var data = await _service.GetDataByCompanyAsync(companyId.Value);
```

### ✅ 4. **Audit Trail**
Log company context in all operations:
```csharp
_logger.LogInformation(
    "User {UserId} performed {Action} in Company {CompanyId}",
    userId, action, companyId);
```

---

## ✅ Summary

✅ **TenantService** - Company context management  
✅ **RequireCompanyAccess** - Company-level authorization  
✅ **RequirePermission** - Permission-based authorization  
✅ **RequireCompanyRole** - Role-based authorization  
✅ **JWT with Default Company** - Seamless context switching  
✅ **Multi-Source Company ID** - Header/Route/Default  
✅ **Per-Company Permissions** - Isolated authorization  
✅ **Data Isolation** - Company-scoped queries  
✅ **Security Best Practices** - Least privilege, explicit checks  

**Your ERP now has enterprise-grade multi-tenant authorization!** 🏢🔐✨

