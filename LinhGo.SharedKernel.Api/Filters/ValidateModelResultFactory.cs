using LinhGo.SharedKernel.Api.Services;
using LinhGo.SharedKernel.ResourceLocalizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Results;

namespace LinhGo.SharedKernel.Api.Filters;

public class ValidateModelResultFactory(
    IResourceLocalizer localizer,
    ILanguageCodeService languageCodeService,
    ICorrelationIdService correlationIdService)
    : IFluentValidationAutoValidationResultFactory
{
    public IActionResult CreateActionResult(ActionExecutingContext context,
        ValidationProblemDetails? validationProblemDetails)
    {
        var languageCode = languageCodeService.GetCurrentLanguageCode();

        var errors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .SelectMany(x => x.Value!.Errors.Select(e => new
            {
                Code = e.ErrorMessage,
                Description = LocalizeValidationMessage(e.ErrorMessage, languageCode),
            }))
            .ToList();

        var response = new
        {
            Type = "Validation",
            Errors = errors,
            CorrelationId = correlationIdService.GetCorrelationId()
        };

        return new BadRequestObjectResult(response);
    }
    
    /// <summary>
    /// Localizes validation error messages
    /// FluentValidation error messages can be localized by the localizer if they exist,
    /// otherwise returns the original message
    /// </summary>
    private string LocalizeValidationMessage(string? errorMessage, string languageCode)
    {
        if (string.IsNullOrEmpty(errorMessage))
        {
            return localizer.GetMessage("VALIDATE_FAILED", languageCode);
        }

        // Try to localize the message (in case it's an error code)
        // If not found in localizer, return the original message from FluentValidation
        var localizedMessage = localizer.GetMessage(errorMessage, languageCode);
        
        // If localizer returns the same string (not found), use the original message
        return localizedMessage == errorMessage ? errorMessage : localizedMessage;
    }
}