using System.ComponentModel.DataAnnotations;

namespace backend.Dto.Common
{
    public class PaginationDto
    {
        [Required]
        public int PageNumber { get; set; } = 1;
        [Required]
        public int PageSize { get; set; } = 2;
    }
}