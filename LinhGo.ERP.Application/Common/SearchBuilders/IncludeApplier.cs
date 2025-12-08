using Microsoft.EntityFrameworkCore;

namespace LinhGo.ERP.Application.Common.SearchBuilders;

// IncludeApplier.cs
public static class IncludeApplier
{
    public static IQueryable<T> ApplyIncludes<T>(IQueryable<T> query, string? includeCsv, HashSet<string> allowed)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(includeCsv)) return query;
        var parts = includeCsv.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
        foreach (var p in parts)
        {
            if (!allowed.Contains(p)) continue;
            query = query.Include(p); // string-based include - safe because of whitelist
        }
        return query;
    }
}
