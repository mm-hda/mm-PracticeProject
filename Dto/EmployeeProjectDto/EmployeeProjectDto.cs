using System.ComponentModel.DataAnnotations;

namespace backend.Dto.EmployeeProjectDto
{
    public class EmployeeProjectDto
    {
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid ProjectId { get; set; }
    }
}