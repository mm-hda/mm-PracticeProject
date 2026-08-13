using backend.Extensions;
using backend.Filters;
using backend.Middleware;

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

builder.Services.AddHttpContextAccessor();

builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);

builder.Services.AddApplicationApiVersioning();

builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>());

builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddSwaggerServices();

builder.Services.AddApplicationServices();

builder.Services.AddForwardedHeadersConfiguration();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDevClient", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://127.0.0.1:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

await app.CheckDatabaseConnectionAsync().ConfigureAwait(false);

app.UseSwaggerMiddleware();

app.UseApplicationMiddleware();

app.UseCors("AllowAngularDevClient");

// app.UseMiddleware<JwtRefreshMiddleware>();

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
