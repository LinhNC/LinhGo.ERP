using LinhGo.ERP.Application.Common.Constants;

namespace LinhGo.ERP.Api.Services;

/// <summary>
/// Service interface for accessing correlation ID
/// </summary>
public interface ICorrelationIdService
{
    string GetCorrelationId();
}

/// <summary>
/// Service implementation for accessing correlation ID from HTTP context
/// </summary>
public class CorrelationIdService(IHttpContextAccessor httpContextAccessor) : ICorrelationIdService
{
    public string GetCorrelationId()
    {
        var context = httpContextAccessor.HttpContext;
        
        if (context?.Items.TryGetValue(GeneralConstants.CorrelationIdHeaderName, out var correlationId) == true)
        {
            return correlationId?.ToString() ?? string.Empty;
        }

        return string.Empty;
    }
}

