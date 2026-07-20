using backend.Data;

using Microsoft.EntityFrameworkCore;

using Serilog;

namespace backend.Extensions;

internal static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(configuration.GetConnectionString("MyConnection")));

        return services;
    }

    public static async Task CheckDatabaseConnectionAsync(this WebApplication app)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(app);

            using var scope = app.Services.CreateScope();

            var dbContext =
                scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (!await dbContext.Database
                    .CanConnectAsync()
                    .ConfigureAwait(false))
            {
                Log.Fatal(
                    "Database connection failed. Unable to connect to SQLite database.");
            }
            else
            {
                Log.Information(
                    "Database connection established successfully.");
            }
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Database initialization failed.");
            throw;
        }
    }
}
