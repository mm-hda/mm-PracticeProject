namespace backend.Dto.DepartmentDto
{
    public class DepartmentResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int TotalPositions { get; set; }
        public int TotalUsers { get; set; }
    }
}