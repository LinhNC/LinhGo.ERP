using LinhGo.ERP.Api.Configurations;
using Microsoft.Extensions.Options;

namespace LinhGo.ERP.Api.Extensions;

public static class ServiceConfigurationsExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddConfigurations(IConfiguration configuration)
        {
            return services;
        }
    }
}