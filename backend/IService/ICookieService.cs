namespace backend.IService;

public interface ICookieService
{
    void AppendAccessTokenCookie(string accessToken);
    void AppendRefreshTokenCookie(string refreshToken, DateTime refreshTokenExpiresAtUtc);
    string? GetRefreshTokenCookie();
    void ClearAuthCookies();
}
