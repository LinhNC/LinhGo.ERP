using LinhGo.ERP.Api.Services;
using LinhGo.ERP.Application.Common.Errors;
using LinhGo.ERP.Application.Common.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LinhGo.ERP.Api.Filters;

/// <summary>
/// Validates model state and returns standardized localized error response
/// </summary>
public class ValidateModelStateAttribute(IErrorMessageLocalizer localizer, ICorrelationIdService correlationIdService, ILanguageCodeService languageCodeService)
    : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var languageCode = languageCodeService.GetCurrentLanguageCode();
            
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e => new
                {
                    Code = GeneralErrors.ValidationFailed,
                    Description = LocalizeValidationMessage(e.ErrorMessage, languageCode),
                    Field = x.Key
                }))
                .ToList();

            var response = new
            {
                Type = "Validation",
                Errors = errors,
                CorrelationId = correlationIdService.GetCorrelationId()
            };

            context.Result = new BadRequestObjectResult(response);
        }
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
            return localizer.GetErrorMessage(GeneralErrors.ValidationFailed, languageCode);
        }

        // Try to localize the message (in case it's an error code)
        // If not found in localizer, return the original message from FluentValidation
        var localizedMessage = localizer.GetErrorMessage(errorMessage, languageCode);
        
        // If localizer returns the same string (not found), use the original message
        return localizedMessage == errorMessage ? errorMessage : localizedMessage;
    }
}

