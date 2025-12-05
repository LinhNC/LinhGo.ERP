namespace LinhGo.ERP.Domain.Common;

/// <summary>
/// Interface for entities that belong to a specific company (tenant)
/// </summary>
public interface ITenantEntity
{
    Guid CompanyId { get; set; }
}

