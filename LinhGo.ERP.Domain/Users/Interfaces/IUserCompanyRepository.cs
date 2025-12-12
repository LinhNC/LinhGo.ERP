using LinhGo.ERP.Domain.Common.Interfaces;
using LinhGo.ERP.Domain.Users.Entities;
using LinhGo.SharedKernel.Querier;
using LinhGo.SharedKernel.Result;

namespace LinhGo.ERP.Domain.Users.Interfaces;

public interface IUserCompanyRepository : IRepository<UserCompany>
{
    Task<UserCompany?> GetByUserAndCompanyAsync(Guid userId, Guid companyId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserCompany>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserCompany>> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserCompany>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserCompany>> GetActiveByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<bool> IsUserAssignedToCompanyAsync(Guid userId, Guid companyId, CancellationToken cancellationToken = default);
    Task<UserCompany?> GetWithPermissionsAsync(Guid userId, Guid companyId, CancellationToken cancellationToken = default);
    Task AssignUserToCompanyAsync(Guid userId, Guid companyId, string role, CancellationToken cancellationToken = default);
    Task RemoveUserFromCompanyAsync(Guid userId, Guid companyId, CancellationToken cancellationToken = default);
    Task UpdateRoleAsync(Guid userId, Guid companyId, string role, CancellationToken cancellationToken = default);
    Task<PagedResult<UserCompany>> QueryAsync(QuerierParams queries, CancellationToken cancellationToken = default);
}

