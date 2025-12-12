# ✅ Web Calls API Pattern - Final Architecture

## Summary

Refactored the Web project to follow best practices: **Web calls API for authentication** instead of accessing the database directly. Now API is the single source of truth!

---

## 🎯 Architecture Pattern

### ✅ Correct Pattern (Now Implemented):

```
┌─────────────────────────────────────────────────┐
│           LinhGo.ERP.Web                         │
│           (Blazor Server)                        │
│                                                  │
│  AccountController                               │
│       ↓                                          │
│  WebAuthenticationService                        │
│       ↓ HTTP POST                                │
│  /api/v1/auth/login                             │
└───────────────────┬─────────────────────────────┘
                    │ HTTP Request
                    ↓
┌─────────────────────────────────────────────────┐
│           LinhGo.ERP.Api                         │
│                                                  │
│  AuthController                                  │
│       ↓                                          │
│  IAuthenticationService                          │
│       ↓                                          │
│  Database (User validation)                      │
└──────────────────────────────────────────────────┘
```

**Benefits:**
- ✅ **Single source of truth** - API handles all auth logic
- ✅ **Consistent behavior** - Mobile, Desktop apps use same API
- ✅ **Separation of concerns** - Web is just a UI client
- ✅ **Easier to maintain** - Change API, all clients benefit
- ✅ **Better security** - Database access only through API

---

## 📦 What Changed

### 1. Created WebAuthenticationService ✅
**File:** `/LinhGo.ERP.Web/Services/WebAuthenticationService.cs`

**Purpose:** Calls API endpoints instead of accessing database directly

```csharp
public interface IWebAuthenticationService
{
    Task<WebAuthResult> LoginAsync(string email, string password, bool rememberMe);
    Task LogoutAsync();
}
```

**What it does:**
- ✅ Calls `POST /api/v1/auth/login` endpoint
- ✅ Receives JWT tokens from API
- ✅ Sets cookies for Blazor Server
- ✅ Signs in user with cookie authentication
- ✅ Calls `POST /api/v1/auth/logout` to invalidate tokens

### 2. Simplified AccountController ✅
**File:** `/LinhGo.ERP.Web/Controllers/AccountController.cs`

**Before:** 180+ lines with database logic
**After:** 88 lines, just calls WebAuthenticationService

```csharp
public class AccountController : Controller
{
    private readonly IWebAuthenticationService _webAuthService;
    
    [HttpPost("login")]
    public async Task<IActionResult> Login(...)
    {
        // Simply call API via service
        var result = await _webAuthService.LoginAsync(email, password, rememberMe);
        
        if (!result.IsSuccess)
            return Redirect("/login?error=...");
        
        return LocalRedirect(returnUrl ?? "/");
    }
}
```

### 3. Configured HttpClient ✅
**File:** `/LinhGo.ERP.Web/Program.cs`

```csharp
// Register Web Authentication Service
builder.Services.AddScoped<IWebAuthenticationService, WebAuthenticationService>();

// Configure HttpClient to call API
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri("https://localhost:5001");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
```

### 4. Added API Configuration ✅
**File:** `/LinhGo.ERP.Web/appsettings.json`

```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:5001"
  }
}
```

---

## 🔥 Request Flow

### Login Flow

```
1. User submits login form
   ↓
2. AccountController.Login(email, password, rememberMe)
   ↓
3. WebAuthenticationService.LoginAsync()
   ↓ HTTP POST /api/v1/auth/login
4. API AuthController.Login()
   ↓
5. API IAuthenticationService.AuthenticateAsync()
   ↓
6. Database: Validate user, get roles, generate tokens
   ↓
7. API returns: { accessToken, refreshToken, user }
   ↓
8. WebAuthenticationService:
   - Sets JWT cookies (access_token, refresh_token)
   - Signs in user with Cookie auth (for Blazor)
   ↓
9. AccountController redirects to home page
   ↓
10. User is authenticated!
```

### Logout Flow

```
1. User clicks logout
   ↓
2. AccountController.Logout()
   ↓
3. WebAuthenticationService.LogoutAsync()
   ↓ HTTP POST /api/v1/auth/logout (with Bearer token)
4. API AuthController.Logout()
   ↓
5. API invalidates refresh token in database
   ↓
6. WebAuthenticationService:
   - Deletes JWT cookies
   - Signs out from Cookie auth
   ↓
7. AccountController redirects to /login
```

---

## 📊 Two Controllers Explained

### Why Two Controllers?

**API Controller** (`AuthController`)
- **Purpose:** JSON API for ALL clients
- **Returns:** JSON responses
- **Clients:** Web, Mobile, Desktop, External apps
- **Location:** `/api/v1/auth/login`

**Web Controller** (`AccountController`)
- **Purpose:** Handle HTML form POSTs from Blazor
- **Returns:** Redirects (not JSON)
- **Clients:** Only Blazor Server forms
- **Location:** `/account/login`

### Why Not One Controller?

**Different requirements:**

| Aspect | API Controller | Web Controller |
|--------|----------------|----------------|
| **Input** | JSON body | HTML form |
| **Output** | JSON response | Redirect response |
| **Error Handling** | JSON error object | Query string error |
| **CSRF Protection** | Not needed (stateless) | Required (form POST) |
| **Content Type** | application/json | application/x-www-form-urlencoded |

**Example:**

```csharp
// API Controller - Returns JSON
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    return Ok(new LoginResponse { ... }); // JSON
}

// Web Controller - Returns Redirect
[HttpPost("login")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Login([FromForm] string email, [FromForm] string password)
{
    return LocalRedirect("/"); // Redirect
}
```

---

## 🎯 Best Practices Applied

### 1. **API as Single Source of Truth** ✅
- All authentication logic in API
- Web just consumes the API
- Mobile/Desktop will also consume same API

### 2. **No Database Access from Web** ✅
- Web doesn't reference Infrastructure directly (for auth)
- Web doesn't call repositories directly
- All data access through API

### 3. **HttpClient Best Practices** ✅
```csharp
// Named HttpClient with IHttpClientFactory
services.AddHttpClient("API", client => { ... });

// Injected in service
private readonly HttpClient _httpClient;
public WebAuthenticationService(IHttpClientFactory factory)
{
    _httpClient = factory.CreateClient("API");
}
```

### 4. **Configuration-Based API URL** ✅
```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:5001" // Dev
    // "BaseUrl": "https://api.production.com" // Prod
  }
}
```

### 5. **Proper Error Handling** ✅
```csharp
if (!response.IsSuccessStatusCode)
{
    return WebAuthResult.Failed("Invalid email or password");
}
```

---

## 💡 Code Comparison

### Before (Direct Database Access):

```csharp
// ❌ Web directly calls Authorization services
public class AccountController : Controller
{
    private readonly IAuthenticationService _authService;
    private readonly IUserRepository _userRepo; // Direct DB access!
    
    [HttpPost("login")]
    public async Task<IActionResult> Login(...)
    {
        // Directly authenticate against database
        var result = await _authService.AuthenticateAsync(email, password);
        // ...
    }
}
```

**Problems:**
- Web has database dependencies
- Duplicate auth logic in API and Web
- Mobile/Desktop would need same logic
- Hard to maintain consistency

### After (API Call):

```csharp
// ✅ Web calls API
public class AccountController : Controller
{
    private readonly IWebAuthenticationService _webAuthService;
    
    [HttpPost("login")]
    public async Task<IActionResult> Login(...)
    {
        // Call API endpoint
        var result = await _webAuthService.LoginAsync(email, password, rememberMe);
        
        if (!result.IsSuccess)
            return Redirect($"/login?error={result.ErrorMessage}");
        
        return LocalRedirect(returnUrl ?? "/");
    }
}
```

**Benefits:**
- ✅ Web is just a UI client
- ✅ No database dependencies
- ✅ API is single source of truth
- ✅ Easy to add Mobile/Desktop
- ✅ Consistent behavior

---

## 🚀 Future Enhancements

### 1. **Add Token Refresh in Web**
```csharp
// Automatically refresh expired tokens
public async Task<string?> GetValidAccessTokenAsync()
{
    var token = GetAccessTokenFromCookie();
    
    if (IsTokenExpired(token))
    {
        token = await RefreshTokenAsync();
    }
    
    return token;
}
```

### 2. **Add Delegating Handler for Automatic Token**
```csharp
// Automatically add Bearer token to API calls
public class AuthenticationDelegatingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(...)
    {
        var token = await GetAccessTokenAsync();
        request.Headers.Authorization = new("Bearer", token);
        return await base.SendAsync(request, cancellationToken);
    }
}
```

### 3. **Add Health Check**
```csharp
// Check if API is available
public async Task<bool> IsApiHealthyAsync()
{
    var response = await _httpClient.GetAsync("/health");
    return response.IsSuccessStatusCode;
}
```

---

## ✅ Summary

### What You Have Now:

✅ **API Controller** (`AuthController`)
- Single source of truth for authentication
- Returns JSON for all clients
- Handles database validation

✅ **Web Controller** (`AccountController`)
- Thin controller for form handling
- Calls API via WebAuthenticationService
- Returns redirects for Blazor

✅ **WebAuthenticationService**
- Encapsulates API calls
- Handles cookies and sign-in
- Clean separation of concerns

### Architecture:

```
Web (Form) → AccountController → WebAuthenticationService → API → Database
                                         ↑
Mobile App ──────────────────────────────┘
Desktop App ─────────────────────────────┘
External App ────────────────────────────┘
```

**Result:** Clean, maintainable, scalable architecture! 🚀

### Key Points:

1. **API is the single source of truth** ✅
2. **Web calls API (doesn't access DB)** ✅
3. **Two controllers for different purposes** ✅
4. **All clients use same API** ✅
5. **Easy to add new clients** ✅

**Your ERP now has proper client-server architecture!** 🎉

