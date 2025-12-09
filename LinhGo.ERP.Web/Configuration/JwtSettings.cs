namespace LinhGo.ERP.Web.Configuration;

/// <summary>
/// JWT authentication settings
/// Best Practice: Store sensitive values in User Secrets / Environment Variables
/// </summary>
public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    /// <summary>
    /// Secret key for signing tokens (must be at least 32 characters)
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Token issuer (your application name)
    /// </summary>
    public string Issuer { get; set; } = "LinhGo.ERP";

    /// <summary>
    /// Token audience (who can use the token)
    /// </summary>
    public string Audience { get; set; } = "LinhGo.ERP.Client";

    /// <summary>
    /// Access token lifetime in minutes (15-30 minutes recommended)
    /// </summary>
    public int AccessTokenExpirationMinutes { get; set; } = 15;

    /// <summary>
    /// Refresh token lifetime in days (7-30 days recommended)
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;

    /// <summary>
    /// Validate the settings
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(SecretKey))
            throw new InvalidOperationException("JWT SecretKey is required");

        if (SecretKey.Length < 32)
            throw new InvalidOperationException("JWT SecretKey must be at least 32 characters");

        if (string.IsNullOrWhiteSpace(Issuer))
            throw new InvalidOperationException("JWT Issuer is required");

        if (string.IsNullOrWhiteSpace(Audience))
            throw new InvalidOperationException("JWT Audience is required");

        if (AccessTokenExpirationMinutes <= 0)
            throw new InvalidOperationException("AccessTokenExpirationMinutes must be positive");

        if (RefreshTokenExpirationDays <= 0)
            throw new InvalidOperationException("RefreshTokenExpirationDays must be positive");
    }
}

