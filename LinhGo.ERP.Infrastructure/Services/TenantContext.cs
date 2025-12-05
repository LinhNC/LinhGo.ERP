using LinhGo.ERP.Domain.Common;
namespace LinhGo.ERP.Infrastructure.Services;
public class TenantContext : ITenantContext
{
    private Guid? _currentCompanyId;
    public Guid? CurrentCompanyId => _currentCompanyId;
    public void SetCompanyId(Guid companyId)
    {
        _currentCompanyId = companyId;
    }
}
