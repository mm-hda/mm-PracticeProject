using backend.Data;
using backend.Dto.DashboardDtos;
using backend.Entities;
using backend.GenericRepositories;
using backend.IRepository;

namespace backend.Repositories;

internal sealed class DashboardRepository(
    IGenericRepository<User> userRepository,
    IGenericRepository<Role> roleRepository,
    IGenericRepository<Branch> branchRepository,
    IGenericRepository<Department> departmentRepository,
    IGenericRepository<Position> positionRepository,
    IGenericRepository<Project> projectRepository,
    IGenericRepository<EmployeeProject> employeeProjectRepository)
    : IDashboardRepository
{
    public async Task<int> GetTotalUsersAsync(CancellationToken cancellationToken)
    {
        var roles = await roleRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var users = await userRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var adminRole = roles.FirstOrDefault(x => x.Name == "Admin");

        if (adminRole is null)
        {
            return users.Count;
        }

        return users.Count(x => x.RoleId != adminRole.Id);
    }

    public async Task<int> GetTotalBranchesAsync(CancellationToken cancellationToken)
        => (await branchRepository.GetAllAsync(cancellationToken).ConfigureAwait(false)).Count;

    public async Task<int> GetTotalDepartmentsAsync(CancellationToken cancellationToken)
        => (await departmentRepository.GetAllAsync(cancellationToken).ConfigureAwait(false)).Count;

    public async Task<int> GetTotalProjectsAsync(CancellationToken cancellationToken)
        => (await projectRepository.GetAllAsync(cancellationToken).ConfigureAwait(false)).Count;

    public async Task<int> GetTotalRunningProjectsAsync(CancellationToken cancellationToken)
    {
        var projects = await projectRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        return projects.Count(x => x.EndDate == null || x.EndDate >= DateTime.UtcNow);
    }

    public async Task<IReadOnlyCollection<RoleUserCountDto>> GetRoleWiseUserCountsAsync(CancellationToken cancellationToken)
    {
        var roles = await roleRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var users = await userRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        return roles
            .Where(x => x.Name != "Admin")
            .Select(role => new RoleUserCountDto
            {
                RoleId = role.Id,
                RoleName = role.Name,
                UserCount = users.Count(user => user.RoleId == role.Id)
            })
            .ToList();
    }

    public async Task<IReadOnlyCollection<BranchDashboardDto>> GetBranchesAsync(CancellationToken cancellationToken)
    {
        var branches = await branchRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var users = await userRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var roles = await roleRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var adminRole = roles.FirstOrDefault(x => x.Name == "Admin");

        return branches.Select(branch => new BranchDashboardDto
        {
            BranchId = branch.Id,
            BranchName = branch.Name,
            Location = branch.Location,
            UserCount = users.Count(user =>
                user.BranchId == branch.Id &&
                (adminRole == null || user.RoleId != adminRole.Id))
        }).ToList();
    }

    public async Task<IReadOnlyCollection<DepartmentDashboardDto>> GetDepartmentsAsync(CancellationToken cancellationToken)
    {
        var departments = await departmentRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var positions = await positionRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var users = await userRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var roles = await roleRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var adminRole = roles.FirstOrDefault(x => x.Name == "Admin");

        return departments
            .Where(x => x.Name != "Admin")
            .Select(department => new DepartmentDashboardDto
            {
                DepartmentId = department.Id,
                DepartmentName = department.Name,

                UserCount = users.Count(user =>
                    user.DepartmentId == department.Id &&
                    (adminRole == null || user.RoleId != adminRole.Id)),

                Positions = [
                    .. positions
                        .Where(position => position.DepartmentId == department.Id)
                        .Select(position => new PositionDashboardDto
                        {
                            PositionId = position.Id,
                            PositionName = position.Name,

                            UserCount = users.Count(user =>
                                user.PositionId == position.Id &&
                                (adminRole == null || user.RoleId != adminRole.Id))
                        })
                ],

                TotalPositions = positions.Count(position =>
                    position.DepartmentId == department.Id)

            }).ToList();
    }

    public async Task<IReadOnlyCollection<ProjectDashboardDto>> GetRunningProjectsAsync(CancellationToken cancellationToken)
    {
        var projects = await projectRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var users = await userRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var roles = await roleRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var employeeProjects = await employeeProjectRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var adminRole = roles.FirstOrDefault(x => x.Name == "Admin");
        var userDictionary = users.ToDictionary(x => x.Id);

        return projects
            .Where(project => project.EndDate == null || project.EndDate >= DateTime.UtcNow)
            .Select(project =>
            {
                userDictionary.TryGetValue(project.ProjectManagerId, out var manager);

                return new ProjectDashboardDto
                {
                    ProjectId = project.Id,
                    ProjectName = project.Name,
                    Description = project.Description,
                    StartDate = project.StartDate,
                    EndDate = project.EndDate,
                    ProjectManagerId = project.ProjectManagerId,
                    ProjectManagerName = manager?.Name ?? string.Empty,

                    UserCount = employeeProjects.Count(ep =>
                    {
                        if (ep.ProjectId != project.Id)
                        {
                            return false;
                        }

                        if (!userDictionary.TryGetValue(ep.UserId, out var user))
                        {
                            return false;
                        }

                        return adminRole == null || user.RoleId != adminRole.Id;
                    })
                };
            })
            .ToList();
    }
}
