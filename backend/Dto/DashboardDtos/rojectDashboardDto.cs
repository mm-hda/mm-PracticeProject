namespace backend.Dto.DashboardDtos;

public class ProjectDashboardDto
{
    public Guid ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid ProjectManagerId { get; set; }
    public string? ProjectManagerName { get; set; }
    public int UserCount { get; set; }
}
