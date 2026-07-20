using backend.Extensions;

using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddSerilogLogging(builder.Configuration);

builder.Services.AddDatabase(builder.Configuration);

builder.Services.AddApplicationHttpLogging();

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddAuthorization();

builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);

builder.Services.AddControllers();

builder.Services.AddSwaggerServices();

builder.Services.AddApplicationServices();

builder.Services.AddForwardedHeadersConfiguration();

var app = builder.Build();

await app.CheckDatabaseConnectionAsync().ConfigureAwait(false);

app.UseSwaggerMiddleware();

app.UseApplicationMiddleware();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

Log.Information("HRMS Application Started");

try
{
    Log.Information("Application Started");
    await app.RunAsync().ConfigureAwait(false);
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application Crashed");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync().ConfigureAwait(false);
}
