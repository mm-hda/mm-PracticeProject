using System.ComponentModel.DataAnnotations;

namespace backend.Dto.ProjectDtos;

public class ProjectDto
{
    public Guid Id { get; set; }

    [Required]
    public string? Name { get; set; }

    public string? Description { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [Required]
    public Guid ProjectManagerId { get; set; }
}
