using System.Diagnostics.CodeAnalysis;

namespace backend.Dto.DashboardDtos;

[SuppressMessage("Design", "CA1002:Do not expose generic lists")]
[SuppressMessage("Usage", "CA2227:Collection properties should be read only")]
public class DashboardResponseDto
{
    public int TotalUsers { get; set; }
    public int TotalBranches { get; set; }
    public int TotalDepartments { get; set; }
    public int TotalProjects { get; set; }
    public int TotalRunningProjects { get; set; }

    public List<RoleUserCountDto> RoleWiseUserCounts { get; set; } = [];
    public List<BranchDashboardDto> Branches { get; set; } = [];
    public List<DepartmentDashboardDto> Departments { get; set; } = [];
    public List<ProjectDashboardDto> RunningProjects { get; set; } = [];
}
