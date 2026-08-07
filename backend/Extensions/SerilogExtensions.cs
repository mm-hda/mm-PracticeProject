using System.Globalization;

using Serilog;

namespace backend.Extensions;

internal static class SerilogExtensions
{
    public static ConfigureHostBuilder AddSerilogLogging(
        this ConfigureHostBuilder host,
        IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
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

        host.UseSerilog();

        return host;
    }
}
