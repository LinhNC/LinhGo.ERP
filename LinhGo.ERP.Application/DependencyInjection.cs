using FluentValidation;
using LinhGo.ERP.Application.Abstractions.Services;
using LinhGo.ERP.Application.Common.Localization;
using Microsoft.Extensions.DependencyInjection;
using LinhGo.ERP.Application.Services;
using LinhGo.ERP.Application.Validators.Companies;

namespace LinhGo.ERP.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);
        services.AddValidatorsFromAssemblyContaining<CreateCompanyValidator>();
        services.AddValidatorsFromAssemblyContaining(typeof(AssemblyInformation));
        
        // Add localization with resource provider pattern
        services.AddSingleton<IErrorMessageResourceProvider, JsonErrorMessageResourceProvider>();
        services.AddSingleton<IErrorMessageLocalizer, ErrorMessageLocalizer>();
        
        services.AddScoped<ICompanyService, CompanyService>();
       
        return services;
    }
}