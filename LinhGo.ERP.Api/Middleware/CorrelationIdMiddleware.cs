using System.Diagnostics;
using LinhGo.ERP.Application.Common.Constants;

namespace LinhGo.ERP.Api.Middleware;

/// <summary>
/// Middleware to handle correlation IDs for request tracing
/// </summary>
public class CorrelationIdMiddleware
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
        var correlationId = GetOrCreateCorrelationId(context);
        
        // Add correlation ID to response headers
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(GeneralConstants.CorrelationIdHeaderName))
            {
                context.Response.Headers.Append(GeneralConstants.CorrelationIdHeaderName, correlationId);
            }
            return Task.CompletedTask;
        });

        // Add correlation ID to HttpContext items for easy access
        context.Items[GeneralConstants.CorrelationIdHeaderName] = correlationId;

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

    private string GetOrCreateCorrelationId(HttpContext context)
    {
        // Try to get correlation ID from request header
        if (context.Request.Headers.TryGetValue(GeneralConstants.CorrelationIdHeaderName, out var correlationId) 
            && !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId.ToString();
        }

        // Generate new correlation ID if not provided
        return Guid.NewGuid().ToString();
    }
}


