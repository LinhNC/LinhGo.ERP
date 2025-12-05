namespace LinhGo.ERP.Domain.Common;

/// <summary>
/// Service to provide the current tenant (company) context
/// </summary>
public interface ITenantContext
{
    Guid? CurrentCompanyId { get; }
    void SetCompanyId(Guid companyId);
}

