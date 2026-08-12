using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Entities;

public sealed class RefreshToken
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public Guid UserId { get; set; }
    [Required]
    public string Token { get; set; } = string.Empty;
    [Required]
    public DateTime ExpiresAtUtc { get; set; }
    [Required]
    public DateTime CreatedAtUtc { get; set; }
    [Required]
    public bool IsRevoked { get; set; }

    public DateTime? RevokedAtUtc { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}
