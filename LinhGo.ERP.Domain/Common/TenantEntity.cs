namespace LinhGo.ERP.Domain.Common;

/// <summary>
/// Base entity for all company-specific (tenant) entities
/// </summary>
public abstract class TenantEntity : BaseEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
}

