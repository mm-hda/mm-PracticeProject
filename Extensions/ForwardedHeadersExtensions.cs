using Microsoft.AspNetCore.HttpOverrides;

namespace backend.Extensions;

internal static class ForwardedHeadersExtensions
{
    public static IServiceCollection AddForwardedHeadersConfiguration(
        this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor |
                ForwardedHeaders.XForwardedProto;
        });

        return services;
    }
}
