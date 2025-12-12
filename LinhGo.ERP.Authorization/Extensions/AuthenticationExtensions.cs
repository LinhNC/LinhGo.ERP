using System.Text;
using LinhGo.ERP.Authorization.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace LinhGo.ERP.Authorization.Extensions;

/// <summary>
/// Extension methods for configuring JWT authentication
/// </summary>
public static class AuthenticationExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds JWT Bearer authentication to the service collection
        /// </summary>
        public IServiceCollection AddJwtAuthentication(IConfiguration configuration)
        {
            // Register and validate JWT settings from configuration
            var jwtSettingsSection = configuration.GetSection(nameof(JwtSettings));
            services.AddOptions<JwtSettings>()
                .Bind(jwtSettingsSection)
                .ValidateDataAnnotations()
                .ValidateOnStart();
        
            var jwtSettings = jwtSettingsSection.Get<JwtSettings>()
                              ?? throw new InvalidOperationException("JwtSettings configuration is missing");

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.RequireHttpsMetadata = true; // Set to false for development if needed
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                        ClockSkew = TimeSpan.Zero // No tolerance for expired tokens
                    };

                    // Support token from cookie for Blazor
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            // Check Authorization header first
                            if (!string.IsNullOrEmpty(context.Request.Headers["Authorization"]))
                            {
                                return Task.CompletedTask;
                            }

                            // Fall back to cookie
                            var token = context.Request.Cookies["access_token"];
                            if (!string.IsNullOrEmpty(token))
                            {
                                context.Token = token;
                            }

                            return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = context =>
                        {
                            if (context.Exception is SecurityTokenExpiredException)
                            {
                                context.Response.Headers["Token-Expired"] = "true";
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            return services;
        }

        /// <summary>
        /// Adds authorization policies for role and permission-based access control
        /// </summary>
        public IServiceCollection AddAuthorizationPolicies()
        {
            services.AddAuthorizationBuilder()
                // Role-based policies
                .AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"))
                .AddPolicy("RequireManagerRole", policy => policy.RequireRole("Admin", "Manager"))
                .AddPolicy("RequireEmployeeRole", policy => policy.RequireRole("Admin", "Manager", "Employee"))
            
                // Permission-based policies
                .AddPolicy("CanManageUsers", policy => 
                    policy.RequireAssertion(context => 
                        context.User.HasClaim("permission", "users.manage") || 
                        context.User.IsInRole("Admin")))
            
                .AddPolicy("CanManageCompanies", policy => 
                    policy.RequireAssertion(context => 
                        context.User.HasClaim("permission", "companies.manage") || 
                        context.User.IsInRole("Admin")))
            
                .AddPolicy("CanViewReports", policy => 
                    policy.RequireAssertion(context => 
                        context.User.HasClaim("permission", "reports.view") || 
                        context.User.IsInRole("Admin") ||
                        context.User.IsInRole("Manager")));

            return services;
        }
    }
}

