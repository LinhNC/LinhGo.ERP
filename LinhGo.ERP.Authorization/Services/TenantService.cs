using LinhGo.ERP.Authorization.Common;
using LinhGo.ERP.Domain.Users.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LinhGo.ERP.Authorization.Services;

/// <summary>
/// Service to get current tenant (company) context from authenticated user
/// </summary>
public interface ITenantService
{
    Guid? GetCurrentCompanyId();
    Task<bool> HasAccessToCompanyAsync(Guid companyId);
    Task<string?> GetUserRoleInCompanyAsync(Guid companyId);
    Task<List<string>> GetUserPermissionsInCompanyAsync(Guid companyId);
}

public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ILogger<TenantService> _logger;

    public TenantService(
        IHttpContextAccessor httpContextAccessor,
        IUserCompanyRepository userCompanyRepository,
        ILogger<TenantService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _userCompanyRepository = userCompanyRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get current company ID from request header or user's default company
    /// </summary>
    public Guid? GetCurrentCompanyId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return null;

        // Try to get company ID from X-Company-Id header (preferred)
        if (httpContext.Request.Headers.TryGetValue("X-Company-Id", out var companyIdHeader))
        {
            if (Guid.TryParse(companyIdHeader.ToString(), out var companyId))
            {
                return companyId;
            }
        }

        // Fallback: Get user's default company from claims
        var defaultCompanyId = httpContext.User.FindFirst(AuthClaims.DefaultCompanyId)?.Value;
        if (!string.IsNullOrEmpty(defaultCompanyId) && Guid.TryParse(defaultCompanyId, out var defaultGuid))
        {
            return defaultGuid;
        }
        
        
        return null;
    }

    /// <summary>
    /// Check if current user has access to specified company
    /// </summary>
    public async Task<bool> HasAccessToCompanyAsync(Guid companyId)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return false;

        var userIdClaim = httpContext.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return false;
        }

        // Check if user has active assignment to this company
        var userCompany = await _userCompanyRepository.GetByUserAndCompanyAsync(userId, companyId);
        return userCompany != null && userCompany.IsActive;
    }

    /// <summary>
    /// Get user's role in specified company
    /// </summary>
    public async Task<string?> GetUserRoleInCompanyAsync(Guid companyId)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return null;

        var userIdClaim = httpContext.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        var userCompany = await _userCompanyRepository.GetByUserAndCompanyAsync(userId, companyId);
        return userCompany?.Role;
    }

    /// <summary>
    /// Get user's permissions in specified company
    /// </summary>
    public async Task<List<string>> GetUserPermissionsInCompanyAsync(Guid companyId)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return new List<string>();

        var userIdClaim = httpContext.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return new List<string>();
        }

        var userCompany = await _userCompanyRepository.GetByUserAndCompanyAsync(userId, companyId);
        if (userCompany == null || !userCompany.IsActive)
        {
            return new List<string>();
        }

        // Map role to permissions (this should come from database in production)
        return MapRoleToPermissions(userCompany.Role);
    }

    private List<string> MapRoleToPermissions(string role)
    {
        // TODO: Load from database - Permission table
        var permissions = new List<string>();

        switch (role.ToLower())
        {
            case "admin":
                permissions.AddRange(new[]
                {
                    "users.view", "users.create", "users.update", "users.delete", "users.manage",
                    "companies.view", "companies.create", "companies.update", "companies.delete", "companies.manage",
                    "transactions.view", "transactions.create", "transactions.update", "transactions.delete",
                    "reports.view", "reports.create", "reports.export",
                    "settings.view", "settings.update",
                    "audit.view"
                });
                break;
            case "manager":
                permissions.AddRange(new[]
                {
                    "users.view",
                    "companies.view",
                    "transactions.view", "transactions.create", "transactions.update",
                    "reports.view", "reports.export"
                });
                break;
            case "accountant":
                permissions.AddRange(new[]
                {
                    "transactions.view", "transactions.create", "transactions.update",
                    "reports.view", "reports.export"
                });
                break;
            case "employee":
                permissions.AddRange(new[]
                {
                    "transactions.view",
                    "reports.view"
                });
                break;
            case "viewer":
                permissions.AddRange(new[]
                {
                    "companies.view",
                    "reports.view"
                });
                break;
        }

        return permissions;
    }
}

