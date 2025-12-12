using System.ComponentModel.DataAnnotations;

namespace LinhGo.ERP.Authorization.Configurations;

/// <summary>
/// JWT authentication settings
/// </summary>
public class JwtSettings
{
    [Required]
    public string Secret { get; set; } = string.Empty;
    
    [Required]
    public string Issuer { get; set; } = string.Empty;
    
    [Required]
    public string Audience { get; set; } = string.Empty;
    
    [Range(1, 1440)]
    public int AccessTokenExpirationMinutes { get; set; } = 15; // 15 minutes
    
    [Range(1, 43200)]
    public int RefreshTokenExpirationMinutes { get; set; } = 10080; // 7 days
}

