using backend.Data;
using backend.Dto.DashboardDtos;
using backend.IRepository;

using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

internal sealed class DashboardRepository(AppDbContext context)
    : IDashboardRepository
{
    public async Task<int> GetTotalUsersAsync()
    {
        return await context.Users
            .AsNoTracking()
            .Include(x => x.Role)
            .CountAsync(x => x.Role != null && x.Role.Name != "Admin").ConfigureAwait(false);
    }

    public async Task<int> GetTotalBranchesAsync()
    {
        return await context.Branches
            .AsNoTracking()
            .CountAsync().ConfigureAwait(false);
    }

    public async Task<int> GetTotalDepartmentsAsync()
    {
        return await context.Departments
            .AsNoTracking()
            .CountAsync().ConfigureAwait(false);
    }

    public async Task<int> GetTotalProjectsAsync()
    {
        return await context.Projects
            .AsNoTracking()
            .CountAsync().ConfigureAwait(false);
    }

    public async Task<int> GetTotalRunningProjectsAsync()
    {
        return await context.Projects
            .AsNoTracking()
            .CountAsync(x => x.EndDate == null || x.EndDate >= DateTime.UtcNow).ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<RoleUserCountDto>> GetRoleWiseUserCountsAsync()
    {
        return await context.Roles
            .AsNoTracking()
            .Where(x => x.Name != "Admin")
            .Select(x => new RoleUserCountDto
            {
                RoleId = x.Id,
                RoleName = x.Name,
                UserCount = context.Users.Count(u => u.RoleId == x.Id)
            })
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<BranchDashboardDto>> GetBranchesAsync()
    {
        return await context.Branches
            .AsNoTracking()
            .Select(x => new BranchDashboardDto
            {
                BranchId = x.Id,
                BranchName = x.Name,
                Location = x.Location,
                UserCount = context.Users
                    .Include(u => u.Role)
                    .Count(u =>
                        u.BranchId == x.Id &&
                        u.Role != null &&
                        u.Role.Name != "Admin")
            })
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<DepartmentDashboardDto>> GetDepartmentsAsync()
    {
        return await context.Departments
            .AsNoTracking()
            .Where(x => x.Name != "Admin")
            .Select(x => new DepartmentDashboardDto
            {
                DepartmentId = x.Id,
                DepartmentName = x.Name,
                UserCount = context.Users
                    .Include(u => u.Role)
                    .Count(u =>
                        u.DepartmentId == x.Id &&
                        u.Role != null &&
                        u.Role.Name != "Admin"),

                TotalPositions = context.Positions
                    .Count(p => p.DepartmentId == x.Id),

                Positions = context.Positions
                    .Where(p => p.DepartmentId == x.Id)
                    .Select(p => new PositionDashboardDto
                    {
                        PositionId = p.Id,
                        PositionName = p.Name,
                        UserCount = context.Users
                            .Include(u => u.Role)
                            .Count(u =>
                                u.PositionId == p.Id &&
                                u.Role != null &&
                                u.Role.Name != "Admin")
                    })
                    .ToList()
            })
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<ProjectDashboardDto>> GetRunningProjectsAsync()
    {
        return await context.Projects
            .AsNoTracking()
            .Include(x => x.ProjectManager)
            .Where(x => x.EndDate == null || x.EndDate >= DateTime.UtcNow)
            .Select(x => new ProjectDashboardDto
            {
                ProjectId = x.Id,
                ProjectName = x.Name,
                Description = x.Description,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                ProjectManagerId = x.ProjectManagerId,
                ProjectManagerName = x.ProjectManager != null
                    ? x.ProjectManager.Name
                    : string.Empty,

                UserCount = context.EmployeeProjects
                    .Include(ep => ep.User)
                    .ThenInclude(u => u!.Role)
                    .Count(ep =>
                        ep.ProjectId == x.Id &&
                        ep.User != null &&
                        ep.User.Role != null &&
                        ep.User.Role.Name != "Admin")
            })
            .ToListAsync().ConfigureAwait(false);
    }
}
