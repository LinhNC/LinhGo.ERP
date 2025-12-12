using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LinhGo.SharedKernel.Cache;

public static class DependencyInjection
{
    /// <summary>
    /// Configure distributed caching based on environment
    /// Development: In-Memory Cache (fast, simple, no external dependencies)
    /// Staging/Production: Redis Cache (distributed, persistent, scalable)
    /// </summary>
    public static IServiceCollection AddCaching(
        this IServiceCollection services, 
        IConfiguration configuration, 
        IHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            // Development: Use in-memory distributed cache
            // Benefits: No setup required, fast, good for local development
            services.AddDistributedMemoryCache();
            
            Console.WriteLine("✓ Cache Configuration: In-Memory Cache (Development)");
        }
        else
        {
            // Production/Staging: Use Redis
            // Benefits: Distributed, persistent, production-ready
            var redisConnection = configuration.GetConnectionString("Redis");
            
            if (string.IsNullOrEmpty(redisConnection))
            {
                // Fallback to in-memory if Redis not configured
                Console.WriteLine("⚠ Warning: Redis connection string not found. Falling back to in-memory cache.");
                services.AddDistributedMemoryCache();
            }
            else
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnection;
                    options.InstanceName = configuration["Redis:InstanceName"];
                });
                
                Console.WriteLine($"✓ Cache Configuration: Redis Cache ({environment.EnvironmentName})");
                Console.WriteLine($"  Redis Instance: {configuration["Redis:InstanceName"] ?? "LinhGoERP:"}");
            }
        }
        
        // Register cache service (works with both in-memory and Redis)
        services.AddSingleton<ICacheService, CacheService>();
        
        return services;
    }
}