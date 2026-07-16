namespace backend.Dto.DashboardDto
{
    public class DashboardResponseDto
    {
        public int TotalUsers { get; set; }
        public int TotalBranches { get; set; }
        public int TotalDepartments { get; set; }
        public int TotalProjects { get; set; }
        public int TotalRunningProjects { get; set; }
        public List<RoleUserCountDto> RoleWiseUserCounts { get; set; } = new();
        public List<BranchDashboardDto> Branches { get; set; } = new();
        public List<DepartmentDashboardDto> Departments { get; set; } = new();
        public List<ProjectDashboardDto> RunningProjects { get; set; } = new();
    }
}