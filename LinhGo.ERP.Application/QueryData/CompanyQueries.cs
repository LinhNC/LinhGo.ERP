using System.Linq.Expressions;
using LinhGo.ERP.Domain.Companies.Entities;

namespace LinhGo.ERP.Application.QueryData;

public static class CompanyQueries
{
    public static readonly IReadOnlyDictionary<string, Expression<Func<Company, object>>> FilterableFields =
        new Dictionary<string, Expression<Func<Company, object>>>(StringComparer.OrdinalIgnoreCase)
        {
          ["code"] = x => x.Code,
          ["country"] = x => x.Country,
          ["industry"] = x => x.Industry,
          ["isActive"] = x => x.IsActive,
          ["city"] = x => x.City,
          ["subscriptionPlan"] = x => x.SubscriptionPlan
        };
    
    public static readonly IReadOnlyDictionary<string, LambdaExpression> SortableFields =
        new Dictionary<string, LambdaExpression>(StringComparer.OrdinalIgnoreCase)
        {
            { "Name", (Expression<Func<Company, object>>)(c => c.Name) },
            { "Code", (Expression<Func<Company, object>>)(c => c.Code) },
            { "CreatedAt", (Expression<Func<Company, object>>)(c => c.CreatedAt) },
            { "UpdatedAt", (Expression<Func<Company, object>>)(c => c.UpdatedAt) },
            { "Country", (Expression<Func<Company, object>>)(c => c.Country) },
            { "Industry", (Expression<Func<Company, object>>)(c => c.Industry) },
            { "IsActive", (Expression<Func<Company, object>>)(c => c.IsActive) },
            { "City", (Expression<Func<Company, object>>)(c => c.City) },
            { "SubscriptionPlan", (Expression<Func<Company, object>>)(c => c.SubscriptionPlan) }
        };
    
    public static readonly HashSet<string> AllowedIncludes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Name"
    };
}