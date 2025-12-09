# Login Page Still Loading - Troubleshooting Guide

## Quick Checklist

Run through these checks if login page is still stuck loading:

### 1. Check Browser Console
Press F12 → Console tab

**Look for:**
```
[AuthorizedView] Checking authentication...
[AuthorizedView] Authentication result: true/false
```

**If you see authentication result: false:**
- Cookie not set properly
- Cookie not being sent
- HttpContext not available

### 2. Check Network Tab
Press F12 → Network tab

**During login process, you should see:**
1. `POST /api/auth/login` → Status: 200 OK
2. Response should include `Set-Cookie` header
3. After redirect: `GET /` → Cookie should be in Request Headers

**If /api/auth/login returns 404:**
```bash
# Check that controllers are registered:
# In Program.cs, verify both lines exist:
builder.Services.AddControllers();
app.MapControllers();
```

**If no Set-Cookie header:**
- Check AuthController.cs exists
- Check SignInAsync() is being called
- Check cookie settings in Program.cs

### 3. Check Application/Storage Tab
Press F12 → Application tab (Chrome) or Storage tab (Firefox)

**Look for cookie:**
- Name: `LinhGoERP.Auth`
- HttpOnly: ✓ (Yes)
- Secure: ✓ (Yes)
- SameSite: Strict
- Has expiration date

**If cookie not present:**
- Login API call failed
- Cookie settings preventing creation
- HTTPS not enabled (required for Secure flag)

### 4. Test API Endpoint Directly

Open new tab and go to:
```
POST https://your-domain/api/auth/login
Body: {
  "email": "test@example.com",
  "password": "password",
  "rememberMe": false
}
```

**Expected response:**
```json
{
  "success": true,
  "message": "Login successful",
  "user": {
    "email": "test@example.com",
    ...
  }
}
```

**If 404 Not Found:**
- Controllers not registered
- AuthController.cs not in correct location
- Namespace issue

### 5. Check Server Logs

Look for these log messages:

**Success flow:**
```
[CookieAuthenticationService] Authentication check: true
[AuthController] User test@example.com logged in successfully
```

**Failure indicators:**
```
[CookieAuthenticationService] HttpContext is null when checking authentication
[AuthController] Error during login
Headers are read-only, response has already started
```

### 6. Verify File Structure

```
LinhGo.ERP.Web/
├── Controllers/
│   └── AuthController.cs         ← Must exist
├── Components/
│   ├── Pages/
│   │   ├── Login.razor            ← Navigates to login-processing
│   │   └── LoginProcessing.razor  ← Calls API
│   └── Layout/
│       └── AuthorizedView.razor   ← Checks authentication
├── Core/
│   ├── Services/
│   │   └── CookieAuthenticationService.cs
│   └── Models/
│       └── AuthModels.cs
└── Program.cs                     ← Controllers registered
```

### 7. Common Fixes

#### Fix 1: HttpContextAccessor not registered
```csharp
// In Program.cs, add:
builder.Services.AddHttpContextAccessor();
```

#### Fix 2: Middleware order wrong
```csharp
// In Program.cs, correct order:
app.UseHttpsRedirection();
app.UseAuthentication();    // ← Before Authorization
app.UseAuthorization();     // ← After Authentication
app.UseAntiforgery();
```

#### Fix 3: HTTPS not enabled
```bash
# Run with HTTPS:
dotnet run --launch-profile https
```

Or in launchSettings.json:
```json
{
  "https": {
    "applicationUrl": "https://localhost:5001;http://localhost:5000"
  }
}
```

#### Fix 4: Cookie not being sent
Check SameSite setting:
```csharp
// In Program.cs:
options.Cookie.SameSite = SameSiteMode.Lax; // Try Lax instead of Strict
```

### 8. Debug Mode

Add this to Login.razor for debugging:
```razor
@code {
    protected override async Task OnInitializedAsync()
    {
        Console.WriteLine($"[Login] ReturnUrl: {ReturnUrl}");
        Console.WriteLine($"[Login] Checking if authenticated...");
        
        var isAuthenticated = await AuthService.IsAuthenticatedAsync();
        Console.WriteLine($"[Login] IsAuthenticated: {isAuthenticated}");
        
        if (isAuthenticated)
        {
            Console.WriteLine($"[Login] Already authenticated, redirecting to {returnUrl}");
            NavigationManager.NavigateTo(returnUrl, forceLoad: true);
        }
    }
}
```

### 9. Test Authentication Status

Create a test page:
```razor
@page "/auth-test"
@inject ICookieAuthenticationService AuthService

<h3>Authentication Test</h3>
<button @onclick="CheckAuth">Check Auth</button>
<p>Result: @result</p>

@code {
    string result = "";
    
    async Task CheckAuth()
    {
        var isAuth = await AuthService.IsAuthenticatedAsync();
        var email = await AuthService.GetCurrentUserEmailAsync();
        result = $"Authenticated: {isAuth}, Email: {email}";
    }
}
```

### 10. Nuclear Option: Clear Everything

If nothing works:
1. Clear all browser cookies for your site
2. Close all browser tabs
3. Stop the application
4. Delete bin/ and obj/ folders
5. `dotnet clean`
6. `dotnet build`
7. `dotnet run`
8. Open fresh incognito window
9. Try login again

## Expected Behavior

### Successful Login Flow:
1. Enter credentials → Click "Sign In"
2. See "Logging in..." spinner (1-2 seconds)
3. See "Login Successful" notification
4. Redirect to home page
5. See home page content

### If Stuck:
1. Spinner shows forever
2. No redirect happens
3. Console shows `Authentication result: false`

## Getting Help

If still stuck, provide:
1. Browser console logs
2. Network tab screenshot
3. Server logs
4. Cookie present? (Yes/No)
5. API endpoint returns 200? (Yes/No)

## Success Indicators

✅ API endpoint returns 200 OK  
✅ Cookie appears in Application tab  
✅ Cookie sent with requests  
✅ Console shows "Authentication result: true"  
✅ Page redirects to home  
✅ Home content displays  

If all above are ✅ = Login working correctly!

