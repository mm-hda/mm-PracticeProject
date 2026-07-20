using System.ComponentModel.DataAnnotations;

namespace backend.Dto.BranchDtos;

public class BranchDto
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Location { get; set; } = string.Empty;
}

