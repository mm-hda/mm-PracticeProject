namespace backend.Dto.PositionDto
{
    public class PositionResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int TotalUsers { get; set; }
    }
}