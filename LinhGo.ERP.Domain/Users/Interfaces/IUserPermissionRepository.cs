using LinhGo.ERP.Domain.Common.Interfaces;
using LinhGo.ERP.Domain.Users.Entities;

namespace LinhGo.ERP.Domain.Users.Interfaces;

public interface IUserPermissionRepository : IRepository<UserPermission>
{
    Task<IEnumerable<UserPermission>> GetByUserCompanyIdAsync(Guid userCompanyId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserPermission>> GetByUserAndCompanyAsync(Guid userId, Guid companyId, CancellationToken cancellationToken = default);
    Task<UserPermission?> GetByPermissionKeyAsync(Guid userCompanyId, string permissionKey, CancellationToken cancellationToken = default);
    Task<bool> HasPermissionAsync(Guid userId, Guid companyId, string permissionKey, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetUserPermissionKeysAsync(Guid userId, Guid companyId, CancellationToken cancellationToken = default);
    Task GrantPermissionAsync(Guid userCompanyId, string permissionKey, CancellationToken cancellationToken = default);
    Task RevokePermissionAsync(Guid userCompanyId, string permissionKey, CancellationToken cancellationToken = default);
    Task GrantPermissionsAsync(Guid userCompanyId, IEnumerable<string> permissionKeys, CancellationToken cancellationToken = default);
    Task RevokeAllPermissionsAsync(Guid userCompanyId, CancellationToken cancellationToken = default);
}

