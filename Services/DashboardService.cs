using System.Linq;

using backend.Dto.DashboardDtos;
using backend.GenericResponse;
using backend.IRepository;
using backend.IService;

namespace backend.Services;

internal sealed class DashboardService(IDashboardRepository dashboardRepository) : IDashboardService
{
    public async Task<ServiceResponse<DashboardResponseDto>> GetDashboard()
    {
        try
        {
            DashboardResponseDto dashboard = new()
            {
                TotalUsers = await dashboardRepository.GetTotalUsersAsync().ConfigureAwait(false),
                TotalBranches = await dashboardRepository.GetTotalBranchesAsync().ConfigureAwait(false),
                TotalDepartments = await dashboardRepository.GetTotalDepartmentsAsync().ConfigureAwait(false),
                TotalProjects = await dashboardRepository.GetTotalProjectsAsync().ConfigureAwait(false),
                TotalRunningProjects = await dashboardRepository.GetTotalRunningProjectsAsync().ConfigureAwait(false),
                RoleWiseUserCounts = [.. await dashboardRepository.GetRoleWiseUserCountsAsync().ConfigureAwait(false)],
                Branches = [.. await dashboardRepository.GetBranchesAsync().ConfigureAwait(false)],
                Departments = [.. await dashboardRepository.GetDepartmentsAsync().ConfigureAwait(false)],
                RunningProjects = [.. await dashboardRepository.GetRunningProjectsAsync().ConfigureAwait(false)]
            };

            return new ServiceResponse<DashboardResponseDto>
            {
                StatusCode = CustomCodes.DataRetrieved,
                IsSuccess = true,
                Data = dashboard
            };
        }
        catch (Exception)
        {
            return new ServiceResponse<DashboardResponseDto>
            {
                StatusCode = CustomCodes.InternalServerError,
                IsSuccess = false,
                Data = new DashboardResponseDto()
            };
            throw;
        }
    }
}
