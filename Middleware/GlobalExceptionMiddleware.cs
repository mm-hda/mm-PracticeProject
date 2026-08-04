using backend.GenericResponse;

using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;

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
        catch (CosmosException ex)
        {
            logger.LogError(ex, "Database dependency is not available.");
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsJsonAsync(ResponseResults<string>.Failure(CustomCodes.DatabaseDependencyNotFound)).ConfigureAwait(false);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database update failed.");
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsJsonAsync(ResponseResults<string>.Failure(CustomCodes.DatabaseDependencyNotFound)).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled application exception.");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(ResponseResults<string>.Failure(CustomCodes.InternalServerError)).ConfigureAwait(false);
            throw;
        }
    }
}

