using LinhGo.SharedKernel.Api.Middleware;

namespace LinhGo.SharedKernel.Api.Extensions;

/// <summary>
/// Extension methods for configuring middleware
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Adds correlation ID middleware to the pipeline
    /// </summary>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
    
    /// <summary>
    /// Adds language localization middleware to the pipeline
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseLanguageLocalization(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestLocalizationMiddleware>();
    }
}

