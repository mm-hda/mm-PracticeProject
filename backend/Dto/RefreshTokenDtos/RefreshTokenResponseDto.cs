namespace backend.Dto.RefreshTokenDtos;

public sealed class RefreshTokenResponseDto
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public bool IsActive { get; set; }

    public DateTime? LastUsedAtUtc { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string UserEmail { get; set; } = string.Empty;
}
