using backend.Data;

using Microsoft.EntityFrameworkCore;

using Serilog;

namespace backend.Extensions;

internal static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options => options.UseCosmos(
                configuration["CosmosDb:Endpoint"]!,
                configuration["CosmosDb:Key"]!,
                configuration["CosmosDb:DatabaseName"]!));

        return services;
    }
    public static async Task CheckDatabaseConnectionAsync(this WebApplication app)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(app);

            using var scope = app.Services.CreateScope();

            Console.WriteLine("Checking Cosmos DB connection...");
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.EnsureCreatedAsync().ConfigureAwait(false);
            Console.WriteLine("Cosmos DB connection established successfully.");
            Log.Information("Cosmos DB connection established successfully.");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Cosmos DB initialization failed.");

            throw;
        }
    }
}
