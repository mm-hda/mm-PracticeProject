using System.ComponentModel.DataAnnotations;

namespace backend.Dto.DepartmentDtos;

public class DepartmentDto
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string? Name { get; set; }
}
