using LinhGo.ERP.Api.Configuarations;

namespace LinhGo.ERP.Api.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddCors(
        this IServiceCollection services, IConfiguration configuration)
    {
        var wildcardAnyOrigin = "*";
        var corsConfig = configuration.GetSection("CorsPolicySettings").Get<CorsPolicySettings>();
        
        if (corsConfig == null)
        {
            throw new InvalidOperationException("CorsPolicySettings configuration section is missing.");
        }
        
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy",
                policy =>
                {
                    if (corsConfig.Domains.Any(origin => origin == wildcardAnyOrigin)) {
                        policy.SetIsOriginAllowed(host => true);
                    }
                    else {
                        policy.WithOrigins(corsConfig.Domains);
                    }
                    policy.AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
        });
        return services;
    }
}