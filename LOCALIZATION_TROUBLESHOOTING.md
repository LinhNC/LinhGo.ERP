# 🔧 Localization Troubleshooting & Testing Guide

## Issue: Language Display Not Working

### ✅ Fixes Applied

1. **Added Missing Import** - Added `@using Microsoft.JSInterop` to LanguageSwitcher
2. **Added Debug Logging** - Console logs to track culture changes
3. **Updated .csproj** - Added EmbeddedResource entries for .resx files

### 🧪 How to Test

#### Step 1: Rebuild the Project
```bash
cd /Users/linhnc/Documents/Projects/Personal/LinhGo.ERP
dotnet clean
dotnet build
```

#### Step 2: Run the Application
```bash
dotnet run --project LinhGo.ERP.Web
```

#### Step 3: Open Browser Console
1. Press **F12** to open Developer Tools
2. Go to **Console** tab
3. Keep it open to see debug messages

#### Step 4: Navigate to Login Page
Go to: `http://localhost:5000/login`

**Expected Console Output:**
```
[LanguageSwitcher] Current culture: en-US, Selected: en-US
```

#### Step 5: Check Page Content
Look for these elements:
- **Page Title:** Should say "Sign In - LinhGo ERP" (in browser tab)
- **Form Title:** Should say "Welcome back"
- **Email Label:** Should say "Email Address"
- **Button:** Should say "Sign In"
- **Language Dropdown:** Should show "English"

#### Step 6: Test Language Switch
1. Click the language dropdown (shows "English")
2. Select "Tiếng Việt"

**Expected Console Output:**
```
[LanguageSwitcher] Language changed to: vi-VN
[LanguageSwitcher] Setting cookie: LinhGoERP.Culture=c=vi-VN|uic=vi-VN
[LanguageSwitcher] Reloading page...
[LanguageSwitcher] Current culture: vi-VN, Selected: vi-VN
```

#### Step 7: Verify Vietnamese Text
After page reload, check:
- **Page Title:** Should say "Đăng Nhập - LinhGo ERP"
- **Form Title:** Should say "Chào mừng trở lại"
- **Email Label:** Should say "Địa Chỉ Email"
- **Button:** Should say "Đăng Nhập"
- **Language Dropdown:** Should show "Tiếng Việt"

#### Step 8: Check Cookie
1. F12 → Application tab → Cookies
2. Look for `LinhGoERP.Culture`
3. Value should be: `c=vi-VN|uic=vi-VN`

### ❌ Common Issues & Solutions

#### Issue 1: Dropdown Shows English, But No Languages
**Symptom:** Dropdown appears empty or shows nothing

**Solution:**
```bash
# The component might not be loading
# Check browser console for errors
# Verify Radzen is loaded:
```

Open browser console and type:
```javascript
typeof Radzen
// Should return: "object"
```

#### Issue 2: Language Changes But Text Stays English
**Symptom:** Cookie is set, but page still shows English

**Possible Causes:**
1. **Resource files not embedded**
   ```bash
   # Rebuild project
   dotnet clean
   dotnet build
   ```

2. **Middleware not configured**
   Check Program.cs has:
   ```csharp
   app.UseRequestLocalization();
   ```

3. **Resource files in wrong location**
   Verify structure:
   ```
   LinhGo.ERP.Web/
   └── Resources/
       └── Pages/
           ├── Login.resx
           └── Login.vi.resx
   ```

#### Issue 3: Page Doesn't Reload After Language Change
**Symptom:** Click language, nothing happens

**Debug Steps:**
1. Check browser console for JavaScript errors
2. Verify console shows: `[LanguageSwitcher] Language changed to: vi-VN`
3. If no console output, the Change event isn't firing

**Solution:**
Make sure LanguageSwitcher has:
```razor
Change="@OnLanguageChanged"
```

#### Issue 4: Cookie Not Being Set
**Symptom:** Console shows cookie being set, but Application tab shows no cookie

**Debug:**
```javascript
// In browser console:
document.cookie
// Should show: "LinhGoERP.Culture=..."
```

If empty, try:
```javascript
// Manually set cookie:
document.cookie = "LinhGoERP.Culture=c=vi-VN|uic=vi-VN; path=/; max-age=31536000"
// Then reload page
location.reload()
```

#### Issue 5: Localizer Returns Key Instead of Value
**Symptom:** Page shows `[FormTitle]` instead of "Welcome back"

**Causes:**
1. Resource file not embedded
2. Resource namespace mismatch
3. Resource file not found

**Solution:**
Check the .resx file has:
```xml
<data name="FormTitle" xml:space="preserve">
  <value>Welcome back</value>
</data>
```

And Login.razor has:
```razor
@inject IStringLocalizer<Login> Localizer
```

### 🔍 Debug Commands

#### Check Current Culture
Add this temporarily to Login.razor:
```razor
<div style="background: yellow; padding: 10px; margin: 10px;">
    DEBUG: Culture = @System.Globalization.CultureInfo.CurrentCulture.Name<br/>
    UI Culture = @System.Globalization.CultureInfo.CurrentUICulture.Name<br/>
    FormTitle = @Localizer["FormTitle"]<br/>
</div>
```

#### Check Resource File Loading
Add to Login.razor @code:
```csharp
protected override void OnInitialized()
{
    Console.WriteLine($"[Login] Current Culture: {CultureInfo.CurrentCulture.Name}");
    Console.WriteLine($"[Login] Current UI Culture: {CultureInfo.CurrentUICulture.Name}");
    Console.WriteLine($"[Login] FormTitle: {Localizer["FormTitle"]}");
    Console.WriteLine($"[Login] EmailLabel: {Localizer["EmailLabel"]}");
}
```

#### Force Culture Change
Add this button temporarily:
```razor
<button @onclick="@(() => ForceVietnamese())">Force Vietnamese</button>

@code {
    private void ForceVietnamese()
    {
        var culture = new System.Globalization.CultureInfo("vi-VN");
        System.Globalization.CultureInfo.CurrentCulture = culture;
        System.Globalization.CultureInfo.CurrentUICulture = culture;
        StateHasChanged();
    }
}
```

### ✅ Success Indicators

When working correctly, you should see:

#### In Browser Console:
```
[LanguageSwitcher] Current culture: en-US, Selected: en-US
[Login] Current Culture: en-US
[Login] Current UI Culture: en-US
[Login] FormTitle: Welcome back
[Login] EmailLabel: Email Address
```

#### On Page:
- ✅ Dropdown shows "English" or "Tiếng Việt"
- ✅ All text matches selected language
- ✅ Page title in browser tab matches language
- ✅ Placeholders match language

#### In Cookies (F12 → Application):
- ✅ `LinhGoERP.Culture` cookie exists
- ✅ Value is `c=vi-VN|uic=vi-VN` or `c=en-US|uic=en-US`

### 📋 Quick Checklist

- [ ] Project rebuilt after adding resources
- [ ] Browser cache cleared
- [ ] Resource files exist in `/Resources/Pages/`
- [ ] `.csproj` has `<EmbeddedResource>` entries
- [ ] `Program.cs` has `AddLocalization()` and `UseRequestLocalization()`
- [ ] Login.razor has `@inject IStringLocalizer<Login> Localizer`
- [ ] LanguageSwitcher component loads without errors
- [ ] Browser console shows debug messages
- [ ] Cookie is set when changing language
- [ ] Page reloads after language change

### 🎯 Expected Behavior Flow

```
1. User opens /login
   → Middleware reads LinhGoERP.Culture cookie (if exists)
   → Sets CultureInfo.CurrentCulture
   → Page renders with localized strings

2. User clicks language dropdown
   → Sees: English, Tiếng Việt

3. User selects Tiếng Việt
   → OnLanguageChanged fires
   → JavaScript sets cookie
   → Page reloads (forceLoad: true)
   → Middleware reads new cookie value
   → Sets culture to vi-VN
   → Page renders in Vietnamese

4. User closes browser and reopens
   → Cookie still exists
   → Middleware reads cookie
   → Page loads in Vietnamese automatically
```

### 🆘 Still Not Working?

If after all troubleshooting it still doesn't work:

1. **Check Program.cs middleware order:**
   ```csharp
   app.UseRequestLocalization();  // MUST be before UseAuthentication
   app.UseSession();
   app.UseAuthentication();
   app.UseAuthorization();
   ```

2. **Verify resource file format:**
   - Must be valid XML
   - Must have proper `<data>` elements
   - Must have correct `name` attributes

3. **Check for file encoding:**
   - Resource files should be UTF-8
   - Check for BOM (Byte Order Mark) issues

4. **Try minimal test:**
   Create a simple page:
   ```razor
   @page "/test-locale"
   @using Microsoft.Extensions.Localization
   @inject IStringLocalizer<Login> Localizer
   
   <h1>Test: @Localizer["FormTitle"]</h1>
   <p>Culture: @System.Globalization.CultureInfo.CurrentCulture.Name</p>
   ```

If this shows the key instead of value, the issue is with resource loading, not the component.

## Summary

The localization system should now work correctly. If you're still having issues:
1. Check the console logs
2. Verify the cookie is being set
3. Ensure the middleware is configured
4. Rebuild the project

**Most common fix:** Just rebuild the project after adding the resource files!

```bash
dotnet clean && dotnet build && dotnet run
```

