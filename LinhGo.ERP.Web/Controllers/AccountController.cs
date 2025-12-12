using LinhGo.ERP.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace LinhGo.ERP.Web.Controllers;

/// <summary>
/// Account Controller - Blazor Server Authentication
/// Calls API for authentication instead of direct database access
/// </summary>
[Route("[controller]")]
public class AccountController : Controller
{
    private readonly IWebAuthenticationService _webAuthService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        IWebAuthenticationService webAuthService,
        ILogger<AccountController> logger)
    {
        _webAuthService = webAuthService;
        _logger = logger;
    }

    /// <summary>
    /// Login POST endpoint - Calls API for authentication
    /// </summary>
    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        [FromForm] string email, 
        [FromForm] string password, 
        [FromForm] bool rememberMe, 
        [FromForm] string? returnUrl)
    {
        _logger.LogInformation("[Account] Login attempt for {Email}", email);

        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return Redirect($"/login?error={Uri.EscapeDataString("Email and password are required")}&returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}");
            }

            // Call API via WebAuthenticationService
            var result = await _webAuthService.LoginAsync(email, password, rememberMe);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("[Account] Login failed for {Email}: {Error}", email, result.ErrorMessage);
                return Redirect($"/login?error={Uri.EscapeDataString(result.ErrorMessage ?? "Login failed")}&returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}");
            }

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
    /// Logout endpoint - Calls API to invalidate token
    /// </summary>
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        _logger.LogInformation("[Account] Logout attempt");

        try
        {
            await _webAuthService.LogoutAsync();
            return LocalRedirect("/login");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Account] Logout error");
            return LocalRedirect("/");
        }
    }
}

