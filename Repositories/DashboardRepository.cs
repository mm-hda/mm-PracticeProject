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
        var role = await roleRepository.FirstOrDefaultAsync(x => x.Name == "Admin", cancellationToken).ConfigureAwait(false);
        if (role is null)
        {
            return 0;
        }

        var users = await userRepository.FindAsync(x => x.RoleId != role.Id, cancellationToken).ConfigureAwait(false);

        return users.Count;
    }

    public async Task<int> GetTotalBranchesAsync(CancellationToken cancellationToken) => await branchRepository.CountAsync(x => true, cancellationToken).ConfigureAwait(false);

    public async Task<int> GetTotalDepartmentsAsync(CancellationToken cancellationToken)
        => await departmentRepository.CountAsync(x => true, cancellationToken).ConfigureAwait(false);

    public async Task<int> GetTotalProjectsAsync(CancellationToken cancellationToken)
        => await projectRepository.CountAsync(x => true, cancellationToken).ConfigureAwait(false);

    public async Task<int> GetTotalRunningProjectsAsync(CancellationToken cancellationToken)
    {
        var projects = await projectRepository.CountAsync(x => x.EndDate == null || x.EndDate >= DateTime.UtcNow, cancellationToken).ConfigureAwait(false);
        return projects;
    }

    public async Task<IReadOnlyCollection<RoleUserCountDto>> GetRoleWiseUserCountsAsync(CancellationToken cancellationToken)
    {
        var roles = await roleRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var result = new List<RoleUserCountDto>();

        foreach (var role in roles.Where(x => x.Name != "Admin"))
        {
            var count = await userRepository.CountAsync(
                x => x.RoleId == role.Id,
                cancellationToken).ConfigureAwait(false);

            result.Add(new RoleUserCountDto
            {
                RoleId = role.Id,
                RoleName = role.Name,
                UserCount = count
            });
        }

        return result;
    }

    public async Task<IReadOnlyCollection<BranchDashboardDto>> GetBranchesAsync(CancellationToken cancellationToken)
    {
        var branches = await branchRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var result = new List<BranchDashboardDto>();

        foreach (var branch in branches)
        {
            var userCount = await userRepository.CountAsync(x => x.BranchId == branch.Id, cancellationToken).ConfigureAwait(false);

            result.Add(new BranchDashboardDto
            {
                BranchId = branch.Id,
                BranchName = branch.Name,
                Location = branch.Location,
                UserCount = userCount
            });
        }

        return result;
    }
    public async Task<IReadOnlyCollection<DepartmentDashboardDto>> GetDepartmentsAsync(CancellationToken cancellationToken)
    {
        var departmentsTask = departmentRepository.GetAllAsync(cancellationToken);
        var positionsTask = positionRepository.GetAllAsync(cancellationToken);
        var adminRoleTask = roleRepository.FirstOrDefaultAsync(x => x.Name == "Admin", cancellationToken);

        await Task.WhenAll(departmentsTask, positionsTask, adminRoleTask).ConfigureAwait(false);

        var departments = await departmentsTask.ConfigureAwait(false);
        var positions = await positionsTask.ConfigureAwait(false);
        var adminRole = await adminRoleTask.ConfigureAwait(false);

        var result = new List<DepartmentDashboardDto>();

        foreach (var department in departments.Where(x => x.Name != "Admin"))
        {
            var userCount = await userRepository.CountAsync(x => x.DepartmentId == department.Id && (adminRole == null || x.RoleId != adminRole.Id), cancellationToken).ConfigureAwait(false);

            var departmentPositions = positions
                .Where(x => x.DepartmentId == department.Id)
                .ToList();

            var positionDtos = new List<PositionDashboardDto>();

            foreach (var position in departmentPositions)
            {
                var positionUserCount = await userRepository.CountAsync(x => x.PositionId == position.Id && (adminRole == null || x.RoleId != adminRole.Id), cancellationToken).ConfigureAwait(false);

                positionDtos.Add(new PositionDashboardDto
                {
                    PositionId = position.Id,
                    PositionName = position.Name,
                    UserCount = positionUserCount
                });
            }

            result.Add(new DepartmentDashboardDto
            {
                DepartmentId = department.Id,
                DepartmentName = department.Name,
                UserCount = userCount,
                Positions = positionDtos,
                TotalPositions = departmentPositions.Count
            });
        }

        return result;
    }

    public async Task<IReadOnlyCollection<ProjectDashboardDto>> GetRunningProjectsAsync(
    CancellationToken cancellationToken)
    {
        var projectsTask = projectRepository.GetAllAsync(cancellationToken);
        var usersTask = userRepository.GetAllAsync(cancellationToken);
        var adminRoleTask = roleRepository.FirstOrDefaultAsync(x => x.Name == "Admin", cancellationToken);

        await Task.WhenAll(
            projectsTask,
            usersTask,
            adminRoleTask).ConfigureAwait(false);

        var projects = await projectsTask.ConfigureAwait(false);
        var users = await usersTask.ConfigureAwait(false);
        var adminRole = await adminRoleTask.ConfigureAwait(false);

        var userDictionary = users.ToDictionary(x => x.Id);

        var result = new List<ProjectDashboardDto>();

        foreach (var project in projects.Where(x => x.EndDate == null || x.EndDate >= DateTime.UtcNow))
        {
            userDictionary.TryGetValue(project.ProjectManagerId, out var manager);

            var totalUsers = await employeeProjectRepository.CountAsync(x => x.ProjectId == project.Id, cancellationToken).ConfigureAwait(false);

            result.Add(new ProjectDashboardDto
            {
                ProjectId = project.Id,
                ProjectName = project.Name,
                Description = project.Description,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                ProjectManagerId = project.ProjectManagerId,
                ProjectManagerName = manager?.Name ?? string.Empty,
                UserCount = totalUsers
            });
        }

        return result;
    }
}
