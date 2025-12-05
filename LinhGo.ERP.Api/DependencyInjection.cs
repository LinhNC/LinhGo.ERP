using LinhGo.ERP.Api.Extensions;
using LinhGo.ERP.Api.Middleware;
using LinhGo.ERP.Api.Services;
using LinhGo.ERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

namespace LinhGo.ERP.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        // Add services to the container.
        services.AddControllers(options =>
        {
            // Add global model validation filter
            options.Filters.Add<Filters.ValidateModelStateAttribute>();
        })
        .ConfigureApiBehaviorOptions(options =>
        {
            // Disable automatic model validation - we'll handle it with our filter
            options.SuppressModelStateInvalidFilter = true;
        });

        services.AddConfigurations(configuration);
        // Add CORS
        services.AddCors();

        // Add API Versioning
        services.AddApiVersioningWithExplorer();
    
        // Add OpenAPI/Scalar
        services.AddEndpointsApiExplorer();
        services.AddOpenApi();

        // Add HTTP Context Accessor for correlation ID access
        services.AddHttpContextAccessor();
        services.AddScoped<ICorrelationIdService, CorrelationIdService>();
        
        return services;
    }

    public static WebApplication BuildWebApplication(this WebApplication app, IConfiguration configuration)
    {
        // Run migrations at startup
        app.MigrateDatabase();
        
        // Configure the HTTP request pipeline.
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
                .WithTitle("LinhGo ERP Api")
                .WithClassicLayout()
                .WithTheme(Scalar.AspNetCore.ScalarTheme.Default)
                .WithDefaultHttpClient(Scalar.AspNetCore.ScalarTarget.CSharp, Scalar.AspNetCore.ScalarClient.HttpClient);
        });

        app.UseHttpsRedirection();
        app.UseCors("CorsPolicy");
        app.UseAuthorization();
        app.MapControllers();
        
        return app;
    }

    private static void MigrateDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<ErpDbContext>();
            
            // Apply any pending migrations
            if (context.Database.GetPendingMigrations().Any())
            {
                Console.WriteLine("Applying pending migrations...");
                context.Database.Migrate();
                Console.WriteLine("Migrations applied successfully.");
            }
            else
            {
                Console.WriteLine("Database is up to date.");
            }
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while migrating the database.");
            throw;
        }
    }
}