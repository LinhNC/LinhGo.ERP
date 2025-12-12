using LinhGo.ERP.Authorization.Common.Errors;
using LinhGo.ERP.Authorization.DTOs;
using LinhGo.ERP.Domain.Common;
using LinhGo.ERP.Domain.Users.Interfaces;
using LinhGo.SharedKernel.Result;
using Microsoft.Extensions.Logging;

namespace LinhGo.ERP.Authorization.Services;

/// <summary>
/// Authentication service for user login, token refresh, and logout operations
/// </summary>
public interface IAuthenticationService
{
    Task<Result<AuthenticationResponse>> AuthenticateAsync(string emailOrUsername, string password);
    Task<Result<AuthenticationResponse>> RefreshTokenAsync(string accessToken, string refreshToken);
    Task LogoutAsync(Guid userId);
}

public class AuthenticationService(
    IUserRepository userRepository,
    IUserCompanyRepository userCompanyRepository,
    IJwtTokenService jwtTokenService,
    ILogger<AuthenticationService> logger)
    : IAuthenticationService
{
    public async Task<Result<AuthenticationResponse>> AuthenticateAsync(string emailOrUsername, string password)
    {
        try
        {
            // Find user by email or username
            var user = await userRepository.GetByEmailAsync(emailOrUsername)
                ?? await userRepository.GetByUsernameAsync(emailOrUsername);

            if (user == null)
            {
                logger.LogWarning("Login attempt failed: User not found - {EmailOrUsername}", emailOrUsername);
                return Error.WithUnauthorizedCode(AuthenticationErrors.UserNotFound);
            }
            
            if (!VerifyPassword(password, user.PasswordHash))
            {
                logger.LogWarning("Login attempt failed: Invalid password for user {UserId}", user.Id);
                return Error.WithUnauthorizedCode(AuthenticationErrors.InvalidPassword);
            }

            // Check if user is active
            if (!user.IsActive)
            {
                logger.LogWarning("Login attempt failed: User is inactive - {UserId}", user.Id);
                return Error.WithUnauthorizedCode(AuthenticationErrors.AccountInactive);
            }

            // Get user roles and permissions from UserCompany
            var userCompanies = await userCompanyRepository.GetActiveByUserIdAsync(user.Id);
            var roles = userCompanies.Select(uc => uc.Role).Distinct().ToList();
            var permissions = GetPermissionsForRoles(roles);

            var companyIds = userCompanies.Select(uc => uc.CompanyId).Distinct().ToList();
            // Get default company
            var defaultCompanyId = userCompanies.FirstOrDefault(uc => uc.IsDefaultCompany)?.CompanyId;

            // Generate tokens with default company context
            var accessToken = jwtTokenService.GenerateAccessToken(user, roles, permissions, defaultCompanyId, companyIds);
            var refreshToken = jwtTokenService.GenerateRefreshToken();

            // Store refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            user.LastLoginAt = DateTime.UtcNow;
            await userRepository.UpdateAsync(user);

            logger.LogInformation("User {UserId} logged in successfully", user.Id);

            return new AuthenticationResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = new UserAuthInfo
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    DefaultCompanyId = defaultCompanyId,
                    CompanyIds = companyIds?.ToArray(),
                    Roles = roles,
                    Permissions = permissions
                }
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during authentication for {EmailOrUsername}", emailOrUsername);
            return Error.WithUnauthorizedCode(AuthenticationErrors.AuthenticationFailed);
        }
    }

    public async Task<Result<AuthenticationResponse>> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        try
        {
            var principal = jwtTokenService.ValidateToken(accessToken);
            if (principal == null)
            {
                return Error.WithUnauthorizedCode(AuthenticationErrors.InvalidAccessToken);
            }

            var userIdClaim = principal.FindFirst(c => c.Type == "sub")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Error.WithUnauthorizedCode(AuthenticationErrors.InvalidTokenClaims);
            }

            var user = await userRepository.GetByIdAsync(userId);
            if (user == null || user.RefreshToken != refreshToken)
            {
                return Error.WithFailureCode(AuthenticationErrors.InvalidRefreshToken);
            }

            if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return Error.WithFailureCode(AuthenticationErrors.RefreshTokenExpired);
            }

            // Generate new tokens
            var userCompanies = await userCompanyRepository.GetActiveByUserIdAsync(user.Id);
            var roles = userCompanies.Select(uc => uc.Role).Distinct().ToList();
            var permissions = GetPermissionsForRoles(roles);

            var companyIds = userCompanies.Select(uc => uc.CompanyId).Distinct().ToList();
            // Get default company
            var defaultCompanyId = userCompanies.FirstOrDefault(uc => uc.IsDefaultCompany)?.CompanyId;

            var newAccessToken = jwtTokenService.GenerateAccessToken(user, roles, permissions, defaultCompanyId, companyIds);
            var newRefreshToken = jwtTokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await userRepository.UpdateAsync(user);

            logger.LogInformation("Token refreshed for user {UserId}", user.Id);

            return new AuthenticationResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                User = new UserAuthInfo
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    DefaultCompanyId = defaultCompanyId,
                    CompanyIds = companyIds?.ToArray(),
                    Roles = roles,
                    Permissions = permissions
                }
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during token refresh");
            return Error.WithFailureCode(AuthenticationErrors.TokenRefreshFailed);
        }
    }

    public async Task LogoutAsync(Guid userId)
    {
        try
        {
            var user = await userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                await userRepository.UpdateAsync(user);
                
                logger.LogInformation("User {UserId} logged out successfully", userId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during logout for user {UserId}", userId);
        }
    }

    #region Helper Methods

    private bool VerifyPassword(string password, string passwordHash)
    {
        // Verify password using BCrypt
        // BCrypt.Verify automatically extracts the salt from the hash and verifies
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }

    private List<string> GetPermissionsForRoles(List<string> roles)
    {
        // TODO: Implement permission mapping from database
        var permissions = new List<string>();

        if (roles.Contains("Admin"))
        {
            permissions.AddRange(new[]
            {
                "users.manage", "companies.manage", "reports.view", "reports.manage",
                "settings.manage", "audit.view"
            });
        }
        else if (roles.Contains("Manager"))
        {
            permissions.AddRange(new[] { "reports.view", "users.view", "companies.view" });
        }
        else if (roles.Contains("Employee"))
        {
            permissions.AddRange(new[] { "reports.view" });
        }

        return permissions.Distinct().ToList();
    }

    #endregion
}
