using System.ComponentModel.DataAnnotations;

namespace backend.Dto.BranchDtos;

public class BranchDto
{
    public Guid Id { get; set; }

    [Required]
    public string? Name { get; set; }

    [Required]
    public string? Location { get; set; }
}

