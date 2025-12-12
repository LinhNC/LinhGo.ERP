using Microsoft.Extensions.DependencyInjection;

namespace LinhGo.SharedKernel.ResourceLocalizer;

public static class DependencyInjection
{
    public static IServiceCollection AddResourceLocalizer(this IServiceCollection services, Action<ResourceLocalizerConfiguration>? resourceLocalizerConfiguration = null)
    {
        var configuration = new ResourceLocalizerConfiguration();

        if (resourceLocalizerConfiguration != null)
        {
            resourceLocalizerConfiguration.Invoke(configuration);
            services.Configure(resourceLocalizerConfiguration);
        }
        
        services.AddScoped<IResourceLocalizer, ResourceLocalizer>();
        
        return services;
    }
}