using System.Security.Claims;
using LinhGo.ERP.Web.Configuration;
using LinhGo.ERP.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace LinhGo.ERP.Web.Controllers;

/// <summary>
/// Account Controller - Best Practice for Blazor Server Authentication
/// Uses standard ASP.NET Core Cookie Authentication with form POST
/// </summary>
[Route("[controller]")]
public class AccountController : Controller
{
    private readonly ITokenService _tokenService;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        ITokenService tokenService,
        JwtSettings jwtSettings,
        ILogger<AccountController> logger)
    {
        _tokenService = tokenService;
        _jwtSettings = jwtSettings;
        _logger = logger;
    }

    /// <summary>
    /// Login POST endpoint - Standard form POST (Best Practice for Blazor Server)
    /// This is called directly from HTML form, so cookies are automatically saved to browser
    /// </summary>
    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login([FromForm] string email, [FromForm] string password, 
        [FromForm] bool rememberMe, [FromForm] string? returnUrl)
    {
        _logger.LogInformation("[Account] Login attempt for {Email}", email);

        try
        {
            // Validate credentials
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return Redirect($"/login?error={Uri.EscapeDataString("Email and password are required")}&returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}");
            }

            // TODO: Validate against database
            var userId = Guid.NewGuid().ToString();
            var roles = new[] { "User" };

            // Generate JWT tokens
            var accessToken = _tokenService.GenerateAccessToken(userId, email, roles);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Set JWT tokens in cookies
            SetJwtCookies(accessToken, refreshToken, rememberMe);

            // Sign in user for Blazor Server authentication
            await SignInUser(userId, email, roles, rememberMe);

            _logger.LogInformation("[Account] User {Email} logged in successfully", email);

            // Redirect to return URL or home
            return LocalRedirect(returnUrl ?? "/");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Account] Login error for {Email}", email);
            return Redirect($"/login?error={Uri.EscapeDataString("An error occurred during login")}&returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}");
        }
    }

    /// <summary>
    /// Logout endpoint
    /// </summary>
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        _logger.LogInformation("[Account] Logout attempt");

        try
        {
            // Clear JWT cookies
            Response.Cookies.Delete("access_token");
            Response.Cookies.Delete("refresh_token");

            // Sign out from cookie authentication
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            _logger.LogInformation("[Account] User logged out successfully");

            return LocalRedirect("/login");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Account] Logout error");
            return LocalRedirect("/");
        }
    }

    #region Helper Methods

    private void SetJwtCookies(string accessToken, string refreshToken, bool rememberMe)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,  // Secure if HTTPS
            SameSite = SameSiteMode.Lax,
            Path = "/"
        };

        // Access token
        Response.Cookies.Append("access_token", accessToken, new CookieOptions
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
            Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
            {
                HttpOnly = cookieOptions.HttpOnly,
                Secure = cookieOptions.Secure,
                SameSite = cookieOptions.SameSite,
                Path = cookieOptions.Path,
                Expires = DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
            });
        }

        _logger.LogDebug("[Account] JWT cookies set");
    }

    private async Task SignInUser(string userId, string email, string[] roles, bool rememberMe)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, email),
            new(ClaimTypes.Email, email)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = rememberMe,
            ExpiresUtc = rememberMe 
                ? DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
                : DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            AllowRefresh = true
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
    }

    #endregion
}

