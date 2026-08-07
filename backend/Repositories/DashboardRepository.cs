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
        var departments = await departmentRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var positions = await positionRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var users = await userRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var adminRole = await roleRepository.FirstOrDefaultAsync(x => x.Name == "Admin", cancellationToken).ConfigureAwait(false);

        var filteredUsers = adminRole is null ? users : users.Where(x => x.RoleId != adminRole.Id).ToList();

        var departmentUserCounts = filteredUsers.GroupBy(x => x.DepartmentId).ToDictionary(x => x.Key, x => x.Count());

        var positionUserCounts = filteredUsers.GroupBy(x => x.PositionId).ToDictionary(x => x.Key, x => x.Count());

        var positionsByDepartment = positions.GroupBy(x => x.DepartmentId).ToDictionary(x => x.Key, x => x.ToList());

        var result = new List<DepartmentDashboardDto>();

        foreach (var department in departments.Where(x => x.Name != "Admin"))
        {
            positionsByDepartment.TryGetValue(department.Id, out var departmentPositions);

            departmentPositions ??= [];

            departmentUserCounts.TryGetValue(department.Id, out var userCount);

            var positionDtos = departmentPositions.Select(position =>
                {
                    positionUserCounts.TryGetValue(position.Id, out var positionUserCount);

                    return new PositionDashboardDto
                    {
                        PositionId = position.Id,
                        PositionName = position.Name,
                        UserCount = positionUserCount
                    };
                }).ToList();

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

    public async Task<IReadOnlyCollection<ProjectDashboardDto>> GetRunningProjectsAsync(CancellationToken cancellationToken)
    {
        var projects = await projectRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var users = await userRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var employeeProjects = await employeeProjectRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var userDictionary = users.ToDictionary(x => x.Id);

        var projectUserCounts = employeeProjects.GroupBy(x => x.ProjectId)
            .ToDictionary(x => x.Key, x => x.Count());

        var result = new List<ProjectDashboardDto>();

        foreach (var project in projects.Where(x => x.EndDate == null || x.EndDate >= DateTime.UtcNow))
        {
            userDictionary.TryGetValue(project.ProjectManagerId, out var manager);

            projectUserCounts.TryGetValue(project.Id, out var userCount);

            result.Add(new ProjectDashboardDto
            {
                ProjectId = project.Id,
                ProjectName = project.Name,
                Description = project.Description,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                ProjectManagerId = project.ProjectManagerId,
                ProjectManagerName = manager?.Name ?? string.Empty,
                UserCount = userCount
            });
        }

        return result;
    }
}
