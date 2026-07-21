using System.ComponentModel.DataAnnotations;

namespace backend.Dto.PositionDtos;

public class PositionDto
{
    public Guid Id { get; set; }

    [Required]
    public string? Name { get; set; }

    [Required]
    public Guid DepartmentId { get; set; }
}
