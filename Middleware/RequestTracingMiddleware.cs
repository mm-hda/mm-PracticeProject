using System.Diagnostics;
using System.Collections.Generic;

namespace backend.Middleware;

internal sealed class RequestTracingMiddleware(RequestDelegate _next, ILogger<RequestTracingMiddleware> _logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();

        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-TraceId"] = context.TraceIdentifier;
            return Task.CompletedTask;
        });

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["TraceId"] = context.TraceIdentifier
        }))
        {
            _logger.LogInformation(
                "Request Started: {Method} {Path} {QueryString} from {RemoteIp}",
                context.Request.Method,
                context.Request.Path,
                context.Request.QueryString.HasValue ? context.Request.QueryString.Value : string.Empty,
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown");

            await _next(context).ConfigureAwait(false);

            if (context.Response.StatusCode == 401)
            {
                _logger.LogWarning(
                    "Unauthorized access attempt to {Path} from {IpAddress}",
                    context.Request.Path,
                    context.Connection.RemoteIpAddress);
            }
            else if (context.Response.StatusCode == 403)
            {
                _logger.LogWarning("Forbidden access attempt to {Path} from {IpAddress}",
                    context.Request.Path,
                    context.Connection.RemoteIpAddress);
            }

            sw.Stop();

            _logger.LogInformation(
                "Request Finished: {StatusCode} in {ElapsedMilliseconds}ms",
                context.Response.StatusCode,
                sw.ElapsedMilliseconds);
        }
    }
}
