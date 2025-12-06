using LinhGo.ERP.Application.Abstractions.Services;
using LinhGo.ERP.Application.Common.Localization;
using Microsoft.Extensions.DependencyInjection;
using LinhGo.ERP.Application.Services;

namespace LinhGo.ERP.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);
        
        // Add localization with resource provider pattern
        services.AddResourceLocalizer(options =>
        {
            options.ResourcePath = Path.Combine("Resources", "Localization");
        });
        
        services.AddScoped<ICompanyService, CompanyService>();
        
        return services;
    }
}