using backend.Data;

using Microsoft.EntityFrameworkCore;

using Serilog;

namespace backend.Extensions;

internal static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

        return services;
    }

    public static async Task CheckDatabaseConnectionAsync(
        this WebApplication app)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(app);

            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await dbContext.Database.CanConnectAsync().ConfigureAwait(false);

            Log.Information("SQLite database connection established successfully.");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "SQLite database connection failed.");
            throw;
        }
    }
}
