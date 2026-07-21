namespace backend.Dto.DashboardDtos;

public class BranchDashboardDto
{
    public Guid BranchId { get; set; }
    public string? BranchName { get; set; }
    public string? Location { get; set; }
    public int UserCount { get; set; }
}
