using LinhGo.ERP.Domain.Common;

namespace LinhGo.ERP.Domain.Companies.Entities;

/// <summary>
/// Stores company-specific configuration settings
/// </summary>
public class CompanySettings : BaseEntity
{
    public Guid CompanyId { get; set; }
    public required string SettingKey { get; set; }
    public required string SettingValue { get; set; }
    public string? SettingGroup { get; set; }
    public string? Description { get; set; }
    
    public virtual Company? Company { get; set; }
}

