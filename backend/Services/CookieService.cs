using backend.IService;

namespace backend.Services;

internal sealed class CookieService(IHttpContextAccessor httpContextAccessor, ITokenService tokenService) : ICookieService
{
    private const string AccessTokenCookieName = "jwt";
    private const string RefreshTokenCookieName = "refreshToken";

    public void AppendAccessTokenCookie(string accessToken)
    {
        httpContextAccessor.HttpContext?.Response.Cookies.Append(
            AccessTokenCookieName,
            accessToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddMinutes(tokenService.GetAccessTokenExpiryMinutes())
            });
    }

    public void AppendRefreshTokenCookie(string refreshToken, DateTime refreshTokenExpiresAtUtc)
    {
        httpContextAccessor.HttpContext?.Response.Cookies.Append(
            RefreshTokenCookieName,
            refreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                Expires = new DateTimeOffset(refreshTokenExpiresAtUtc, TimeSpan.Zero)
            });
    }

    public string? GetRefreshTokenCookie() => httpContextAccessor.HttpContext?.Request.Cookies[RefreshTokenCookieName];

    public void ClearAuthCookies()
    {
        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        };

        httpContextAccessor.HttpContext?.Response.Cookies.Delete(AccessTokenCookieName, options);

        httpContextAccessor.HttpContext?.Response.Cookies.Delete(RefreshTokenCookieName, options);
    }
}
