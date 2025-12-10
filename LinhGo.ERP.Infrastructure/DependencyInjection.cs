using LinhGo.ERP.Application.Abstractions.Caching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LinhGo.ERP.Domain.Common;
using LinhGo.ERP.Domain.Common.Interfaces;
using LinhGo.ERP.Domain.Companies.Interfaces;
using LinhGo.ERP.Domain.Customers.Interfaces;
using LinhGo.ERP.Domain.Inventory.Interfaces;
using LinhGo.ERP.Domain.Orders.Interfaces;
using LinhGo.ERP.Domain.Users.Interfaces;
using LinhGo.ERP.Infrastructure.Data;
using LinhGo.ERP.Infrastructure.Repositories;
using LinhGo.ERP.Infrastructure.Services;

namespace LinhGo.ERP.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // Database - PostgreSQL
        services.AddDbContext<ErpDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ErpDbContext).Assembly.GetName().Name)
            )
        );

        // Distributed Caching - Environment-specific configuration
        AddCaching(services, configuration, environment);

        // Tenant Context (Scoped - per request)
        services.AddScoped<ITenantContext, TenantContext>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Repositories
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserCompanyRepository, UserCompanyRepository>();
        services.AddScoped<IUserPermissionRepository, UserPermissionRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IStockRepository, StockRepository>();
        services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        return services;
    }

    /// <summary>
    /// Configure distributed caching based on environment
    /// Development: In-Memory Cache (fast, simple, no external dependencies)
    /// Staging/Production: Redis Cache (distributed, persistent, scalable)
    /// </summary>
    private static void AddCaching(
        IServiceCollection services, 
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
    }
}

