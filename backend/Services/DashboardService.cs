using System.Linq;

using backend.Dto.DashboardDtos;
using backend.GenericResponse;
using backend.IRepository;
using backend.IService;

namespace backend.Services;

internal sealed class DashboardService(IDashboardRepository dashboardRepository) : IDashboardService
{
    public async Task<ServiceResponse<DashboardResponseDto>> GetDashboard(CancellationToken cancellationToken)
    {
        DashboardResponseDto dashboard = new()
        {
            TotalUsers = await dashboardRepository.GetTotalUsersAsync(cancellationToken).ConfigureAwait(false),
            TotalBranches = await dashboardRepository.GetTotalBranchesAsync(cancellationToken).ConfigureAwait(false),
            TotalDepartments = await dashboardRepository.GetTotalDepartmentsAsync(cancellationToken).ConfigureAwait(false),
            TotalProjects = await dashboardRepository.GetTotalProjectsAsync(cancellationToken).ConfigureAwait(false),
            TotalRunningProjects = await dashboardRepository.GetTotalRunningProjectsAsync(cancellationToken).ConfigureAwait(false),
            RoleWiseUserCounts = [.. await dashboardRepository.GetRoleWiseUserCountsAsync(cancellationToken).ConfigureAwait(false)],
            Branches = [.. await dashboardRepository.GetBranchesAsync(cancellationToken).ConfigureAwait(false)],
            Departments = [.. await dashboardRepository.GetDepartmentsAsync(cancellationToken).ConfigureAwait(false)],
            RunningProjects = [.. await dashboardRepository.GetRunningProjectsAsync(cancellationToken).ConfigureAwait(false)]
        };

        return new ServiceResponse<DashboardResponseDto>
        {
            StatusCode = CustomCodes.DataRetrieved,
            IsSuccess = true,
            Data = dashboard
        };
    }
}
