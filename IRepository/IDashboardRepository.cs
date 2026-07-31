using backend.Dto.DashboardDtos;

namespace backend.IRepository;

public interface IDashboardRepository
{
    Task<int> GetTotalUsersAsync(CancellationToken cancellationToken);

    Task<int> GetTotalBranchesAsync(CancellationToken cancellationToken);

    Task<int> GetTotalDepartmentsAsync(CancellationToken cancellationToken);

    Task<int> GetTotalProjectsAsync(CancellationToken cancellationToken);

    Task<int> GetTotalRunningProjectsAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<RoleUserCountDto>> GetRoleWiseUserCountsAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<BranchDashboardDto>> GetBranchesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<DepartmentDashboardDto>> GetDepartmentsAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ProjectDashboardDto>> GetRunningProjectsAsync(CancellationToken cancellationToken);
}
