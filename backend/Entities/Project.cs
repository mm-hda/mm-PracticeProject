using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Entities;

public class Project
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [Required]
    public Guid ProjectManagerId { get; set; }

    [ForeignKey(nameof(ProjectManagerId))]
    public User? ProjectManager { get; set; }

    public ICollection<EmployeeProject> EmployeeProjects { get; } = [];
}
