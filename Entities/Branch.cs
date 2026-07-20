using System.ComponentModel.DataAnnotations;

namespace backend.Entities;

public class Branch
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Location { get; set; } = string.Empty;

    public ICollection<User> Users { get; } = [];
}
