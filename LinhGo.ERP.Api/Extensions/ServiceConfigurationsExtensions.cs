using LinhGo.ERP.Api.Configuarations;
using Microsoft.Extensions.Options;

namespace LinhGo.ERP.Api.Extensions;

public static class ServiceConfigurationsExtensions
{
    public static IServiceCollection AddConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAndValidateSingleton<ServiceConfigurations>(configuration);
        services.AddAndValidateSingleton<ConnectionStrings>(configuration.GetRequiredSection(nameof(ConnectionStrings)));
        services.AddAndValidateSingleton<CorsPolicySettings>(configuration.GetRequiredSection(nameof(CorsPolicySettings)));
        
        return services;
    }

    public static IServiceCollection AddAndValidateSingleton<TOptions>(this IServiceCollection services, IConfiguration configuration)
        where TOptions : class, new()
    {
        services
            .AddOptions<TOptions>()
            .Bind(configuration)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton(serviceProvider => serviceProvider.GetRequiredService<IOptions<TOptions>>().Value);

        return services;
    }
}