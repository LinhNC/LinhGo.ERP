namespace LinhGo.ERP.Authorization.Common.Errors;

/// <summary>
/// Authentication-related error codes
/// </summary>
public static class AuthenticationErrors
{
    // Authentication errors
    public const string InvalidCredentials = "AUTH_INVALID_CREDENTIALS";
    public const string UserNotFound = "AUTH_USER_NOT_FOUND";
    public const string InvalidPassword = "AUTH_INVALID_PASSWORD";
    public const string AccountInactive = "AUTH_ACCOUNT_INACTIVE";
    public const string AuthenticationFailed = "AUTH_FAILED";
    
    // Token errors
    public const string InvalidAccessToken = "AUTH_INVALID_ACCESS_TOKEN";
    public const string InvalidRefreshToken = "AUTH_INVALID_REFRESH_TOKEN";
    public const string RefreshTokenExpired = "AUTH_REFRESH_TOKEN_EXPIRED";
    public const string InvalidTokenClaims = "AUTH_INVALID_TOKEN_CLAIMS";
    public const string TokenRefreshFailed = "AUTH_TOKEN_REFRESH_FAILED";
    
    // Logout errors
    public const string LogoutFailed = "AUTH_LOGOUT_FAILED";
}

