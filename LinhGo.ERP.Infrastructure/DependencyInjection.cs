using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        IConfiguration configuration)
    {
        var ss = configuration.GetConnectionString("DefaultConnection");
        // Database - PostgreSQL
        services.AddDbContext<ErpDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ErpDbContext).Assembly.GetName().Name)
            )
        );

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
}

