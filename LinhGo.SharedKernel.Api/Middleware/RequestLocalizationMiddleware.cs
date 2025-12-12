using System.Globalization;
using LinhGo.SharedKernel.Api.Constants;
using LinhGo.SharedKernel.Api.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LinhGo.SharedKernel.Api.Middleware;

/// <summary>
/// Middleware to set the culture based on the Accept-Language header
/// Best Practice: Resolve scoped services from HttpContext.RequestServices
/// </summary>
internal class RequestLocalizationMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLocalizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Resolve scoped service from request scope
        var languageCodeService = context.RequestServices.GetRequiredService<ILanguageCodeService>();
        
        var languageCode = GetLanguageFromHeader(context, languageCodeService);
        
        // Set the culture for this request
        var culture = new CultureInfo(languageCode);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        
        // Store language in HttpContext items for later use
        context.Items[ApiConstants.LanguageHeaderName] = languageCode;

        await _next(context);
    }

    private static string GetLanguageFromHeader(HttpContext context, ILanguageCodeService languageCodeService)
    {
        var acceptLanguage = context.Request.Headers.AcceptLanguage.FirstOrDefault();
        
        if (string.IsNullOrEmpty(acceptLanguage))
        {
            return languageCodeService.GetDefaultLanguageCode();
        }

        // Parse Accept-Language header (e.g., "vi-VN,vi;q=0.9,en-US;q=0.8,en;q=0.7")
        var languages = acceptLanguage
            .Split(',')
            .Select(lang => lang.Split(';')[0].Trim())
            .Select(lang => lang.Length > 2 ? lang.Substring(0, 2) : lang);

        // Find first supported language
        foreach (var lang in languages)
        {
            return lang.ToLower();
        }

        return languageCodeService.GetDefaultLanguageCode();
    }
}



