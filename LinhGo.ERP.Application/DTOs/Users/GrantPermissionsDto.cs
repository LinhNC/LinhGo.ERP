namespace LinhGo.ERP.Application.DTOs.Users;

public class GrantPermissionsDto
{
    public Guid UserId { get; set; }
    public Guid CompanyId { get; set; }
    public List<string> PermissionKeys { get; set; } = new();
}

