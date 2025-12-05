using LinhGo.ERP.Domain.Common;

namespace LinhGo.ERP.Domain.Users.Entities;

/// <summary>
/// Represents a user in the system
/// Users can belong to one or multiple companies with different roles
/// </summary>
public class User : BaseEntity
{
    public required string Email { get; set; }
    public required string UserName { get; set; }
    public required string PasswordHash { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Avatar { get; set; }
    
    public bool IsActive { get; set; } = true;
    public bool EmailConfirmed { get; set; } = false;
    public DateTime? LastLoginAt { get; set; }
    
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockedOutUntil { get; set; }
    
    public virtual ICollection<UserCompany>? UserCompanies { get; set; }
}

