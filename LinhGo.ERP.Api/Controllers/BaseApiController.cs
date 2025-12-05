using LinhGo.ERP.Api.Services;
using LinhGo.ERP.Application.Common;
using LinhGo.ERP.Application.Common.Localization;
using Microsoft.AspNetCore.Mvc;

namespace LinhGo.ERP.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Gets the error message localizer from DI container (lazy initialization)
    /// </summary>
    protected IErrorMessageLocalizer ErrorMessageLocalizer =>
        field ??= HttpContext.RequestServices.GetRequiredService<IErrorMessageLocalizer>();

    /// <summary>
    /// Gets the correlation ID service from DI container (lazy initialization)
    /// </summary>
    protected ICorrelationIdService CorrelationIdService =>
        field ??= HttpContext.RequestServices.GetRequiredService<ICorrelationIdService>();
    
    protected ILanguageCodeService LanguageCodeService =>
        field ??= HttpContext.RequestServices.GetRequiredService<ILanguageCodeService>();
    

    /// <summary>
    /// Converts a Result to an appropriate HTTP response based on error type
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    protected ActionResult ToResponse<T>(Result<T> result)
    {
        if (!result.IsError)
        {
            return Ok(result.Value);
        }
        
        var correlationId = CorrelationIdService.GetCorrelationId();
        var error = result.FirstError;
        var languageCode = LanguageCodeService.GetCurrentLanguageCode();
            
        var errors = result.Errors.Select(e =>
        {
            // Use Parameters property if available, otherwise empty array
            var parameters = e.Parameters;
            
            return new
            {
                Code = e.Code,
                Description = ErrorMessageLocalizer.GetErrorMessage(e.Code, languageCode, parameters)
            };
        }).ToList();
            
        return error.Type switch
        {
            ErrorType.NotFound => NotFound(new
            {
                Type = error.Type.ToString(),
                Errors = errors,
                CorrelationId = correlationId
            }),
            ErrorType.Validation => BadRequest(new
            {
                Type = error.Type.ToString(),
                Errors = errors,
                CorrelationId = correlationId
            }),
            ErrorType.Conflict => Conflict(new
            {
                Type = error.Type.ToString(),
                Errors = errors,
                CorrelationId = correlationId
            }),
            ErrorType.Failure => BadRequest(new
            {
                Type = error.Type.ToString(),
                Errors = errors,
                CorrelationId = correlationId
            }),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new
            {
                Type = error.Type.ToString(),
                Errors = errors,
                CorrelationId = correlationId
            })
        };

    }

    /// <summary>
    /// Converts a Result to Created response for POST operations
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    protected ActionResult ToCreatedResponse<T>(Result<T> result, string actionName, object routeValues)
    {
        return result.IsError ? ToResponse(result) : CreatedAtAction(actionName, routeValues, result.Value);
    }

    /// <summary>
    /// Converts a Result to NoContent response for DELETE operations
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    protected ActionResult ToNoContentResponse(Result<bool> result)
    {
        return result.IsError ? ToResponse(result) : NoContent();
    }
}
