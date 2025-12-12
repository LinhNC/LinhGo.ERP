namespace LinhGo.ERP.Authorization.DTOs;

public record UserAuthInfo
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public required string UserName { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public Guid? DefaultCompanyId { get; init; }
    public Guid[]? CompanyIds { get; init; } = [];
    public required List<string>? Roles { get; init; }
    public required List<string>? Permissions { get; init; }
}