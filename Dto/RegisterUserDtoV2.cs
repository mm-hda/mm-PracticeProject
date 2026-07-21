using System.ComponentModel.DataAnnotations;

namespace backend.Dto;

public class RegisterUserDtoV2
{
    public Guid Id { get; set; }

    [Required]
    public string? FirstName { get; set; }

    [Required]
    public string? LastName { get; set; }

    [Required]
    public string? Email { get; set; }

    [Required]
    public string? Password { get; set; }
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
