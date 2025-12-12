namespace LinhGo.ERP.Authorization.Common;

/// <summary>
/// Custom claim types for authentication
/// </summary>
public static class AuthClaims
{
    // Standard claims
    public const string UserId = "sub";
    public const string Email = "email";
    public const string UserName = "username";
    
    // Multi-tenant claims
    public const string DefaultCompanyId = "default_company_id";
    public const string CompanyId = "company_id";
    
    // Permission claim
    public const string Permission = "permission";
}

