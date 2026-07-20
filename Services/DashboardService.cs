using backend.Data;
using backend.Dto.DashboardDtos;
using backend.IService;
using backend.GenericResponse;

using Microsoft.EntityFrameworkCore;

namespace backend.Services;

internal sealed class DashboardService(AppDbContext context) : IDashboardService
{
    public async Task<Tuple<int, DashboardResponseDto>> GetDashboard()
    {
        try
        {
            DashboardResponseDto dashboard = new();

            dashboard.TotalUsers = await context.Users.AsNoTracking()
            .Include(x => x.Role)
            .CountAsync(x => x.Role != null && x.Role.Name != "Admin").ConfigureAwait(false);

            dashboard.TotalBranches = await context.Branches.AsNoTracking().CountAsync().ConfigureAwait(false);

            dashboard.TotalDepartments = await context.Departments.AsNoTracking().CountAsync().ConfigureAwait(false);

            dashboard.TotalProjects = await context.Projects.AsNoTracking().CountAsync().ConfigureAwait(false);

            dashboard.TotalRunningProjects = await context.Projects.AsNoTracking()
            .CountAsync(x => x.EndDate == null || x.EndDate >= DateTime.UtcNow).ConfigureAwait(false);

            dashboard.RoleWiseUserCounts = await context.Roles.AsNoTracking()
                .Where(r => r.Name != "Admin")
                .Select(r => new RoleUserCountDto
                {
                    RoleId = r.Id,
                    RoleName = r.Name,
                    UserCount = context.Users.Count(u => u.RoleId == r.Id)
                }).ToListAsync().ConfigureAwait(false);

            dashboard.Branches = await context.Branches.AsNoTracking()
                .Select(b => new BranchDashboardDto
                {
                    BranchId = b.Id,
                    BranchName = b.Name,
                    Location = b.Location,

                    UserCount = context.Users.Include(u => u.Role)
                    .Count(u => u.BranchId == b.Id && u.Role != null && u.Role.Name != "Admin")
                }).ToListAsync().ConfigureAwait(false);

            dashboard.Departments = await context.Departments.AsNoTracking()
                .Where(d => d.Name != "Admin")
                .Select(d => new DepartmentDashboardDto
                {
                    DepartmentId = d.Id,

                    DepartmentName = d.Name,

                    UserCount = context.Users
                        .Include(u => u.Role)
                        .Count(u => u.DepartmentId == d.Id && u.Role != null && u.Role.Name != "Admin"),

                    TotalPositions = context.Positions
                        .Count(p => p.DepartmentId == d.Id),

                    Positions = context.Positions
                        .Where(p => p.DepartmentId == d.Id)
                        .Select(p => new PositionDashboardDto
                        {
                            PositionId = p.Id,
                            PositionName = p.Name,
                            UserCount = context.Users.Include(u => u.Role)
                            .Count(u => u.PositionId == p.Id && u.Role != null && u.Role.Name != "Admin")
                        }).ToList()
                }).ToListAsync().ConfigureAwait(false);

            dashboard.RunningProjects = await context.Projects.AsNoTracking()
                .Include(x => x.ProjectManager)
                .Where(x => x.EndDate == null || x.EndDate >= DateTime.UtcNow)
                .Select(p => new ProjectDashboardDto
                {
                    ProjectId = p.Id,

                    ProjectName = p.Name,

                    Description = p.Description,

                    StartDate = p.StartDate,

                    EndDate = p.EndDate,

                    ProjectManagerId = p.ProjectManagerId,

                    ProjectManagerName = p.ProjectManager != null ? p.ProjectManager.Name : "",

                    UserCount = context.EmployeeProjects
                        .Include(ep => ep.User)
                        .ThenInclude(u => u!.Role)
                        .Count(ep =>
                            ep.ProjectId == p.Id &&
                            ep.User != null &&
                            ep.User.Role != null &&
                            ep.User.Role.Name != "Admin")
                }).ToListAsync().ConfigureAwait(false);

            return new Tuple<int, DashboardResponseDto>(CustomCodes.DataRetrieved, dashboard);
        }
        catch (Exception)
        {
            return new Tuple<int, DashboardResponseDto>(CustomCodes.InternalServerError, new DashboardResponseDto());
            throw;
        }
    }
}
