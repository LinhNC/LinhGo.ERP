using LinhGo.SharedKernel.Api.Constants;

namespace LinhGo.SharedKernel.Api.Services;

/// <summary>
/// Service interface for accessing correlation ID
/// </summary>
internal interface ICorrelationIdService
{
    string GetOrCreateCorrelationId();
    string GetCorrelationId();
}

/// <summary>
/// Service implementation for accessing correlation ID from HTTP context
/// </summary>
internal class CorrelationIdService(IHttpContextAccessor httpContextAccessor) : ICorrelationIdService
{
    public string GetOrCreateCorrelationId()
    {
        var correlationId = GetCorrelationId();
        return !string.IsNullOrEmpty(correlationId) ? correlationId : Guid.NewGuid().ToString();
    }

    public string GetCorrelationId()
    {
        var context = httpContextAccessor.HttpContext;
        
        if (context?.Items.TryGetValue(ApiConstants.CorrelationIdHeaderName, out var correlationId) == true)
        {
            return correlationId?.ToString() ?? string.Empty;
        }

        return string.Empty;
    }
}