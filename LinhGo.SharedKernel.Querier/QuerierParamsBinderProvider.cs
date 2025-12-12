using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LinhGo.SharedKernel.Querier;

/// <summary>
/// Provider that returns SearchQueryParamsBinder for SearchQueryParams model type
/// </summary>
public sealed class QuerierParamsBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType == typeof(QuerierParams))
        {
            return new QuerierParamsBinder();
        }

        return null;
    }
}