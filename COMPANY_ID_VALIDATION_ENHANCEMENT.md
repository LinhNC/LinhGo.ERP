# ✅ Multi-Tenant Company ID Validation Enhanced

## Summary

Enhanced the multi-tenant authorization filters to **enforce strict validation** that the company ID in the route parameter matches the company context (from header/default), preventing unauthorized cross-company access attempts.

---

## 🔐 Security Enhancement

### What Was Fixed

**Before:** Authorization filters only checked if user had access to the resolved company ID, but didn't validate it matched the route parameter.

**Attack Scenario Prevented:**
```bash
# User belongs to Company A and Company B
# Route says Company A, but user provides header for Company B
GET /api/v1/companies/company-a-id/users
Headers:
  X-Company-Id: company-b-id  # ← Mismatch!
  Authorization: Bearer <token>

# OLD: Would check access to company-b, potentially exposing company-a data
# NEW: Returns 403 Forbidden - Company context mismatch
```

**After:** All three authorization filters now validate:
1. ✅ Company ID exists in context
2. ✅ If route has `{companyId}`, it **MUST match** the resolved company ID
3. ✅ User has active access to the validated company
4. ✅ User has required permission/role in that company

---

## 🛡️ Validation Flow

```
Request: GET /api/v1/companies/{companyId}/users
Headers: X-Company-Id: {headerCompanyId}

┌─────────────────────────────────────────────────────────┐
│ Step 1: Resolve Company Context                         │
│ Priority: Header → Route → Default                       │
│ Result: resolvedCompanyId                               │
└────────────────┬────────────────────────────────────────┘
                 │
                 ↓
┌─────────────────────────────────────────────────────────┐
│ Step 2: Validate Route Match (NEW!)                     │
│ If route contains {companyId}:                          │
│   ✓ Parse route company ID                              │
│   ✓ Compare: resolvedCompanyId == routeCompanyId        │
│   ✗ If mismatch → 403 Forbidden                         │
└────────────────┬────────────────────────────────────────┘
                 │
                 ↓
┌─────────────────────────────────────────────────────────┐
│ Step 3: Verify User Has Access                          │
│ Query: UserCompany table                                │
│   ✓ User has active assignment to company?              │
│   ✗ If no access → 403 Forbidden                        │
└────────────────┬────────────────────────────────────────┘
                 │
                 ↓
┌─────────────────────────────────────────────────────────┐
│ Step 4: Check Permission/Role (if applicable)           │
│   ✓ User has required permission?                       │
│   ✓ User has required role?                             │
│   ✗ If insufficient → 403 Forbidden                     │
└────────────────┬────────────────────────────────────────┘
                 │
                 ↓
            ✅ AUTHORIZED
```

---

## 📋 Updated Filters

### 1. RequireCompanyAccessAttribute

**Validation Added:**
```csharp
// Get company ID from route parameter
if (context.HttpContext.Request.RouteValues.TryGetValue("companyId", out var routeCompanyId))
{
    Guid routeCompanyGuid;
    if (!Guid.TryParse(routeCompanyId?.ToString(), out routeCompanyGuid))
    {
        return BadRequest("Invalid company ID format");
    }

    // NEW: Ensure resolved ID matches route ID
    if (resolvedCompanyId.Value != routeCompanyGuid)
    {
        return 403 Forbidden("Company context mismatch");
    }
}

// Then check user has access
var hasAccess = await tenantService.HasAccessToCompanyAsync(resolvedCompanyId.Value);
```

### 2. RequirePermissionAttribute

**Same validation + Permission check:**
```csharp
// 1. Validate route matches resolved company ID
// 2. Verify user has access to company
// 3. Check user has required permission in that company
if (!permissions.Contains(_permission))
{
    return 403 Forbidden($"Missing permission: {_permission}");
}
```

### 3. RequireCompanyRoleAttribute

**Same validation + Role check:**
```csharp
// 1. Validate route matches resolved company ID
// 2. Verify user has access to company
// 3. Check user has required role in that company
if (!_roles.Contains(userRole))
{
    return 403 Forbidden($"Required role: {string.Join(", ", _roles)}");
}
```

---

## 🚀 Usage Examples

### Example 1: Valid Request (Company IDs Match)

```bash
GET /api/v1/companies/550e8400-e29b-41d4-a716-446655440001/users
Headers:
  X-Company-Id: 550e8400-e29b-41d4-a716-446655440001  ✅ Matches route
  Authorization: Bearer <token>

# Validation Flow:
# 1. ✅ Resolved company: 550e8400...001 (from header)
# 2. ✅ Route company: 550e8400...001
# 3. ✅ Match confirmed
# 4. ✅ User has access to company
# Result: 200 OK
```

### Example 2: Invalid Request (Company IDs Don't Match)

```bash
GET /api/v1/companies/550e8400-e29b-41d4-a716-446655440001/users
Headers:
  X-Company-Id: 650e8400-e29b-41d4-a716-446655440002  ❌ Different from route!
  Authorization: Bearer <token>

# Validation Flow:
# 1. ✅ Resolved company: 650e8400...002 (from header)
# 2. ✅ Route company: 550e8400...001
# 3. ❌ MISMATCH DETECTED!
# Result: 403 Forbidden
{
  "type": "Forbidden",
  "errors": {
    "Company": ["Company context mismatch. The company in the route must match the company context."]
  }
}
```

### Example 3: Valid Request (No Header, Uses Route)

```bash
GET /api/v1/companies/550e8400-e29b-41d4-a716-446655440001/users
Headers:
  Authorization: Bearer <token>
  # No X-Company-Id header

# Validation Flow:
# 1. ✅ Resolved company: 550e8400...001 (from route)
# 2. ✅ Route company: 550e8400...001
# 3. ✅ Match confirmed (same source)
# 4. ✅ User has access to company
# Result: 200 OK
```

### Example 4: Valid Request (No Header, Uses Default Company)

```bash
GET /api/v1/transactions/create
Headers:
  Authorization: Bearer <token>
  # No X-Company-Id header, no companyId in route

# Validation Flow:
# 1. ✅ Resolved company: 750e8400...003 (from JWT default_company_id)
# 2. ⚠️  No route company parameter (not applicable)
# 3. ✅ User has access to default company
# Result: 200 OK (uses default company context)
```

---

## 🎨 Controller Implementation

### Endpoints with Company ID in Route

```csharp
[Authorize]
[RequireCompanyAccess]  // Enforces route/context match
[Route("api/v{version:apiVersion}/companies/{companyId:guid}/users")]
public class UserCompaniesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetCompanyUsers(Guid companyId)
    {
        // companyId from route is GUARANTEED to:
        // 1. Match the X-Company-Id header (if provided)
        // 2. Be the company user has access to
        // 3. Be validated before reaching this code
        
        var users = await _service.GetUsersByCompanyAsync(companyId);
        return Ok(users);
    }
    
    [HttpPost]
    [RequirePermission("users.create")]
    public async Task<IActionResult> AddUserToCompany(
        Guid companyId,  // From route
        AddUserDto dto)
    {
        // Force company ID to match route parameter
        // This prevents clients from specifying different companyId in body
        dto.CompanyId = companyId;
        
        var result = await _service.AddUserAsync(dto);
        return ToResponse(result);
    }
}
```

### Endpoints WITHOUT Company ID in Route

```csharp
[Authorize]
[Route("api/v{version:apiVersion}/transactions")]
public class TransactionsController : BaseApiController
{
    private readonly ITenantService _tenantService;
    
    [HttpGet]
    public async Task<IActionResult> GetTransactions()
    {
        // Get company from header or default
        var companyId = _tenantService.GetCurrentCompanyId();
        
        if (!companyId.HasValue)
        {
            return BadRequest("Company context required");
        }
        
        var transactions = await _service.GetByCompanyAsync(companyId.Value);
        return Ok(transactions);
    }
    
    [HttpPost]
    [RequirePermission("transactions.create")]
    public async Task<IActionResult> CreateTransaction(CreateTransactionDto dto)
    {
        var companyId = _tenantService.GetCurrentCompanyId();
        
        // Enforce company context
        dto.CompanyId = companyId!.Value;
        
        var result = await _service.CreateAsync(dto);
        return ToResponse(result);
    }
}
```

---

## 🔍 Error Responses

### 1. Company Context Mismatch (403 Forbidden)
```json
{
  "type": "Forbidden",
  "errors": {
    "Company": ["Company context mismatch. The company in the route must match the company context."]
  }
}
```

**When:** X-Company-Id header doesn't match route `{companyId}` parameter

### 2. No Company Access (403 Forbidden)
```json
{
  "type": "Forbidden",
  "errors": {
    "Company": ["You do not have access to this company"]
  }
}
```

**When:** User doesn't have active UserCompany assignment

### 3. Missing Permission (403 Forbidden)
```json
{
  "type": "Forbidden",
  "errors": {
    "Permission": ["You do not have the required permission: transactions.delete"]
  }
}
```

**When:** User lacks required permission in that company

### 4. Missing Role (403 Forbidden)
```json
{
  "type": "Forbidden",
  "errors": {
    "Role": ["You do not have the required role. Required: Admin, Manager"]
  }
}
```

**When:** User doesn't have any of the required roles in that company

### 5. Invalid Company ID Format (400 Bad Request)
```json
{
  "type": "BadRequest",
  "errors": {
    "CompanyId": ["Invalid company ID format in route"]
  }
}
```

**When:** `{companyId}` in route is not a valid GUID

---

## ✅ Security Benefits

### ✅ 1. **Prevents Context Confusion**
Users can't trick the system by providing one company ID in header and accessing another company's route

### ✅ 2. **Enforces Explicit Authorization**
Every request is validated against the exact company being accessed

### ✅ 3. **Data Isolation**
Impossible to access Company A's data while authenticated for Company B

### ✅ 4. **Audit Trail Accuracy**
Company context in logs matches the actual company being accessed

### ✅ 5. **Clear Error Messages**
Developers get specific feedback about what went wrong

---

## 📊 Validation Matrix

| Header Company | Route Company | Result |
|----------------|---------------|--------|
| A | A | ✅ Pass → Check access |
| A | B | ❌ 403 Forbidden (mismatch) |
| B | A | ❌ 403 Forbidden (mismatch) |
| Not provided | A | ✅ Pass → Resolved from route |
| Not provided | Not provided | ✅ Pass → Uses default company |
| A | Not provided | ✅ Pass → Uses header company |

---

## 🎯 Best Practices

### ✅ DO: Use Route Parameter for Company-Specific Endpoints
```csharp
// Good: Company ID in route
[Route("api/v{version:apiVersion}/companies/{companyId:guid}/transactions")]
[RequireCompanyAccess]  // Enforces validation
```

### ✅ DO: Use Header for Global Endpoints
```csharp
// Good: Company from header/default
[Route("api/v{version:apiVersion}/transactions")]
[HttpPost]
public async Task<IActionResult> Create(CreateTransactionDto dto)
{
    var companyId = _tenantService.GetCurrentCompanyId();
    dto.CompanyId = companyId!.Value;
}
```

### ❌ DON'T: Mix Company Sources
```csharp
// Bad: Don't trust company ID from request body when route has it
[HttpPost("companies/{companyId}/transactions")]
public async Task<IActionResult> Create(Guid companyId, CreateTransactionDto dto)
{
    // ❌ Bad: Using dto.CompanyId
    await _service.CreateAsync(dto);
    
    // ✅ Good: Override with route parameter
    dto.CompanyId = companyId;
    await _service.CreateAsync(dto);
}
```

---

## ✅ Summary

✅ **Route/Context Validation** - Company IDs must match  
✅ **Security Enhanced** - Prevents cross-company attacks  
✅ **Clear Error Messages** - Developers know what's wrong  
✅ **Consistent Validation** - All three filters updated  
✅ **No Breaking Changes** - Existing valid requests work  
✅ **Data Isolation** - Strict company boundaries enforced  

**Your multi-tenant ERP now has bulletproof company context validation!** 🔒✨

