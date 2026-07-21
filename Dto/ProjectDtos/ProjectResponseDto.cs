namespace backend.Dto.ProjectDtos;

public class ProjectResponseDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid ProjectManagerId { get; set; }
    public string? ProjectManagerName { get; set; }
    public int TotalUsers { get; set; }
}
