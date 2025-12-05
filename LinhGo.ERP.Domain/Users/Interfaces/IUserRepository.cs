using LinhGo.ERP.Domain.Common.Interfaces;
using LinhGo.ERP.Domain.Users.Entities;

namespace LinhGo.ERP.Domain.Users.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetUsersByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<User?> GetWithCompaniesAsync(Guid userId, CancellationToken cancellationToken = default);
}

