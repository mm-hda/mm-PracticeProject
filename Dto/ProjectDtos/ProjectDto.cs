using System.ComponentModel.DataAnnotations;

namespace backend.Dto.ProjectDtos;

public class ProjectDto
{
    public Guid Id { get; set; }

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
}
