using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace LinhGo.ERP.Authorization.Common;

public static class ClaimBuilder
{
    public static List<Claim> BuildUserClaims(
        string userId, 
        string userName,
        string email,
        IEnumerable<string>? roles, 
        string? defaultCompanyId,
        IEnumerable<string>? companies,
        IEnumerable<string>? permissions = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, userName),
            new(ClaimTypes.Email, email),
            new (AuthClaims.UserId, userId),
            new (AuthClaims.UserName, userName),
            new (AuthClaims.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Add default company ID if provided (for multi-tenant context)
        if (!string.IsNullOrEmpty(defaultCompanyId))
        {
            claims.Add(new Claim(AuthClaims.DefaultCompanyId, defaultCompanyId));
        }

        // Add roles
        if (roles != null)
        {
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }
        
        // Add permissions
        if (permissions != null)
        {
            foreach (var permission in permissions)
            {
                claims.Add(new Claim(AuthClaims.Permission, permission));
            }
        }

        if (companies != null)
        {
            foreach (var company in companies)
            {
                claims.Add(new Claim(AuthClaims.CompanyId, company));
            }
        }

        return claims;
    }
}