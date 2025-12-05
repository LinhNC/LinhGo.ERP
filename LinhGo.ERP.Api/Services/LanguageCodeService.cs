using LinhGo.ERP.Application.Common.Constants;

namespace LinhGo.ERP.Api.Services;

public interface ILanguageCodeService
{
    string GetCurrentLanguageCode();
}

public class LanguageCodeService(IHttpContextAccessor httpContextAccessor) : ILanguageCodeService
{
    public string GetCurrentLanguageCode()
    {
        var context = httpContextAccessor.HttpContext;
        
        if (context?.Items.TryGetValue(GeneralConstants.LanguageHeaderName, out var language) == true)
        {
            return language?.ToString() ?? GeneralConstants.DefaultLanguage;
        }

        return GeneralConstants.DefaultLanguage;
    }
}