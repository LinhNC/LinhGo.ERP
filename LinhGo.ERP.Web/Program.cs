using System.Globalization;
using System.Text;
using LinhGo.ERP.Web.Components;
using LinhGo.ERP.Web.Configuration;
using LinhGo.ERP.Web.Core.Interfaces;
using LinhGo.ERP.Web.Core.Services;
using LinhGo.ERP.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.IdentityModel.Tokens;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

// Add HTTP Context Accessor (required for cookie auth)
builder.Services.AddHttpContextAccessor();

// Configure Localization (Best Practice)
builder.Services.AddLocalization();

// Configure supported cultures
var supportedCultures = new[]
{
    new CultureInfo("en-US"), // English (default)
    new CultureInfo("vi-VN")  // Vietnamese
};

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    
    // Best Practice: Use cookie to persist user's language preference
    // Using default cookie name for compatibility
    options.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider());
});

// Configure JWT Settings
var jwtSettings = new JwtSettings();
builder.Configuration.GetSection(JwtSettings.SectionName).Bind(jwtSettings);
jwtSettings.Validate();
builder.Services.AddSingleton(jwtSettings);

// Register Token Service
builder.Services.AddScoped<ITokenService, TokenService>();

// Configure Authentication with JWT Bearer + Cookie (Best Practice: Dual Authentication)
builder.Services.AddAuthentication(options =>
{
    // Default scheme is Cookie for Blazor Server components
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    // Cookie settings - For Blazor Server auth state
    options.Cookie.Name = "auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.MaxAge = TimeSpan.FromDays(30);
    
    options.ExpireTimeSpan = TimeSpan.FromMinutes(jwtSettings.AccessTokenExpirationMinutes);
    options.SlidingExpiration = true;
    
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/access-denied";
    options.ReturnUrlParameter = "returnUrl";
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    // JWT Bearer settings - For API authentication
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ClockSkew = TimeSpan.Zero // No tolerance for expiration
    };
});

// Authorization
builder.Services.AddAuthorization();

// Custom Authentication State Provider (Best Practice)
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => 
    sp.GetRequiredService<CustomAuthenticationStateProvider>());
builder.Services.AddCascadingAuthenticationState();

// Add Controllers for API endpoints with antiforgery support
builder.Services.AddControllersWithViews();

// Add antiforgery services (required for ValidateAntiForgeryToken)
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "X-CSRF-TOKEN";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// Add distributed memory cache (required for session)
builder.Services.AddDistributedMemoryCache();

// Add session for form POST pattern
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services
    .AddRadzenComponents()
    .AddRadzenComponents();

// Add HttpClient for API calls within same app
builder.Services.AddHttpClient();

builder.Services.AddScoped<ISystemMenuService, SystemMenuService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// Localization middleware (Best Practice: before Authentication)
app.UseRequestLocalization();

// Session middleware (must be before Authentication)
app.UseSession();

// Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();