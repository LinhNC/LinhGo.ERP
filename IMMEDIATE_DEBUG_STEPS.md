# 🔧 IMMEDIATE DEBUGGING STEPS

## Run These Tests RIGHT NOW

### Test 0: TEST COOKIE SETTING (NEW!)
1. **Start the application**
2. **Open browser** and go to: `http://localhost:5000/api/auth/test-cookie` (or your port)
3. **Press F12** → **Application tab** → **Cookies**
4. **What do you see?**
   - ✅ Cookie `LinhGoERP.Auth` appears → Cookie system is working!
   - ❌ No cookie appears → Cookie system broken, check server logs

### Test 1: Check if API Endpoint Works
1. **Open browser** and go to: `http://localhost:5000/api/auth/status` (adjust port if needed)
2. **What do you see?**
   - ✅ `{"isAuthenticated":false}` → API is working!
   - ❌ `404 Not Found` → API not registered, controllers broken
   - ❌ `Connection refused` → App not running

### Test 2: Open Browser Console BEFORE Login
1. **Press F12** → Console tab
2. **Navigate to `/login`**
3. **Check console logs** - Should see:
   ```
   [Login] OnInitializedAsync - ReturnUrl: /
   [Login] IsAuthenticated check result: false
   [Login] User is NOT authenticated, showing login form
   ```

### Test 3: Try Login with Console Open
1. **Keep console open**
2. **Enter email**: test@test.com
3. **Enter password**: password123
4. **Click "Sign In"**
5. **WATCH THE CONSOLE** - You should see:
   ```
   [LoginProcessing] Starting login process for test@test.com
   [LoginProcessing] Calling API: https://localhost:5001/api/auth/login
   [LoginProcessing] API Response: 200  ← MUST BE 200!
   [LoginProcessing] API Response Body: {"success":true...}
   [LoginProcessing] Redirecting to: /
   ```

### Test 4: Check Server Console
Look at your terminal where the app is running. You should see:
```
[AuthController] Login endpoint called for email: test@test.com
[AuthController] Validating credentials...
[AuthController] Signing in user test@test.com...
[AuthController] User test@test.com logged in successfully, cookie set
```

**If you DON'T see these logs**, the API is not being called!

### Test 5: Check Network Tab
1. **F12** → **Network tab**
2. **Try login again**
3. **Look for**: `POST /api/auth/login`
4. **Click on it** → Check:
   - Status: Should be **200 OK**
   - Response Headers: Look for `Set-Cookie: LinhGoERP.Auth=...`
   - Request payload: Should show your email/password

## What to Share With Me

Please run the tests above and tell me:

1. **Test 1 Result**: What URL did you try? What response?
2. **Browser Console Logs**: Copy ALL the console output
3. **Server Console Logs**: Copy server logs showing [AuthController]
4. **Network Tab**: Screenshot of the /api/auth/login request
5. **What happens**: Does it stay on login page? Does it redirect?

## IMPORTANT: Security Settings Changed!

I've already updated your `Program.cs` with these changes:
```csharp
options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Was: Always
options.Cookie.SameSite = SameSiteMode.Lax;                     // Was: Strict
```

**This allows cookies to work with HTTP (localhost).**

**⚠️ For production, change back to:**
```csharp
options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
options.Cookie.SameSite = SameSiteMode.Strict;
```

## Quick Fixes to Try

### If Test 0 Fails (Cookie Not Set)

**Server logs** should show:
```
[Cookie] OnSigningIn event - Setting authentication cookie
[Cookie] OnSignedIn event - User signed in successfully
```

If you DON'T see these logs, SignInAsync is failing silently.

### If Test 1 Fails (API Returns 404)

Check `Program.cs` has both these lines:
```csharp
// Before var app = builder.Build();
builder.Services.AddControllers();

// After var app = builder.Build();
app.MapControllers();
```

### If No Console Logs Appear

The code might not be up to date. Check:
```bash
# Stop the app
# Clean and rebuild
dotnet clean
dotnet build
dotnet run
```

### If API Returns 500 Error

Check server console for error details. Likely:
- HttpContextAccessor not registered
- Cookie configuration error

### If Cookie Not Being Set

Add this temporarily to `Program.cs`:
```csharp
.AddCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // ← TEMPORARY!
    options.Cookie.SameSite = SameSiteMode.Lax;
    // ... rest
});
```

## Most Common Issue

**API Not Being Called** - If you don't see `[LoginProcessing] API Response: 200` in console:

1. Check `/login-processing` page exists
2. Check `HttpClient` is registered in DI
3. Check `IHttpClientFactory` is available
4. Check browser console for JavaScript errors

## Run This Command

In browser console after login attempt, run:
```javascript
// Check if cookie exists
console.log(document.cookie);

// Should show: LinhGoERP.Auth=...
// If empty, cookie not set!
```

## TELL ME:

After running these tests, tell me which step failed and I'll provide the exact fix!

