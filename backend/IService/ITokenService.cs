using backend.Entities;

namespace backend.IService;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    string HashRefreshToken(string refreshToken);
    int GetAccessTokenExpiryMinutes();
    int GetRefreshTokenExpiryDays();
}
