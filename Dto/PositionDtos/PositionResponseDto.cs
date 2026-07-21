namespace backend.Dto.PositionDtos;

public class PositionResponseDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public Guid DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int TotalUsers { get; set; }
}
