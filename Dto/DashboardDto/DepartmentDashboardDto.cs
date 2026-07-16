namespace backend.Dto.DashboardDto
{
    public class DepartmentDashboardDto
    {
        public Guid DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int UserCount { get; set; }
        public int TotalPositions { get; set; }
        public List<PositionDashboardDto> Positions { get; set; } = new();
    }
}