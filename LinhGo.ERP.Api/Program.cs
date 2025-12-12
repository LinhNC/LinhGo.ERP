using LinhGo.ERP.Api;
using LinhGo.ERP.Application;
using LinhGo.ERP.Infrastructure;
using Serilog;
using LoggerExtensions = LinhGo.SharedKernel.Api.Extensions.LoggerExtensions;

try
{
    // Configure Serilog early
    Log.Logger = LoggerExtensions.CreateBootstrapLogger();
    
    Log.Information("Starting LinhGo ERP API...");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog for logging
    builder.Host.UseSerilog();

    // Add Api and Application and Infrastructure layers
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
    builder.Services.AddApi(builder.Configuration);

    var app = builder.Build();
    try
    {
        app = app.BuildWebApplication(builder.Configuration);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error during BuildWebApplication");
        throw;
    }
    
    Log.Information("LinhGo ERP API started successfully");
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}




