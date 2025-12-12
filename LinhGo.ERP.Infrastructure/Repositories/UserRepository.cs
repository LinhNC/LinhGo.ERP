using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using LinhGo.ERP.Domain.Users.Entities;
using LinhGo.ERP.Domain.Users.Interfaces;
using LinhGo.ERP.Infrastructure.Data;
using LinhGo.SharedKernel.Querier;
using LinhGo.SharedKernel.Result;

namespace LinhGo.ERP.Infrastructure.Repositories;

public class UserRepository(ErpDbContext context) : GenericRepository<User>(context), IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        // Alias for GetByUserNameAsync for consistency
        return await GetByUserNameAsync(username, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(u => u.IsActive && !u.IsDeleted)
            .OrderBy(u => u.UserName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<User>> GetUsersByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(u => u.UserCompanies!.Any(uc => uc.CompanyId == companyId && uc.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(u => u.Email == email);

        if (excludeId.HasValue)
            query = query.Where(u => u.Id != excludeId.Value);

        return !await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> IsUsernameUniqueAsync(string username, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(u => u.UserName == username);

        if (excludeId.HasValue)
            query = query.Where(u => u.Id != excludeId.Value);

        return !await query.AnyAsync(cancellationToken);
    }

    public async Task<User?> GetWithCompaniesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(u => u.UserCompanies)
                .ThenInclude(uc => uc.Company)
            .Include(u => u.UserCompanies)
                .ThenInclude(uc => uc.Permissions)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<PagedResult<User>> QueryAsync(QuerierParams queries, CancellationToken cancellationToken = default)
    {
        // Use AsNoTracking for read-only queries - improves performance
        var baseQuery = DbSet.AsNoTracking();
        Expression<Func<User, User>> identitySelector = u => u;

        var searchBuilder = new QuerierBuilder<User>()
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
    /// Example: ?filter[email]=john@example.com&filter[isActive]=true
    /// </summary>
    private static readonly IReadOnlyDictionary<string, Expression<Func<User, object>>> FilterableFields =
        new Dictionary<string, Expression<Func<User, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["email"] = x => x.Email,
            ["username"] = x => x.UserName,
            ["firstName"] = x => x.FirstName,
            ["lastName"] = x => x.LastName,
            ["isActive"] = x => x.IsActive,
            ["emailConfirmed"] = x => x.EmailConfirmed,
            ["createdAt"] = x => x.CreatedAt,
            ["updatedAt"] = x => x.UpdatedAt
        };

    /// <summary>
    /// Defines fields that can be sorted
    /// Example: ?sort=-createdAt,email
    /// </summary>
    private static readonly IReadOnlyDictionary<string, LambdaExpression> SortableFields =
        new Dictionary<string, LambdaExpression>(StringComparer.OrdinalIgnoreCase)
        {
            ["email"] = (Expression<Func<User, object>>)(x => x.Email),
            ["username"] = (Expression<Func<User, object>>)(x => x.UserName),
            ["firstName"] = (Expression<Func<User, object>>)(x => x.FirstName),
            ["lastName"] = (Expression<Func<User, object>>)(x => x.LastName),
            ["isActive"] = (Expression<Func<User, object>>)(x => x.IsActive),
            ["createdAt"] = (Expression<Func<User, object>>)(x => x.CreatedAt),
            ["updatedAt"] = (Expression<Func<User, object>>)(x => x.UpdatedAt)
        };
    
    #endregion
}

