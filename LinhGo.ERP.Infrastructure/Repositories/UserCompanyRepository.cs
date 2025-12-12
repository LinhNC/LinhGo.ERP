using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using LinhGo.ERP.Domain.Users.Entities;
using LinhGo.ERP.Domain.Users.Interfaces;
using LinhGo.ERP.Infrastructure.Data;
using LinhGo.SharedKernel.Querier;
using LinhGo.SharedKernel.Result;

namespace LinhGo.ERP.Infrastructure.Repositories;

public class UserCompanyRepository(ErpDbContext context)
    : GenericRepository<UserCompany>(context), IUserCompanyRepository
{
    public async Task<UserCompany?> GetByUserAndCompanyAsync(Guid userId, Guid companyId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(uc => uc.Company)
            .Include(uc => uc.Permissions)
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CompanyId == companyId, cancellationToken);
    }

    public async Task<IEnumerable<UserCompany>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(uc => uc.Company)
            .Where(uc => uc.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserCompany>> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(uc => uc.User)
            .Where(uc => uc.CompanyId == companyId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserCompany>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(uc => uc.Company)
            .Where(uc => uc.UserId == userId && uc.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserCompany>> GetActiveByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(uc => uc.User)
            .Where(uc => uc.CompanyId == companyId && uc.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsUserAssignedToCompanyAsync(Guid userId, Guid companyId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(uc => uc.UserId == userId && uc.CompanyId == companyId && uc.IsActive, cancellationToken);
    }

    public async Task<UserCompany?> GetWithPermissionsAsync(Guid userId, Guid companyId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(uc => uc.Company)
            .Include(uc => uc.Permissions)
            .Include(uc => uc.User)
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CompanyId == companyId, cancellationToken);
    }

    public async Task AssignUserToCompanyAsync(Guid userId, Guid companyId, string role, CancellationToken cancellationToken = default)
    {
        var existing = await GetByUserAndCompanyAsync(userId, companyId, cancellationToken);
        
        if (existing != null)
        {
            // User already assigned, just update role and activate
            existing.Role = role;
            existing.IsActive = true;
            DbSet.Update(existing);
        }
        else
        {
            // Create new assignment
            var userCompany = new UserCompany
            {
                UserId = userId,
                CompanyId = companyId,
                Role = role,
                IsActive = true
            };
            await DbSet.AddAsync(userCompany, cancellationToken);
        }
        
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveUserFromCompanyAsync(Guid userId, Guid companyId, CancellationToken cancellationToken = default)
    {
        var userCompany = await GetByUserAndCompanyAsync(userId, companyId, cancellationToken);
        
        if (userCompany != null)
        {
            // Soft delete by marking as inactive
            userCompany.IsActive = false;
            DbSet.Update(userCompany);
            await Context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateRoleAsync(Guid userId, Guid companyId, string role, CancellationToken cancellationToken = default)
    {
        var userCompany = await GetByUserAndCompanyAsync(userId, companyId, cancellationToken);
        
        if (userCompany != null)
        {
            userCompany.Role = role;
            DbSet.Update(userCompany);
            await Context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<PagedResult<UserCompany>> QueryAsync(QuerierParams queries, CancellationToken cancellationToken = default)
    {
        // Use AsNoTracking for read-only queries - improves performance
        var baseQuery = DbSet
            .AsNoTracking()
            .Include(uc => uc.User)
            .Include(uc => uc.Company);
        
        Expression<Func<UserCompany, UserCompany>> identitySelector = uc => uc;

        var searchBuilder = new QuerierBuilder<UserCompany>()
            .WithSource(baseQuery)
            .WithQueryParams(queries)
            .WithSelector(identitySelector)
            .WithFilterMappingFields(FilterableFields)
            .WithSortMappingFields(SortableFields);

        var result = await searchBuilder.BuildAsync(cancellationToken);
        return result;
    }

    #region Querier Configuration

    /// <summary>
    /// Defines fields that can be filtered
    /// Example: ?filter[userId]=guid&filter[isActive]=true
    /// </summary>
    private static readonly IReadOnlyDictionary<string, Expression<Func<UserCompany, object>>> FilterableFields =
        new Dictionary<string, Expression<Func<UserCompany, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["userId"] = x => x.UserId,
            ["companyId"] = x => x.CompanyId,
            ["role"] = x => x.Role,
            ["isActive"] = x => x.IsActive,
            ["isDefaultCompany"] = x => x.IsDefaultCompany,
            ["joinedAt"] = x => x.JoinedAt,
            ["leftAt"] = x => x.LeftAt,
            ["createdAt"] = x => x.CreatedAt
        };

    /// <summary>
    /// Defines fields that can be sorted
    /// Example: ?sort=-joinedAt,role
    /// </summary>
    private static readonly IReadOnlyDictionary<string, LambdaExpression> SortableFields =
        new Dictionary<string, LambdaExpression>(StringComparer.OrdinalIgnoreCase)
        {
            ["userId"] = (Expression<Func<UserCompany, object>>)(x => x.UserId),
            ["companyId"] = (Expression<Func<UserCompany, object>>)(x => x.CompanyId),
            ["role"] = (Expression<Func<UserCompany, object>>)(x => x.Role),
            ["isActive"] = (Expression<Func<UserCompany, object>>)(x => x.IsActive),
            ["isDefaultCompany"] = (Expression<Func<UserCompany, object>>)(x => x.IsDefaultCompany),
            ["joinedAt"] = (Expression<Func<UserCompany, object>>)(x => x.JoinedAt),
            ["leftAt"] = (Expression<Func<UserCompany, object>>)(x => x.LeftAt),
            ["createdAt"] = (Expression<Func<UserCompany, object>>)(x => x.CreatedAt)
        };

    #endregion
}

