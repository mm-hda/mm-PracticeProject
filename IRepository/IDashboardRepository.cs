using backend.Dto.DashboardDtos;

namespace backend.IRepository;

public interface IDashboardRepository
{
    Task<int> GetTotalUsersAsync();

    Task<int> GetTotalBranchesAsync();

    Task<int> GetTotalDepartmentsAsync();

    Task<int> GetTotalProjectsAsync();

    Task<int> GetTotalRunningProjectsAsync();

    Task<IReadOnlyCollection<RoleUserCountDto>> GetRoleWiseUserCountsAsync();

    Task<IReadOnlyCollection<BranchDashboardDto>> GetBranchesAsync();

    Task<IReadOnlyCollection<DepartmentDashboardDto>> GetDepartmentsAsync();

    Task<IReadOnlyCollection<ProjectDashboardDto>> GetRunningProjectsAsync();
}
