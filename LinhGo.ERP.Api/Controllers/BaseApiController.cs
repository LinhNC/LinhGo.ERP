using LinhGo.ERP.Api.Models;
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
    protected IResourceLocalizer ResourceLocalizer =>
        field ??= HttpContext.RequestServices.GetRequiredService<IResourceLocalizer>();

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
            
        var errors = result.Errors.Select(e => new ErrorDetail
        {
            Code = e.Code,
            Description = ResourceLocalizer.GetMessage(e.Code, languageCode, e.Parameters)
        }).ToList();

        var errorResponse = new ErrorResponse
        {
            Type = error.Type.ToString(),
            Errors = errors,
            CorrelationId = correlationId
        };
            
        return error.Type switch
        {
            ErrorType.NotFound => NotFound(errorResponse),
            ErrorType.Validation => BadRequest(errorResponse),
            ErrorType.Conflict => Conflict(errorResponse),
            ErrorType.Failure => BadRequest(errorResponse),
            _ => StatusCode(StatusCodes.Status500InternalServerError, errorResponse)
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
