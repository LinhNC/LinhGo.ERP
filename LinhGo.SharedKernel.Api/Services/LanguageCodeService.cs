using System.Globalization;
using LinhGo.SharedKernel.Api.Constants;

namespace LinhGo.SharedKernel.Api.Services;

internal interface ILanguageCodeService
{
    string GetCurrentLanguageCode();
    string GetDefaultLanguageCode();
}

internal class LanguageCodeService(IHttpContextAccessor httpContextAccessor) : ILanguageCodeService
{
    public string GetCurrentLanguageCode()
    {
        var context = httpContextAccessor.HttpContext;
        
        if (context?.Items.TryGetValue(ApiConstants.LanguageHeaderName, out var language) == true)
        {
            return language?.ToString() ?? CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        }

        return GetDefaultLanguageCode();
    }

    public string GetDefaultLanguageCode()
    {
        return CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
    }
}