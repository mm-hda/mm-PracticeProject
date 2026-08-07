using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Entities;

public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public DateTime? DOB { get; set; }

    public Guid BranchId { get; set; } = Guid.Empty;

    public Guid DepartmentId { get; set; } = Guid.Empty;

    public Guid PositionId { get; set; } = Guid.Empty;
    [Required]
    public Guid RoleId { get; set; }

    [ForeignKey(nameof(RoleId))]
    public Role? Role { get; set; }

    [ForeignKey(nameof(BranchId))]
    public Branch? Branch { get; set; }

    [ForeignKey(nameof(DepartmentId))]
    public Department? Department { get; set; }

    [ForeignKey(nameof(PositionId))]
    public Position? Position { get; set; }

    public ICollection<Project> ManagedProjects { get; } = [];

    public ICollection<EmployeeProject> EmployeeProjects { get; } = [];
}
