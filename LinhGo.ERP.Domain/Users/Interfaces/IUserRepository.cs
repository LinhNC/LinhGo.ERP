using LinhGo.ERP.Domain.Common.Interfaces;
using LinhGo.ERP.Domain.Users.Entities;
using LinhGo.SharedKernel.Querier;
using LinhGo.SharedKernel.Result;

namespace LinhGo.ERP.Domain.Users.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default); // Alias for consistency
    Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetUsersByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> IsUsernameUniqueAsync(string username, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<User?> GetWithCompaniesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PagedResult<User>> QueryAsync(QuerierParams queries, CancellationToken cancellationToken = default);
}

