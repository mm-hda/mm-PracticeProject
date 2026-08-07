using backend.Middleware;

namespace backend.Extensions;

internal static class MiddlewareExtensions
{
    public static WebApplication UseApplicationMiddleware(
        this WebApplication app)
    {
        app.UseMiddleware<GlobalExceptionMiddleware>();

        app.UseForwardedHeaders();

        app.UseMiddleware<RequestTracingMiddleware>();

        app.UseHttpLogging();

        return app;
    }
}
