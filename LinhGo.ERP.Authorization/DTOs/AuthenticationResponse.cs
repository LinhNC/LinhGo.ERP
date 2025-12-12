namespace LinhGo.ERP.Authorization.DTOs;

/// <summary>
/// Authentication response DTO
/// </summary>
public class AuthenticationResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public UserAuthInfo User { get; init; }
}
