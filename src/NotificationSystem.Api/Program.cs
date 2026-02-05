using NotificationSystem.Api;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.AddSerilog();

try
{
    Log.Information("Starting NotificationSystem API");

    builder.Services.AddApiServices(builder.Configuration);
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddAuthorizationPolicies();
    builder.Services.AddCorsPolicy();

    var app = builder.Build();

    await app.InitializeDatabaseAsync();
    app.ConfigureHttpPipeline();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
