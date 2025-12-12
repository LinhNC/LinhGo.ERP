namespace LinhGo.ERP.Authorization.DTOs;

public class RefreshTokenDto
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
}