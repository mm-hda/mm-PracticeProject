using backend.Dto.RefreshTokenDtos;

namespace backend.IRepository;

public interface IRefreshTokenRepository
{
    Task AddRefreshTokenAsync(CreateRefreshTokenDto refreshTokenDto, CancellationToken cancellationToken);
    Task<RefreshTokenResponseDto?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<RefreshTokenResponseDto>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task UpdateLastUsedAtUtcAsync(Guid refreshTokenId, CancellationToken cancellationToken);
    Task DeactivateRefreshTokenAsync(Guid refreshTokenId, CancellationToken cancellationToken);
    Task DeactivateUserRefreshTokensAsync(Guid userId, CancellationToken cancellationToken);
}
