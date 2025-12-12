using LinhGo.ERP.Authorization.Providers;
using LinhGo.ERP.Authorization.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LinhGo.ERP.Authorization.Extensions;

/// <summary>
/// Extension methods for registering multi-tenant authorization services
/// </summary>
public static class AuthorizationServiceCollectionExtensions
{
    /// <param name="services">The service collection</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds complete authentication and authorization services with JWT and multi-tenancy support
        /// </summary>
        /// <param name="configuration">Configuration containing JwtSettings</param>
        /// <returns>The service collection for chaining</returns>
        public IServiceCollection AddAuthenticationAndAuthorization(IConfiguration configuration)
        {
            // Register core authentication services
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
        
            // Register multi-tenant authorization services
            services.AddScoped<ITenantService, TenantService>();
        
            // HTTP context accessor is required for tenant resolution
            services.AddHttpContextAccessor();

            services.AddScoped<IUserContextProvider, UserContextProvider>();
        
            // Configure JWT authentication
            services.AddJwtAuthentication(configuration);
        
            // Configure authorization policies
            services.AddAuthorizationPolicies();
        
            return services;
        }

        /// <summary>
        /// Adds multi-tenant authorization services only (without authentication)
        /// Use this if you want to configure authentication separately
        /// </summary>
        /// <returns>The service collection for chaining</returns>
        public IServiceCollection AddMultiTenantAuthorization()
        {
            // Register core tenant service
            services.AddScoped<ITenantService, TenantService>();
        
            // HTTP context accessor is required for tenant resolution
            services.AddHttpContextAccessor();
        
            return services;
        }
    }
}



