using backend.Data;
using backend.Dto.RefreshTokenDtos;
using backend.Entities;
using backend.GenericRepositories;
using backend.IRepository;

namespace backend.Repositories;

internal sealed class RefreshTokenRepository(AppDbContext context, IGenericRepository<User> userRepository) : GenericRepository<RefreshToken>(context), IRefreshTokenRepository
{
    public async Task AddRefreshTokenAsync(CreateRefreshTokenDto refreshTokenDto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(refreshTokenDto);

        var now = DateTime.UtcNow;

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = refreshTokenDto.UserId,
            TokenHash = refreshTokenDto.TokenHash,
            ExpiresAtUtc = refreshTokenDto.ExpiresAtUtc,
            CreatedAtUtc = refreshTokenDto.CreatedAtUtc,
            IsActive = true,
            LastUsedAtUtc = refreshTokenDto.LastUsedAtUtc ?? now
        };

        await AddAsync(refreshToken, cancellationToken).ConfigureAwait(false);
    }

    public async Task<RefreshTokenResponseDto?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            return null;
        }

        var now = DateTime.UtcNow;

        var refreshToken = await FirstOrDefaultAsync(x => x.TokenHash == tokenHash && x.IsActive && x.ExpiresAtUtc > now, cancellationToken).ConfigureAwait(false);

        if (refreshToken is null)
        {
            return null;
        }

        var user = await userRepository.FirstOrDefaultAsync(x => x.Id == refreshToken.UserId, cancellationToken).ConfigureAwait(false);

        if (user is null)
        {
            return null;
        }

        return new RefreshTokenResponseDto
        {
            Id = refreshToken.Id,
            UserId = refreshToken.UserId,
            TokenHash = refreshToken.TokenHash,
            ExpiresAtUtc = refreshToken.ExpiresAtUtc,
            CreatedAtUtc = refreshToken.CreatedAtUtc,
            IsActive = refreshToken.IsActive && refreshToken.ExpiresAtUtc > now,
            LastUsedAtUtc = refreshToken.LastUsedAtUtc,
            UserName = user.Name,
            UserEmail = user.Email
        };
    }

    public async Task<IReadOnlyCollection<RefreshTokenResponseDto>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            return Array.Empty<RefreshTokenResponseDto>();
        }

        var now = DateTime.UtcNow;

        var refreshTokens = await FindAsync(x => x.UserId == userId && x.IsActive && x.ExpiresAtUtc > now, cancellationToken).ConfigureAwait(false);

        if (refreshTokens.Count == 0)
        {
            return Array.Empty<RefreshTokenResponseDto>();
        }

        var user = await userRepository.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken).ConfigureAwait(false);

        return refreshTokens
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new RefreshTokenResponseDto
            {
                Id = x.Id,
                UserId = x.UserId,
                TokenHash = x.TokenHash,
                ExpiresAtUtc = x.ExpiresAtUtc,
                CreatedAtUtc = x.CreatedAtUtc,
                IsActive = x.IsActive && x.ExpiresAtUtc > now,
                LastUsedAtUtc = x.LastUsedAtUtc,
                UserName = user?.Name ?? string.Empty,
                UserEmail = user?.Email ?? string.Empty
            })
            .ToList();
    }

    public async Task UpdateLastUsedAtUtcAsync(Guid refreshTokenId, CancellationToken cancellationToken)
    {
        if (refreshTokenId == Guid.Empty)
        {
            return;
        }

        var refreshToken = await FirstOrDefaultAsync(x => x.Id == refreshTokenId && x.IsActive, cancellationToken).ConfigureAwait(false);

        if (refreshToken is null)
        {
            return;
        }

        refreshToken.LastUsedAtUtc = DateTime.UtcNow;

        Update(refreshToken, cancellationToken);
    }

    public async Task DeactivateRefreshTokenAsync(Guid refreshTokenId, CancellationToken cancellationToken)
    {
        if (refreshTokenId == Guid.Empty)
        {
            return;
        }

        var refreshToken = await FirstOrDefaultAsync(x => x.Id == refreshTokenId, cancellationToken).ConfigureAwait(false);

        if (refreshToken is null || !refreshToken.IsActive)
        {
            return;
        }

        refreshToken.IsActive = false;

        Update(refreshToken, cancellationToken);
    }

    public async Task DeactivateUserRefreshTokensAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            return;
        }

        var refreshTokens = await FindAsync(x => x.UserId == userId && x.IsActive, cancellationToken).ConfigureAwait(false);

        if (refreshTokens.Count == 0)
        {
            return;
        }

        foreach (var refreshToken in refreshTokens)
        {
            refreshToken.IsActive = false;
            Update(refreshToken, cancellationToken);
        }
    }
}
