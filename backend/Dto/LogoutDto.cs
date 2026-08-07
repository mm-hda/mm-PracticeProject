using System.ComponentModel.DataAnnotations;

namespace backend.Dto;

public class LogoutDto
{
    [Required]
    public string? Email { get; set; }
}
