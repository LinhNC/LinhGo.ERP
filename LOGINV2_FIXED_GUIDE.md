# ✅ LoginV2 Fixed - Testing Guide

## What Was Wrong

**Problem:** LoginV2 was using `HttpClient` from the Blazor Server component, which makes **server-side HTTP requests**. When the API sets cookies, they are saved on the server, NOT in the user's browser!

**Root Cause:**
```
Browser → Blazor Server → HttpClient → API
                ↑                    ↑
          No cookie!           Cookie set here (on server)
```

## The Fix

**Solution:** Use JavaScript `fetch()` API which runs in the browser:
```
Browser → JavaScript fetch() → API
    ↑                            ↑
Cookie saved here!        Cookie set here
```

### Files Updated:
1. ✅ `LoginV2.razor` - Now uses `JS.InvokeAsync("loginUser", ...)`
2. ✅ `wwwroot/js/auth.js` - Updated to call `/api/authentication/login`

## How to Test

### Step 1: Restart Application
```bash
# Stop the app (Ctrl+C)
dotnet run
```

### Step 2: Clear Browser Cache
1. Press F12
2. Go to Application tab
3. Click "Clear storage"
4. Click "Clear site data"
5. Close DevTools

### Step 3: Test LoginV2
1. Go to: `http://localhost:5000/login-v2`
2. Open browser console (F12)
3. Enter credentials:
   - **Email:** test@test.com
   - **Password:** anything
4. Check "Remember Me"
5. Click "Sign In"

### Step 4: Expected Console Output

**Browser Console:**
```
[LoginV2] Attempting login for test@test.com
[JS] loginUser called with: test@test.com
[JS] API Response status: 200
[JS] Login successful, cookies set by browser
[LoginV2] API Response: Success=True
[LoginV2] Login successful
[LoginV2] Redirecting to: /
[LoginV2] OnInitializedAsync - ReturnUrl: /
[LoginV2] IsAuthenticated check result: true
[LoginV2] User is authenticated, redirecting to /
```

**Server Console:**
```
[Auth] Login attempt for test@test.com
[Auth] User test@test.com logged in successfully
[Cookie] Setting authentication cookie
```

### Step 5: Verify Cookies

F12 → Application tab → Cookies

**You should see 3 cookies:**
1. ✅ `access_token` - JWT token (15 min)
2. ✅ `refresh_token` - Refresh token (7 days)
3. ✅ `LinhGoERP.Auth` - Blazor auth cookie

**Cookie Properties:**
- HttpOnly: ✓ Yes
- Secure: (depends on HTTPS)
- SameSite: Lax
- Path: /

## Troubleshooting

### Issue 1: "loginUser is not a function"
**Symptom:** JavaScript error in console

**Solution:**
1. Check if `auth.js` is loaded:
   - Go to: `http://localhost:5000/js/auth.js`
   - Should show the JavaScript code
2. Check `App.razor` has: `<script src="js/auth.js"></script>`
3. Clear browser cache and reload

### Issue 2: API Returns 404
**Symptom:** `[JS] API Response status: 404`

**Solution:**
```bash
# Test API endpoint directly:
curl -X POST http://localhost:5000/api/authentication/status
# Should return: {"isAuthenticated":false}

# If 404, check:
# 1. Controllers are registered in Program.cs
# 2. AuthenticationController.cs exists
# 3. Restart the app
```

### Issue 3: Cookies Not Set
**Symptom:** No cookies in Application tab after login

**Check:**
1. Is the API returning 200 OK?
2. Check browser console for errors
3. Check server console logs
4. Try with different browser

**Debug:**
```javascript
// In browser console after login:
document.cookie
// Should show: "access_token=...; refresh_token=...; LinhGoERP.Auth=..."
```

### Issue 4: Redirect to Login After Success
**Symptom:** Logs show "IsAuthenticated check result: false"

**This means cookies are not being read correctly.**

**Solution:**
1. Check cookie expiration hasn't passed
2. Check cookie domain matches
3. Try clearing all cookies and login again
4. Check server logs for validation errors

### Issue 5: Still Shows Old Login Page
**Solution:**
```bash
# Clear build cache:
dotnet clean
dotnet build

# Force refresh browser:
Ctrl + Shift + R (Windows/Linux)
Cmd + Shift + R (Mac)
```

## Comparison: Working vs Not Working

### ✅ Working (JavaScript fetch):
```
1. User clicks "Sign In"
2. LoginV2 calls JS.InvokeAsync("loginUser", ...)
3. JavaScript fetch() calls /api/authentication/login
4. Browser sends request
5. API returns cookies in Set-Cookie header
6. Browser automatically saves cookies
7. Redirect happens
8. Browser sends cookies with next request
9. Authentication succeeds! ✓
```

### ❌ Not Working (HttpClient):
```
1. User clicks "Sign In"
2. LoginV2 calls HttpClient.PostAsJsonAsync(...)
3. Blazor Server makes HTTP request (server-side)
4. API returns cookies
5. Cookies saved on SERVER (not browser)
6. Redirect happens
7. Browser has NO cookies
8. Authentication fails! ✗
```

## API Endpoints

### Test Status:
```bash
curl http://localhost:5000/api/authentication/status
# Returns: {"isAuthenticated":false,"email":null}
```

### Test Login:
```bash
curl -X POST http://localhost:5000/api/authentication/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@test.com","password":"test","rememberMe":true}' \
  -c cookies.txt -v

# Should see Set-Cookie headers in response
# Check cookies.txt file
```

### Test with Saved Cookies:
```bash
curl http://localhost:5000/api/authentication/status \
  -b cookies.txt

# Returns: {"isAuthenticated":true,"email":"test@test.com"}
```

## Quick Test Checklist

- [ ] Application started: `dotnet run`
- [ ] Browser cache cleared
- [ ] Go to `/login-v2`
- [ ] Enter email: test@test.com
- [ ] Enter password: anything
- [ ] Check "Remember Me"
- [ ] Click "Sign In"
- [ ] Console shows: `[JS] Login successful`
- [ ] Console shows: `Success=True`
- [ ] Redirects to home page
- [ ] Application tab shows 3 cookies
- [ ] Home page displays (not login page)

## Success Indicators

✅ No JavaScript errors in console  
✅ API returns 200 OK  
✅ 3 cookies set in browser  
✅ Redirect happens  
✅ Home page displays  
✅ No infinite loop  
✅ Authentication working  

## Why This Approach?

**Q: Why use JavaScript for login if we're trying to avoid it?**

**A:** For Blazor Server, there's no way around it:
- Blazor Server renders on the server
- HttpClient makes server-side requests
- Cookies from server-side requests don't save to browser
- JavaScript fetch() runs in browser and properly handles cookies

**This is the industry-standard approach for Blazor Server authentication with cookie-based tokens.**

**Alternative:** Use Blazor WebAssembly (WASM) where HttpClient runs in browser, but that's a different hosting model.

## Summary

LoginV2 now works correctly by:
1. ✅ Using JavaScript fetch() for API call
2. ✅ Calling correct endpoint: `/api/authentication/login`
3. ✅ Setting JWT tokens in HTTP-only cookies
4. ✅ Browser automatically saves cookies
5. ✅ Redirect works properly
6. ✅ Authentication succeeds

**Test it now and it should work! 🎉**

