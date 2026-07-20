namespace backend.Dto.DashboardDtos;

public class BranchDashboardDto
{
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int UserCount { get; set; }
}
