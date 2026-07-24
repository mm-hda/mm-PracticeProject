using backend.GenericResponse;

namespace backend.Middleware;

internal sealed class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        try
        {
            await next(context).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError("Database dependency is not available: {ExceptionMessage}", ex.Message);
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(ResponseResults<string>.Failure(CustomCodes.DatabaseDependencyNotFound)).ConfigureAwait(false);
            return;
            throw;
        }
    }
}

