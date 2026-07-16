using backend.GenericResponse;

namespace backend.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(
            HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError("Database dependency is not available: {ExceptionMessage}", ex.Message);

                context.Response.StatusCode = 500;

                await context.Response.WriteAsJsonAsync(ResponseResults<string>.Failure(null, "Database dependency is not found"));
            }
        }
    }
}