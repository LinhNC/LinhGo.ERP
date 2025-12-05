namespace LinhGo.ERP.Application.DTOs.Users;

public class AssignUserToCompanyDto
{
    public Guid UserId { get; set; }
    public Guid CompanyId { get; set; }
    public string Role { get; set; } = "User";
    public bool IsDefault { get; set; }
}

