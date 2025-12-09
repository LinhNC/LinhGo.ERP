using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace LinhGo.ERP.Web.Controllers;

[Route("[controller]/[action]")]
public class CultureController : Controller
{
    public IActionResult Set(string culture, string redirectUri)
    {
        if (!string.IsNullOrEmpty(culture))
        {
            HttpContext.Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions 
                { 
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    Path = "/",
                    HttpOnly = false, // Need to be accessible for reading
                    SameSite = SameSiteMode.Lax
                }
            );
        }

        return LocalRedirect(redirectUri ?? "/");
    }
}

