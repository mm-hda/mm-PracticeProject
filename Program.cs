using backend.Extensions;
using backend.Filters;

using Asp.Versioning;

using FluentValidation;
using FluentValidation.AspNetCore;

using Microsoft.AspNetCore.Mvc;

using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddSerilogLogging(builder.Configuration);

builder.Services.AddDatabase(builder.Configuration);

builder.Services.AddApplicationHttpLogging();

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddAuthorization();

builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);

builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);

builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
        options.ApiVersionReader = new QueryStringApiVersionReader("api-version");
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>());

builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

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
