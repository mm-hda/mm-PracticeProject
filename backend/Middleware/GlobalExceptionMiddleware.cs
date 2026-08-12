using backend.GenericResponse;

using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;

namespace backend.Middleware;

internal sealed class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(context);
            await next(context).ConfigureAwait(false);
        }
        catch (ArgumentNullException ex)
        {
            logger.LogWarning(ex, "Required argument was null.");

            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            await context.Response.WriteAsJsonAsync(ResponseResults<string>.Failure(CustomCodes.DtoIsNullOrEmpty)).ConfigureAwait(false);
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
    }
}

