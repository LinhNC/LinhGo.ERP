using LinhGo.ERP.Authorization.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace LinhGo.ERP.Authorization.Filters;

/// <summary>
/// Authorization filter that ensures user has access to the specified company
/// Validates that the company ID in route matches the resolved company context
/// Usage: [RequireCompanyAccess]
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireCompanyAccessAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var tenantService = context.HttpContext.RequestServices
            .GetRequiredService<ITenantService>();

        var resolvedCompanyId = tenantService.GetCurrentCompanyId();
        
        if (!resolvedCompanyId.HasValue)
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                Type = "Unauthorized",
                Errors = new Dictionary<string, string[]>
                {
                    ["Company"] = new[] { "Company context is required. Please provide X-Company-Id header or access via company-specific endpoint." }
                }
            });
            return;
        }

        // Get company ID from route parameter if it exists
        if (context.HttpContext.Request.RouteValues.TryGetValue("companyId", out var routeCompanyId))
        {
            Guid routeCompanyGuid;
            if (routeCompanyId is Guid guid)
            {
                routeCompanyGuid = guid;
            }
            else if (!Guid.TryParse(routeCompanyId?.ToString(), out routeCompanyGuid))
            {
                context.Result = new BadRequestObjectResult(new
                {
                    Type = "BadRequest",
                    Errors = new Dictionary<string, string[]>
                    {
                        ["CompanyId"] = new[] { "Invalid company ID format in route" }
                    }
                });
                return;
            }

            // Ensure the resolved company ID matches the route company ID
            // This prevents users from accessing data for companies they specify in headers
            // that don't match the route parameter
            if (resolvedCompanyId.Value != routeCompanyGuid)
            {
                context.Result = new ObjectResult(new
                {
                    Type = "Forbidden",
                    Errors = new Dictionary<string, string[]>
                    {
                        ["Company"] = new[] { "Company context mismatch. The company in the route must match the company context." }
                    }
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }
        }

        // Verify user has active access to the company
        var hasAccess = await tenantService.HasAccessToCompanyAsync(resolvedCompanyId.Value);
        
        if (!hasAccess)
        {
            context.Result = new ObjectResult(new
            {
                Type = "Forbidden",
                Errors = new Dictionary<string, string[]>
                {
                    ["Company"] = new[] { "You do not have access to this company" }
                }
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}

/// <summary>
/// Authorization filter that requires specific permission in current company context
/// Validates that the company ID in route matches the resolved company context
/// Usage: [RequirePermission("transactions.create")]
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string _permission;

    public RequirePermissionAttribute(string permission)
    {
        _permission = permission;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var tenantService = context.HttpContext.RequestServices
            .GetRequiredService<ITenantService>();

        var resolvedCompanyId = tenantService.GetCurrentCompanyId();
        
        if (!resolvedCompanyId.HasValue)
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                Type = "Unauthorized",
                Errors = new Dictionary<string, string[]>
                {
                    ["Company"] = new[] { "Company context is required" }
                }
            });
            return;
        }

        // Validate route company ID matches resolved company ID
        if (context.HttpContext.Request.RouteValues.TryGetValue("companyId", out var routeCompanyId))
        {
            Guid routeCompanyGuid;
            if (routeCompanyId is Guid guid)
            {
                routeCompanyGuid = guid;
            }
            else if (!Guid.TryParse(routeCompanyId?.ToString(), out routeCompanyGuid))
            {
                context.Result = new BadRequestObjectResult(new
                {
                    Type = "BadRequest",
                    Errors = new Dictionary<string, string[]>
                    {
                        ["CompanyId"] = new[] { "Invalid company ID format in route" }
                    }
                });
                return;
            }

            if (resolvedCompanyId.Value != routeCompanyGuid)
            {
                context.Result = new ObjectResult(new
                {
                    Type = "Forbidden",
                    Errors = new Dictionary<string, string[]>
                    {
                        ["Company"] = new[] { "Company context mismatch. The company in the route must match the company context." }
                    }
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }
        }

        // Verify user has access to the company
        var hasAccess = await tenantService.HasAccessToCompanyAsync(resolvedCompanyId.Value);
        if (!hasAccess)
        {
            context.Result = new ObjectResult(new
            {
                Type = "Forbidden",
                Errors = new Dictionary<string, string[]>
                {
                    ["Company"] = new[] { "You do not have access to this company" }
                }
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            return;
        }

        // Check if user has the required permission in this company
        var permissions = await tenantService.GetUserPermissionsInCompanyAsync(resolvedCompanyId.Value);
        
        if (!permissions.Contains(_permission))
        {
            context.Result = new ObjectResult(new
            {
                Type = "Forbidden",
                Errors = new Dictionary<string, string[]>
                {
                    ["Permission"] = new[] { $"You do not have the required permission: {_permission}" }
                }
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}

/// <summary>
/// Authorization filter that requires specific role in current company context
/// Validates that the company ID in route matches the resolved company context
/// Usage: [RequireCompanyRole("Admin", "Manager")]
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireCompanyRoleAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string[] _roles;

    public RequireCompanyRoleAttribute(params string[] roles)
    {
        _roles = roles;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var tenantService = context.HttpContext.RequestServices
            .GetRequiredService<ITenantService>();

        var resolvedCompanyId = tenantService.GetCurrentCompanyId();
        
        if (!resolvedCompanyId.HasValue)
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                Type = "Unauthorized",
                Errors = new Dictionary<string, string[]>
                {
                    ["Company"] = new[] { "Company context is required" }
                }
            });
            return;
        }

        // Validate route company ID matches resolved company ID
        if (context.HttpContext.Request.RouteValues.TryGetValue("companyId", out var routeCompanyId))
        {
            Guid routeCompanyGuid;
            if (routeCompanyId is Guid guid)
            {
                routeCompanyGuid = guid;
            }
            else if (!Guid.TryParse(routeCompanyId?.ToString(), out routeCompanyGuid))
            {
                context.Result = new BadRequestObjectResult(new
                {
                    Type = "BadRequest",
                    Errors = new Dictionary<string, string[]>
                    {
                        ["CompanyId"] = new[] { "Invalid company ID format in route" }
                    }
                });
                return;
            }

            if (resolvedCompanyId.Value != routeCompanyGuid)
            {
                context.Result = new ObjectResult(new
                {
                    Type = "Forbidden",
                    Errors = new Dictionary<string, string[]>
                    {
                        ["Company"] = new[] { "Company context mismatch. The company in the route must match the company context." }
                    }
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }
        }

        // Verify user has access to the company
        var hasAccess = await tenantService.HasAccessToCompanyAsync(resolvedCompanyId.Value);
        if (!hasAccess)
        {
            context.Result = new ObjectResult(new
            {
                Type = "Forbidden",
                Errors = new Dictionary<string, string[]>
                {
                    ["Company"] = new[] { "You do not have access to this company" }
                }
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            return;
        }

        // Check if user has one of the required roles in this company
        var userRole = await tenantService.GetUserRoleInCompanyAsync(resolvedCompanyId.Value);
        
        if (string.IsNullOrEmpty(userRole) || !_roles.Contains(userRole, StringComparer.OrdinalIgnoreCase))
        {
            context.Result = new ObjectResult(new
            {
                Type = "Forbidden",
                Errors = new Dictionary<string, string[]>
                {
                    ["Role"] = new[] { $"You do not have the required role. Required: {string.Join(", ", _roles)}" }
                }
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}

