using Microsoft.AspNetCore.HttpLogging;

namespace backend.Extensions;

internal static class HttpLoggingExtensions
{
    public static IServiceCollection AddApplicationHttpLogging(
        this IServiceCollection services)
    {
        services.AddHttpLogging(options =>
        {
            options.LoggingFields =
                HttpLoggingFields.RequestMethod |
                HttpLoggingFields.RequestPath |
                HttpLoggingFields.ResponseStatusCode |
                HttpLoggingFields.RequestBody |
                HttpLoggingFields.ResponseBody;
        });

        return services;
    }
}
