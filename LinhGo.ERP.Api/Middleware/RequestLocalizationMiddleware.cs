using System.Globalization;

namespace LinhGo.ERP.Api.Middleware;

/// <summary>
/// Middleware to set the culture based on the Accept-Language header
/// </summary>
public class RequestLocalizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string[] _supportedLanguages = { "en", "vi" };
    private const string DefaultLanguage = "en";

    public RequestLocalizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var languageCode = GetLanguageFromHeader(context) ?? DefaultLanguage;
        
        // Set the culture for this request
        var culture = new CultureInfo(languageCode);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        
        // Store language in HttpContext items for later use
        context.Items["Language"] = languageCode;

        await _next(context);
    }

    private string? GetLanguageFromHeader(HttpContext context)
    {
        var acceptLanguage = context.Request.Headers.AcceptLanguage.FirstOrDefault();
        
        if (string.IsNullOrEmpty(acceptLanguage))
        {
            return null;
        }

        // Parse Accept-Language header (e.g., "vi-VN,vi;q=0.9,en-US;q=0.8,en;q=0.7")
        var languages = acceptLanguage
            .Split(',')
            .Select(lang => lang.Split(';')[0].Trim())
            .Select(lang => lang.Length > 2 ? lang.Substring(0, 2) : lang);

        // Find first supported language
        foreach (var lang in languages)
        {
            if (_supportedLanguages.Contains(lang, StringComparer.OrdinalIgnoreCase))
            {
                return lang.ToLower();
            }
        }

        return null;
    }
}

/// <summary>
/// Extension method to add the middleware to the pipeline
/// </summary>
public static class RequestLocalizationMiddlewareExtensions
{
    public static IApplicationBuilder UseLanguageLocalization(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLocalizationMiddleware>();
    }
}

