using LinhGo.ERP.Domain.Common;
using LinhGo.ERP.Domain.Common.Interfaces;
using LinhGo.ERP.Domain.Companies.Entities;

namespace LinhGo.ERP.Domain.Companies.Interfaces;

public interface ICompanyRepository : IRepository<Company>
{
    Task<Company?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<Company>> GetActiveCompaniesAsync(CancellationToken cancellationToken = default);
    Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<PagedResult<Company>> SearchAsync(
        SearchQueryParams queries,
        CancellationToken cancellationToken = default);
}

