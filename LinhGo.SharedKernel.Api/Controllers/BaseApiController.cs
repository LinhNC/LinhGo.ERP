using LinhGo.SharedKernel.Api.Models;
using LinhGo.SharedKernel.Api.Services;
using LinhGo.SharedKernel.ResourceLocalizer;
using LinhGo.SharedKernel.Result;
using Microsoft.AspNetCore.Mvc;

namespace LinhGo.SharedKernel.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Gets the error message localizer from DI container (lazy initialization)
    /// </summary>
    private IResourceLocalizer ResourceLocalizer =>
        field ??= HttpContext.RequestServices.GetRequiredService<IResourceLocalizer>();

    /// <summary>
    /// Gets the correlation ID service from DI container (lazy initialization)
    /// </summary>
    private ICorrelationIdService CorrelationIdService =>
        field ??= HttpContext.RequestServices.GetRequiredService<ICorrelationIdService>();
    
    private ILanguageCodeService LanguageCodeService =>
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
            ErrorType.Unauthorized => Unauthorized(errorResponse),
            ErrorType.NoPermission => StatusCode(StatusCodes.Status403Forbidden, errorResponse),
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
