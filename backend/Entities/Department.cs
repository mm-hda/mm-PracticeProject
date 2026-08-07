using System.ComponentModel.DataAnnotations;

namespace backend.Entities;

public class Department
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Position> Positions { get; } = [];

    public ICollection<User> Users { get; } = [];
}
