using backend.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.HttpOverrides;
using backend.Middleware;
using Serilog;
using backend.Extensions;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.File(
        path: "Logs/hrms-log-.txt",
        formatProvider: CultureInfo.InvariantCulture,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        rollOnFileSizeLimit: true,
        fileSizeLimitBytes: 10 * 1024 * 1024,
        outputTemplate:
            "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [TraceId:{TraceId}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("MyConnection")));

builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields =
        HttpLoggingFields.RequestMethod |
        HttpLoggingFields.RequestPath |
        HttpLoggingFields.ResponseStatusCode |
        HttpLoggingFields.RequestBody |
        HttpLoggingFields.ResponseBody;
});

// Add Authentication and Authorization services.
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddAuthorization();

builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//connect the controllers with the services
builder.Services.AddApplicationServices();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

try
{
    using var scope = app.Services.CreateScope();

    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!await dbContext.Database.CanConnectAsync().ConfigureAwait(false))
    {
        Log.Fatal("Database connection failed. Unable to connect to SQLite database.");
    }
    else
    {
        Log.Information("Database connection established successfully.");
    }
}
catch (Exception ex)
{
    Log.Fatal(ex, "Database initialization failed.");
    throw;
}

//Swagger middleware
app.UseSwagger();
app.UseSwaggerUI();

// Middleware for authentication and authorization
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseForwardedHeaders();
app.UseMiddleware<RequestTracingMiddleware>();
app.UseHttpLogging();
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
