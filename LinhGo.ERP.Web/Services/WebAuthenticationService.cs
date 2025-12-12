using System.Security.Claims;
using LinhGo.ERP.Authorization.Common;
using LinhGo.ERP.Authorization.Configurations;
using LinhGo.ERP.Authorization.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

namespace LinhGo.ERP.Web.Services;

/// <summary>
/// Authentication service for Blazor Web that calls the API
/// </summary>
public interface IWebAuthenticationService
{
    Task<WebAuthResult> LoginAsync(string email, string password, bool rememberMe);
    Task LogoutAsync();
}

public class WebAuthenticationService : IWebAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<WebAuthenticationService> _logger;

    public WebAuthenticationService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        IOptions<JwtSettings> jwtSettings,
        ILogger<WebAuthenticationService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("API");
        _httpContextAccessor = httpContextAccessor;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public async Task<WebAuthResult> LoginAsync(string email, string password, bool rememberMe)
    {
        try
        {
            // Call API login endpoint
            var response = await _httpClient.PostAsJsonAsync("/api/v1/auth/login", new
            {
                EmailOrUsername = email,
                Password = password
            });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("API login failed: {Error}", error);
                return WebAuthResult.Failed("Invalid email or password");
            }

            var loginResponse = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
            if (loginResponse == null)
            {
                return WebAuthResult.Failed("Invalid response from server");
            }

            // Set JWT cookies
            SetJwtCookies(loginResponse.AccessToken, loginResponse.RefreshToken, rememberMe);

            // Sign in user for Blazor Server
            await SignInUserAsync(
                loginResponse.User.Id.ToString(),
                loginResponse.User.UserName,
                loginResponse.User.Email,
                loginResponse.User.Roles?.ToArray(),
                loginResponse.User.DefaultCompanyId,
                loginResponse.User.CompanyIds?.ToArray(),
                rememberMe);

            _logger.LogInformation("User {Email} logged in successfully via API", email);

            return WebAuthResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {Email}", email);
            return WebAuthResult.Failed("An error occurred during login");
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return;

            // Get access token from cookie
            var accessToken = httpContext.Request.Cookies["access_token"];
            if (!string.IsNullOrEmpty(accessToken))
            {
                // Call API logout endpoint
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                
                await _httpClient.PostAsync("/api/v1/auth/logout", null);
            }

            // Clear JWT cookies
            httpContext.Response.Cookies.Delete("access_token");
            httpContext.Response.Cookies.Delete("refresh_token");

            // Sign out from cookie authentication
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            _logger.LogInformation("User logged out successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
        }
    }

    #region Helper Methods

    private void SetJwtCookies(string accessToken, string refreshToken, bool rememberMe)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return;

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = httpContext.Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Path = "/"
        };

        // Access token
        httpContext.Response.Cookies.Append("access_token", accessToken, new CookieOptions
        {
            HttpOnly = cookieOptions.HttpOnly,
            Secure = cookieOptions.Secure,
            SameSite = cookieOptions.SameSite,
            Path = cookieOptions.Path,
            Expires = DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes)
        });

        // Refresh token (if remember me)
        if (rememberMe)
        {
            httpContext.Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
            {
                HttpOnly = cookieOptions.HttpOnly,
                Secure = cookieOptions.Secure,
                SameSite = cookieOptions.SameSite,
                Path = cookieOptions.Path,
                Expires = DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.RefreshTokenExpirationMinutes)
            });
        }
    }

    private async Task SignInUserAsync(string userId, string userName, string email, string[]? roles, Guid? defaultCompanyId, Guid[]? companyIds, bool rememberMe)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return;

        var claims = ClaimBuilder.BuildUserClaims(
            userId,
            userName,
            email,
            roles,
            defaultCompanyId?.ToString(),
            companyIds?.Select(c => c.ToString()));

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = rememberMe,
            ExpiresUtc = rememberMe
                ? DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.RefreshTokenExpirationMinutes)
                : DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            AllowRefresh = true
        };

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
    }

    #endregion
}

#region Response Models

public class WebAuthResult
{
    public bool IsSuccess { get; private set; }
    public string? ErrorMessage { get; private set; }

    public static WebAuthResult Success() => new() { IsSuccess = true };
    public static WebAuthResult Failed(string error) => new() { IsSuccess = false, ErrorMessage = error };
}
#endregion

