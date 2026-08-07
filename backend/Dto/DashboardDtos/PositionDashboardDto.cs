namespace backend.Dto.DashboardDtos;

public class PositionDashboardDto
{
    public Guid PositionId { get; set; }
    public string? PositionName { get; set; }
    public int UserCount { get; set; }
}
