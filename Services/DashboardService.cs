using backend.Data;
using backend.Dto.DashboardDto;
using backend.IService;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class DashboardService(AppDbContext _context) : IDashboardService
    {
        public async Task<Tuple<int, DashboardResponseDto, string>> GetDashboard()
        {
            try
            {
                DashboardResponseDto dashboard = new();

                dashboard.TotalUsers = await _context.Users.AsNoTracking()
                .Include(x => x.Role)
                .CountAsync(x => x.Role != null && x.Role.Name != "Admin");

                dashboard.TotalBranches = await _context.Branches.AsNoTracking().CountAsync();

                dashboard.TotalDepartments = await _context.Departments.AsNoTracking().CountAsync();

                dashboard.TotalProjects = await _context.Projects.AsNoTracking().CountAsync();

                dashboard.TotalRunningProjects = await _context.Projects.AsNoTracking()
                .CountAsync(x => x.EndDate == null || x.EndDate >= DateTime.UtcNow);

                dashboard.RoleWiseUserCounts = await _context.Roles.AsNoTracking()
                    .Where(r => r.Name != "Admin")
                    .Select(r => new RoleUserCountDto
                    {
                        RoleId = r.Id,
                        RoleName = r.Name,
                        UserCount = _context.Users.Count(u => u.RoleId == r.Id)
                    }).ToListAsync();

                dashboard.Branches = await _context.Branches.AsNoTracking()
                    .Select(b => new BranchDashboardDto
                    {
                        BranchId = b.Id,
                        BranchName = b.Name,
                        Location = b.Location,

                        UserCount = _context.Users.Include(u => u.Role)
                        .Count(u => u.BranchId == b.Id && u.Role != null && u.Role.Name != "Admin")
                    }).ToListAsync();

                dashboard.Departments = await _context.Departments.AsNoTracking()
                    .Where(d => d.Name != "Admin")
                    .Select(d => new DepartmentDashboardDto
                    {
                        DepartmentId = d.Id,

                        DepartmentName = d.Name,

                        UserCount = _context.Users
                            .Include(u => u.Role)
                            .Count(u => u.DepartmentId == d.Id && u.Role != null && u.Role.Name != "Admin"),

                        TotalPositions = _context.Positions
                            .Count(p => p.DepartmentId == d.Id),

                        Positions = _context.Positions
                            .Where(p => p.DepartmentId == d.Id)
                            .Select(p => new PositionDashboardDto
                            {
                                PositionId = p.Id,
                                PositionName = p.Name,
                                UserCount = _context.Users.Include(u => u.Role)
                                .Count(u => u.PositionId == p.Id && u.Role != null && u.Role.Name != "Admin")
                            }).ToList()
                    }).ToListAsync();

                dashboard.RunningProjects = await _context.Projects.AsNoTracking()
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

                        UserCount = _context.EmployeeProjects
                            .Include(ep => ep.User)
                            .ThenInclude(u => u!.Role)
                            .Count(ep =>
                                ep.ProjectId == p.Id &&
                                ep.User != null &&
                                ep.User.Role != null &&
                                ep.User.Role.Name != "Admin")
                    }).ToListAsync();

                return new Tuple<int, DashboardResponseDto, string>(1, dashboard, "Dashboard retrieved successfully");
            }
            catch (Exception ex)
            {
                return new Tuple<int, DashboardResponseDto, string>(0, new DashboardResponseDto(), ex.Message);
            }
        }
    }
}