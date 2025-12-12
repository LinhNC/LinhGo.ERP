using FluentValidation;
using LinhGo.ERP.Api.Extensions;
using LinhGo.ERP.Authorization.Extensions;
using LinhGo.ERP.Infrastructure.Data;
using LinhGo.SharedKernel.Api;
using Microsoft.EntityFrameworkCore;

namespace LinhGo.ERP.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCommonApiServices(configuration);
        
        services.AddConfigurations(configuration);
        
        // Add complete Authentication & Authorization from Authorization project
        services.AddAuthenticationAndAuthorization(configuration);
        
        services.AddValidatorsFromAssemblyContaining(typeof(Application.AssemblyInformation));
        
        return services;
    }

    public static WebApplication BuildWebApplication(this WebApplication app, IConfiguration configuration)
    {
        // Run migrations at startup
        app.MigrateDatabase();
            
        app.BuildCommonWebApplication(configuration);
        
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