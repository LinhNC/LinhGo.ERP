using Microsoft.EntityFrameworkCore;
using LinhGo.ERP.Domain.Companies.Entities;
using LinhGo.ERP.Domain.Companies.Interfaces;
using LinhGo.ERP.Infrastructure.Data;
using System.Linq.Expressions;
using LinhGo.ERP.Application.Common.SearchBuilders;
using LinhGo.ERP.Domain.Common;

namespace LinhGo.ERP.Infrastructure.Repositories;

public class CompanyRepository(ErpDbContext context) : GenericRepository<Company>(context), ICompanyRepository
{
     public async Task<Company?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Settings)
            .FirstOrDefaultAsync(c => c.Code == code, cancellationToken);
    }

    public async Task<IEnumerable<Company>> GetActiveCompaniesAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(c => c.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(c => c.Code == code);

        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);

        return !await query.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Searches companies with filtering, sorting, and pagination
    /// Performance optimizations: AsNoTracking, no unnecessary projections at this level
    /// </summary>
    public async Task<PagedResult<Company>> SearchAsync(SearchQueryParams queries, CancellationToken cancellationToken = default)
    {
        // Use AsNoTracking for read-only queries - significantly improves performance
        // by avoiding the overhead of change tracking for read-only operations
        var baseQuery = DbSet.AsNoTracking();
        Expression<Func<Company, Company>> identitySelector = c => c;

        var searchBuilder = new SearchBuilder<Company>()
            .WithSource(baseQuery)
            .WithQueryParams(queries)
            .WithSelector(identitySelector)
            .WithFilterMapping(FilterableFields)
            .WithSortMapping(SortableFields);

        var result = await searchBuilder.BuildAsync(cancellationToken);
        return result;
    }
    
    #region Search Configuration
    
    /// <summary>
    /// Defines fields that can be filtered using query parameters
    /// Example: ?filter[country]=USA&filter[isActive]=true
    /// </summary>
    private static readonly IReadOnlyDictionary<string, Expression<Func<Company, object>>> FilterableFields =
        new Dictionary<string, Expression<Func<Company, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["code"] = x => x.Code,
            ["country"] = x => x.Country,
            ["industry"] = x => x.Industry,
            ["isActive"] = x => x.IsActive,
            ["city"] = x => x.City,
            ["state"] = x => x.State,
            ["subscriptionPlan"] = x => x.SubscriptionPlan,
            ["currency"] = x => x.Currency,
            ["language"] = x => x.Language
        };
    
    /// <summary>
    /// Defines fields that can be used for sorting
    /// Example: ?sort=-createdAt,name (descending by createdAt, then ascending by name)
    /// </summary>
    private static readonly IReadOnlyDictionary<string, LambdaExpression> SortableFields =
        new Dictionary<string, LambdaExpression>(StringComparer.OrdinalIgnoreCase)
        {
            { "name", (Expression<Func<Company, object>>)(c => c.Name) },
            { "code", (Expression<Func<Company, object>>)(c => c.Code) },
            { "createdAt", (Expression<Func<Company, object>>)(c => c.CreatedAt) },
            { "updatedAt", (Expression<Func<Company, object>>)(c => c.UpdatedAt) },
            { "country", (Expression<Func<Company, object>>)(c => c.Country) },
            { "industry", (Expression<Func<Company, object>>)(c => c.Industry) },
            { "city", (Expression<Func<Company, object>>)(c => c.City) },
            { "subscriptionPlan", (Expression<Func<Company, object>>)(c => c.SubscriptionPlan) },
            { "isActive", (Expression<Func<Company, object>>)(c => c.IsActive) }
        };
    
    /// <summary>
    /// Defines fields that can be searched using full-text search
    /// Example: ?q=tech (searches in name, code, email, etc.)
    /// </summary>
    private static readonly List<Expression<Func<Company, string>>> SearchableFields = new()
    {
        c => c.Name,
        c => c.Code,
        c => c.Email ?? string.Empty,
        c => c.Phone ?? string.Empty,
        c => c.City ?? string.Empty,
        c => c.Country ?? string.Empty,
        c => c.Industry ?? string.Empty
    };
    
    #endregion
}

