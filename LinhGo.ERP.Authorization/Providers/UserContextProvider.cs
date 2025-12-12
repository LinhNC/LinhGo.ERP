using System.Security.Claims;
using LinhGo.ERP.Authorization.Common;
using Microsoft.AspNetCore.Http;

namespace LinhGo.ERP.Authorization.Providers;

public interface IUserContextProvider
{
    string? UserId();
    string? UserName();
    string? Email();
    string? DefaultCompanyId();
    IEnumerable<string>? CompanyIds();
    IEnumerable<string>? Roles();
}

public class UserContextProvider(IHttpContextAccessor httpContextAccessor) : IUserContextProvider
{
    public string? UserId()
    {
        return httpContextAccessor.HttpContext?.User.FindFirst(AuthClaims.UserId)?.Value;
    }

    public string? UserName()
    {
        return httpContextAccessor.HttpContext?.User.FindFirst(AuthClaims.UserName)?.Value;
    }

    public string? Email()
    {
        return httpContextAccessor.HttpContext?.User.FindFirst(AuthClaims.Email)?.Value;
    }

    public string? DefaultCompanyId()
    {
        return httpContextAccessor.HttpContext?.User.FindFirst(AuthClaims.DefaultCompanyId)?.Value;
    }

    public IEnumerable<string>? CompanyIds()
    {
        var claims = httpContextAccessor.HttpContext?.User.FindAll(AuthClaims.CompanyId);
        return claims?.Select(c => c.Value);
    }

    public IEnumerable<string>? Roles()
    {
        var roles = httpContextAccessor.HttpContext?.User.FindAll(ClaimTypes.Role);
        return roles?.Select(r => r.Value);
    }
}