using Asp.Versioning;
using LinhGo.ERP.Api.Models;
using LinhGo.ERP.Application.Common.Constants;
using LinhGo.ERP.Authorization.DTOs;
using LinhGo.ERP.Authorization.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinhGo.ERP.Api.Controllers.V1;

/// <summary>
/// Authentication endpoints
/// Handles login, logout, and token refresh
/// Should move to a separate Auth microservice in the future
/// </summary>
[ApiVersion(GeneralConstants.ApiV1Version)]
[Route("api/v{version:apiVersion}/auth")]
[Tags("Authentication")]
public class AuthController(
    IAuthenticationService authenticationService,
    ILogger<AuthController> logger) : BaseApiController
{
    /// <summary>
    /// Login with username/email and password
    /// </summary>
    /// <param name="request">Login credentials</param>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await authenticationService.AuthenticateAsync(request.EmailOrUsername, request.Password);
        return ToResponse(result);
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto request)
    {
        var result = await authenticationService.RefreshTokenAsync(request.AccessToken, request.RefreshToken);

        return ToResponse(result);
    }

    /// <summary>
    /// Logout - invalidate refresh token
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        var userIdClaim = User.FindFirst(c => c.Type == "sub")?.Value;
        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
        {
            await authenticationService.LogoutAsync(userId);
        }

        // REST API best practice: Only invalidate token in database
        // Client is responsible for clearing tokens from their storage
        return NoContent();
    }
}

