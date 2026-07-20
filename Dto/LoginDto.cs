using System.ComponentModel.DataAnnotations;

namespace backend.Dto;

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(25)]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}
