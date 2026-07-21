namespace backend.Dto.DepartmentDtos;

public class DepartmentResponseDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public int TotalPositions { get; set; }
    public int TotalUsers { get; set; }
}
