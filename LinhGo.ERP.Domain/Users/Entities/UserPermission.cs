using LinhGo.ERP.Domain.Common;

namespace LinhGo.ERP.Domain.Users.Entities;

/// <summary>
/// Stores specific permissions for users within a company
/// </summary>
public class UserPermission : BaseEntity
{
    public Guid UserCompanyId { get; set; }
    public required string PermissionKey { get; set; }
    public bool IsGranted { get; set; } = true;
    
    public virtual UserCompany? UserCompany { get; set; }
}

