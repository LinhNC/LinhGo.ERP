using LinhGo.ERP.Domain.Common;
using LinhGo.ERP.Domain.Companies.Entities;

namespace LinhGo.ERP.Domain.Users.Entities;

/// <summary>
/// Junction table linking users to companies with specific roles
/// </summary>
public class UserCompany : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid CompanyId { get; set; }
    public required string Role { get; set; }
    public bool IsDefaultCompany { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime? JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LeftAt { get; set; }
    
    public virtual User? User { get; set; }
    public virtual Company? Company { get; set; }
    public virtual ICollection<UserPermission>? Permissions { get; set; }
}

