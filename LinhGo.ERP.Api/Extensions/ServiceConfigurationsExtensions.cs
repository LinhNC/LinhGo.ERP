using LinhGo.ERP.Api.Configurations;
using Microsoft.Extensions.Options;

namespace LinhGo.ERP.Api.Extensions;

public static class ServiceConfigurationsExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddConfigurations(IConfiguration configuration)
        {
            services.AddAndValidateSingleton<ServiceConfigurations>(configuration);
            services.AddAndValidateSingleton<ConnectionStrings>(configuration.GetRequiredSection(nameof(ConnectionStrings)));
            services.AddAndValidateSingleton<CorsPolicySettings>(configuration.GetRequiredSection(nameof(CorsPolicySettings)));
        
            return services;
        }

        public IServiceCollection AddAndValidateSingleton<TOptions>(IConfiguration configuration)
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
}