using backend.IService;

namespace backend.Middleware;

public sealed class JwtRefreshMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IAuthService authService)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(authService);

        var path = context.Request.Path.Value ?? string.Empty;

        if (
            path.Contains("/login", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("/register", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("/logout", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("/refresh", StringComparison.OrdinalIgnoreCase))
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        var accessToken = context.Request.Cookies["jwt"];
        var refreshToken = context.Request.Cookies["refreshToken"];

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        if (string.IsNullOrWhiteSpace(accessToken) || IsJwtExpired(accessToken))
        {

            var response = await authService
                .RefreshTokenAsync(context.RequestAborted)
                .ConfigureAwait(false);

            if (!response.IsSuccess)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
        }

        await next(context).ConfigureAwait(false);
    }

    private static bool IsJwtExpired(string token)
    {
        var jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().ReadJwtToken(token);

        return jwt.ValidTo <= DateTime.UtcNow;
    }
}
