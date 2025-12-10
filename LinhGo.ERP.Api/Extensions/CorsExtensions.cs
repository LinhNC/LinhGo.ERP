using LinhGo.ERP.Api.Configurations;
using Microsoft.Extensions.Options;

namespace LinhGo.ERP.Api.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        var wildcardAnyOrigin = "*";
        
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy",
                policy =>
                {
                    // Resolve CorsPolicySettings from DI container at runtime
                    var serviceProvider = services.BuildServiceProvider();
                    var corsConfig = serviceProvider.GetRequiredService<IOptions<CorsPolicySettings>>().Value;
                    
                    if (corsConfig.Domains.Any(origin => origin == wildcardAnyOrigin))
                    {
                        policy.SetIsOriginAllowed(_ => true);
                    }
                    else
                    {
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