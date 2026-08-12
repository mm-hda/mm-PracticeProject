using backend.Entities;

namespace backend.IRepository;

public interface IRefreshTokenRepository
{
    Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken);

    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
