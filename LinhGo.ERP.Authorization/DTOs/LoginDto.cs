namespace LinhGo.ERP.Authorization.DTOs;

public record LoginRequest
{
    public required string EmailOrUsername { get; init; }
    public required string Password { get; init; }
}