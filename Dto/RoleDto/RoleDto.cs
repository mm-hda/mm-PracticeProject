using System.ComponentModel.DataAnnotations;

namespace backend.Dto.RoleDto
{
    public class RoleDto
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
    }
}