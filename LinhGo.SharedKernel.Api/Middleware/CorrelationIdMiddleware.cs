using System.Diagnostics;
using LinhGo.SharedKernel.Api.Constants;
using LinhGo.SharedKernel.Api.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LinhGo.SharedKernel.Api.Middleware;

/// <summary>
/// Middleware to handle correlation IDs for request tracing
/// Best Practice: Resolve scoped services from HttpContext.RequestServices
/// </summary>
internal class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Resolve scoped service from request scope
        var correlationIdService = context.RequestServices.GetRequiredService<ICorrelationIdService>();
        var correlationId = correlationIdService.GetOrCreateCorrelationId();

        // Add correlation ID to response headers
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(ApiConstants.CorrelationIdHeaderName))
            {
                context.Response.Headers[ApiConstants.CorrelationIdHeaderName] = correlationId;
            }
            return Task.CompletedTask;
        });

        // Add correlation ID to HttpContext items for easy access
        context.Items[ApiConstants.CorrelationIdHeaderName] = correlationId;

        // Add to Activity for distributed tracing
        Activity.Current?.SetTag("correlation_id", correlationId);

        // Log the correlation ID
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["RequestPath"] = context.Request.Path,
            ["RequestMethod"] = context.Request.Method
        }))
        {
            _logger.LogInformation(
                "Request started: {Method} {Path} [CorrelationId: {CorrelationId}]",
                context.Request.Method,
                context.Request.Path,
                correlationId);

            try
            {
                await _next(context);
            }
            finally
            {
                _logger.LogInformation(
                    "Request completed: {Method} {Path} [CorrelationId: {CorrelationId}] - Status: {StatusCode}",
                    context.Request.Method,
                    context.Request.Path,
                    correlationId,
                    context.Response.StatusCode);
            }
        }
    }
}


