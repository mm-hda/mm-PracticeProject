namespace backend.Dto.DashboardDto
{
    public class PositionDashboardDto
    {
        public Guid PositionId { get; set; }
        public string PositionName { get; set; } = string.Empty;
        public int UserCount { get; set; }
    }
}