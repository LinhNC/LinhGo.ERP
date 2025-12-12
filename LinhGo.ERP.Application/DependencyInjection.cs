using LinhGo.ERP.Application.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection;
using LinhGo.ERP.Application.Services;
using LinhGo.SharedKernel.ResourceLocalizer;

namespace LinhGo.ERP.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => { }, typeof(DependencyInjection).Assembly);
        
        // Add localization with resource provider pattern
        services.AddResourceLocalizer(options =>
        {
            options.ResourcePath = Path.Combine("Resources", "Localization");
        });
        
        // Register services with distributed caching support
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserCompanyService, UserCompanyService>();
        
        return services;
    }
}