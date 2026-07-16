using System.Diagnostics;
using System.Collections.Generic;

namespace backend.Middleware
{
    public class RequestTracingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestTracingMiddleware> _logger;

        public RequestTracingMiddleware(
            RequestDelegate next,
            ILogger<RequestTracingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var sw = Stopwatch.StartNew();

            context.Response.OnStarting(() =>
            {
                // Expose the trace id to clients for easier correlation
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

                await _next(context);

                if (context.Response.StatusCode == 401)
                {
                    _logger.LogWarning(
                        "Unauthorized access attempt to {Path} from {IpAddress}",
                        context.Request.Path,
                        context.Connection.RemoteIpAddress);
                }
                else if (context.Response.StatusCode == 403)
                {
                    _logger.LogWarning(
                        "Forbidden access attempt to {Path} from {IpAddress}",
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
}