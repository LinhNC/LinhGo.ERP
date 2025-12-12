using LinhGo.SharedKernel.Api.Extensions;
using LinhGo.SharedKernel.Api.Filters;
using LinhGo.SharedKernel.Api.Services;
using LinhGo.SharedKernel.Querier;
using Scalar.AspNetCore;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

namespace LinhGo.SharedKernel.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddCommonApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add services to the container.
        services.AddControllers(options =>
            {
                options.ModelBinderProviders.Insert(0, new QuerierParamsBinderProvider());
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                // Disable automatic model validation - we'll handle it with our filter
                options.SuppressModelStateInvalidFilter = true;
            });

        services.AddConfigurations(configuration);
        
        // Add complete Authentication & Authorization from Authorization project
        //services.AddAuthenticationAndAuthorization(configuration);
        
        services.AddFluentValidationAutoValidation(cfg =>
        {
            // Replace the default result factory with a custom implementation.
            cfg.OverrideDefaultResultFactoryWith<ValidateModelResultFactory>();
        });
        
        // Add CORS
        services.AddCorsPolicy();

        // Add API Versioning
        services.AddApiVersioningWithExplorer();
    
        // Add OpenAPI/Scalar
        services.AddEndpointsApiExplorer();
        services.AddOpenApi(options =>
        {
            options.AddOperationTransformer<QuerierParamsOpenApiTransformer>();
        });
        
        services.AddHttpContextAccessor();
        services.AddScoped<ICorrelationIdService, CorrelationIdService>();
        services.AddScoped<ILanguageCodeService, LanguageCodeService>();

        return services;
    }
    
    public static WebApplication BuildCommonWebApplication(this WebApplication app, IConfiguration configuration)
    {
            
        // Add Correlation ID middleware early in the pipeline
        app.UseCorrelationId();
        
        // Add language localization middleware
        app.UseLanguageLocalization();

        // Map OpenAPI endpoint
        app.MapOpenApi();
    
        // Use Scalar for API documentation
        app.MapScalarApiReference(options =>
        {
            options
                .WithTitle("Api Contract")
                .WithClassicLayout()
                .WithTheme(ScalarTheme.Default)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });

        app.UseHttpsRedirection();
        app.UseCors("CorsPolicy");
            
        // Authentication must come before Authorization
        app.UseAuthentication();
        app.UseAuthorization();
        
        
        app.MapControllers();
        
        return app;
    }
}