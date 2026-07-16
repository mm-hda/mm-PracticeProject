using Microsoft.IdentityModel.Tokens;
using backend.Data;
using Microsoft.EntityFrameworkCore;
using backend.IService;
using backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.HttpOverrides;
using backend.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.File(
        "Logs/hrms-log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        rollOnFileSizeLimit: true,
        fileSizeLimitBytes: 10 * 1024 * 1024,
        outputTemplate:
        "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [TraceId:{TraceId}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("MyConnection"));
});

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
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        OnChallenge = async context =>
        {
            context.HandleResponse();

            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync("""
            {
                "status": false,
                "message": "Unauthorized request"
            }
            """);
        }
    };

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
        )
    };
});

builder.Services.AddAuthorization();

builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//connect the controllers with the services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<IPositionService, PositionService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IEmployeeProjectService, EmployeeProjectService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

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

    if (!await dbContext.Database.CanConnectAsync())
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
}

//Swagger middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application Crashed");
}
finally
{
    Log.CloseAndFlush();
}