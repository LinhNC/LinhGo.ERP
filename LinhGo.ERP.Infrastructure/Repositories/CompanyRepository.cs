using Microsoft.EntityFrameworkCore;
using LinhGo.ERP.Domain.Companies.Entities;
using LinhGo.ERP.Domain.Companies.Interfaces;
using LinhGo.ERP.Infrastructure.Data;
using System.Linq.Expressions;

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
    
    public async Task<(IEnumerable<Company> Items, int TotalCount)> SearchAsync(
        string? searchTerm,
        string? currency,
        string? country,
        string? industry,
        bool? isActive,
        string? city,
        string? subscriptionPlan,
        int page,
        int pageSize,
        List<(string Field, string Direction)> sortSpecifications,
        CancellationToken cancellationToken = default)
    {
        // Start with base query
        var query = DbSet.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(lowerSearchTerm) ||
                c.Code.ToLower().Contains(lowerSearchTerm) ||
                (c.Email != null && c.Email.ToLower().Contains(lowerSearchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(currency))
        {
            query = query.Where(c => c.Currency == currency);
        }

        if (!string.IsNullOrWhiteSpace(country))
        {
            query = query.Where(c => c.Country == country);
        }

        if (!string.IsNullOrWhiteSpace(industry))
        {
            query = query.Where(c => c.Industry == industry);
        }

        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(c => c.City == city);
        }

        if (!string.IsNullOrWhiteSpace(subscriptionPlan))
        {
            query = query.Where(c => c.SubscriptionPlan == subscriptionPlan);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting - support multiple sort fields
        query = ApplyMultipleSort(query, sortSpecifications);

        // Apply pagination
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <summary>
    /// Applies multiple sort specifications to the query in order.
    /// Each sort field is applied sequentially for multi-level sorting.
    /// </summary>
    /// <param name="query">The IQueryable to sort</param>
    /// <param name="sortSpecifications">List of (Field, Direction) tuples for sorting</param>
    /// <returns>Sorted IQueryable</returns>
    private IQueryable<Company> ApplyMultipleSort(
        IQueryable<Company> query,
        List<(string Field, string Direction)> sortSpecifications)
    {
        // Default sort if none provided
        if (sortSpecifications == null || sortSpecifications.Count == 0)
        {
            return query.OrderBy(c => c.Name);
        }

        IOrderedQueryable<Company>? orderedQuery = null;

        foreach (var (field, direction) in sortSpecifications)
        {
            var (keySelector, isValid) = GetSortKeySelector(field);

            if (!isValid) continue;

            if (orderedQuery == null)
            {
                // First sort: use OrderBy/OrderByDescending
                orderedQuery = direction == "desc"
                    ? query.OrderByDescending(keySelector)
                    : query.OrderBy(keySelector);
            }
            else
            {
                // Subsequent sorts: use ThenBy/ThenByDescending
                orderedQuery = direction == "desc"
                    ? orderedQuery.ThenByDescending(keySelector)
                    : orderedQuery.ThenBy(keySelector);
            }
        }

        return orderedQuery ?? query;
    }

    /// <summary>
    /// Gets the appropriate property selector expression for a field name.
    /// </summary>
    /// <param name="field">Field name (case-insensitive)</param>
    /// <returns>Expression and validity flag</returns>
    private (Expression<Func<Company, IComparable>> KeySelector, bool IsValid) GetSortKeySelector(string field)
    {
        return field.ToLowerInvariant() switch
        {
            "code" => (c => c.Code, true),
            "name" => (c => c.Name, true),
            "createdat" => (c => c.CreatedAt, true),
            "updatedat" => (c => c.UpdatedAt ?? DateTime.MinValue, true),
            "currency" => (c => c.Currency ?? string.Empty, true),
            "country" => (c => c.Country ?? string.Empty, true),
            "industry" => (c => c.Industry ?? string.Empty, true),
            "city" => (c => c.City ?? string.Empty, true),
            "isactive" => (c => c.IsActive ? 1 : 0, true),
            _ => (c => c.Name, false) // Invalid field, return invalid flag
        };
    }
}

