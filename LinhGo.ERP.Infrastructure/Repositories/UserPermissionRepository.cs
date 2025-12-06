using Microsoft.EntityFrameworkCore;
using LinhGo.ERP.Domain.Users.Entities;
using LinhGo.ERP.Domain.Users.Interfaces;
using LinhGo.ERP.Infrastructure.Data;

namespace LinhGo.ERP.Infrastructure.Repositories;

public class UserPermissionRepository(ErpDbContext context)
    : GenericRepository<UserPermission>(context), IUserPermissionRepository
{
    public async Task<IEnumerable<UserPermission>> GetByUserCompanyIdAsync(Guid userCompanyId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(up => up.UserCompanyId == userCompanyId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserPermission>> GetByUserAndCompanyAsync(Guid userId, Guid companyId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(up => up.UserCompany)
            .Where(up => up.UserCompany!.UserId == userId && up.UserCompany.CompanyId == companyId)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserPermission?> GetByPermissionKeyAsync(Guid userCompanyId, string permissionKey, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(up => up.UserCompanyId == userCompanyId && up.PermissionKey == permissionKey, cancellationToken);
    }

    public async Task<bool> HasPermissionAsync(Guid userId, Guid companyId, string permissionKey, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(up => up.UserCompany)
            .AnyAsync(up => up.UserCompany!.UserId == userId && 
                           up.UserCompany.CompanyId == companyId && 
                           up.UserCompany.IsActive &&
                           up.PermissionKey == permissionKey, 
                      cancellationToken);
    }

    public async Task<IEnumerable<string>> GetUserPermissionKeysAsync(Guid userId, Guid companyId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(up => up.UserCompany)
            .Where(up => up.UserCompany!.UserId == userId && 
                        up.UserCompany.CompanyId == companyId && 
                        up.UserCompany.IsActive)
            .Select(up => up.PermissionKey)
            .ToListAsync(cancellationToken);
    }

    public async Task GrantPermissionAsync(Guid userCompanyId, string permissionKey, CancellationToken cancellationToken = default)
    {
        var existing = await GetByPermissionKeyAsync(userCompanyId, permissionKey, cancellationToken);
        
        if (existing == null)
        {
            var permission = new UserPermission
            {
                UserCompanyId = userCompanyId,
                PermissionKey = permissionKey
            };
            await DbSet.AddAsync(permission, cancellationToken);
            await Context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RevokePermissionAsync(Guid userCompanyId, string permissionKey, CancellationToken cancellationToken = default)
    {
        var permission = await GetByPermissionKeyAsync(userCompanyId, permissionKey, cancellationToken);
        
        if (permission != null)
        {
            DbSet.Remove(permission);
            await Context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task GrantPermissionsAsync(Guid userCompanyId, IEnumerable<string> permissionKeys, CancellationToken cancellationToken = default)
    {
        var existingPermissions = await GetByUserCompanyIdAsync(userCompanyId, cancellationToken);
        var existingKeys = existingPermissions.Select(p => p.PermissionKey).ToHashSet();

        var newPermissions = permissionKeys
            .Where(key => !existingKeys.Contains(key))
            .Select(key => new UserPermission
            {
                UserCompanyId = userCompanyId,
                PermissionKey = key
            });

        if (newPermissions.Any())
        {
            await DbSet.AddRangeAsync(newPermissions, cancellationToken);
            await Context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RevokeAllPermissionsAsync(Guid userCompanyId, CancellationToken cancellationToken = default)
    {
        var permissions = await GetByUserCompanyIdAsync(userCompanyId, cancellationToken);
        
        if (permissions.Any())
        {
            DbSet.RemoveRange(permissions);
            await Context.SaveChangesAsync(cancellationToken);
        }
    }
}

