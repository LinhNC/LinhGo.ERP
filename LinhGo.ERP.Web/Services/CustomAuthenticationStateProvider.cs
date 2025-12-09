using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace LinhGo.ERP.Web.Services;

/// <summary>
/// Custom Authentication State Provider for Blazor Server
/// Best Practice: Integrates with ASP.NET Core Authentication
/// </summary>
public class CustomAuthenticationStateProvider : RevalidatingServerAuthenticationStateProvider
{
    private readonly ILogger<CustomAuthenticationStateProvider> _logger;

    public CustomAuthenticationStateProvider(
        ILoggerFactory loggerFactory,
        ILogger<CustomAuthenticationStateProvider> logger)
        : base(loggerFactory)
    {
        _logger = logger;
    }

    /// <summary>
    /// Revalidation interval - how often to check if the user is still authenticated
    /// Best Practice: Balance between security and performance
    /// </summary>
    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(5);

    /// <summary>
    /// Validate the authentication state
    /// Best Practice: Check token expiration and user validity
    /// </summary>
    protected override Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState, CancellationToken cancellationToken)
    {
        try
        {
            var user = authenticationState.User;
            
            if (user?.Identity?.IsAuthenticated != true)
            {
                _logger.LogDebug("User is not authenticated");
                return Task.FromResult(false);
            }

            // Check if token has expired
            var exp = user.FindFirst("exp")?.Value;
            if (!string.IsNullOrEmpty(exp) && long.TryParse(exp, out var expValue))
            {
                var expirationTime = DateTimeOffset.FromUnixTimeSeconds(expValue);
                if (expirationTime <= DateTimeOffset.UtcNow)
                {
                    _logger.LogInformation("Token has expired for user {UserId}", 
                        user.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                    return Task.FromResult(false);
                }
            }

            _logger.LogDebug("Authentication state is valid for user {UserId}", 
                user.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating authentication state");
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Notify authentication state changed
    /// Best Practice: Call this after login/logout to update UI
    /// </summary>
    public void NotifyAuthenticationStateChanged()
    {
        try
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            _logger.LogDebug("Authentication state change notified");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying authentication state change");
        }
    }
}

