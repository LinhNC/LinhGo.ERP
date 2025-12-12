using LinhGo.ERP.Domain.Common;
using LinhGo.ERP.Domain.Common.Interfaces;
using LinhGo.ERP.Domain.Companies.Entities;
using LinhGo.SharedKernel.Querier;
using LinhGo.SharedKernel.Result;

namespace LinhGo.ERP.Domain.Companies.Interfaces;

public interface ICompanyRepository : IRepository<Company>
{
    Task<Company?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<Company>> GetActiveCompaniesAsync(CancellationToken cancellationToken = default);
    Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<PagedResult<Company>> QueryAsync(
        QuerierParams queries,
        CancellationToken cancellationToken = default);
}

