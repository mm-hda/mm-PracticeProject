using System.ComponentModel.DataAnnotations;

namespace backend.Dto;

public class RegisterUserDto
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
    [Required]
    public DateTime? DOB { get; set; }

    [Required]
    public Guid BranchId { get; set; }

    [Required]
    public Guid DepartmentId { get; set; }

    [Required]
    public Guid PositionId { get; set; }

    [Required]
    public Guid RoleId { get; set; }
}
