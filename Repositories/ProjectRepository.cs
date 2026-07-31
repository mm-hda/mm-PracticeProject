using backend.Data;
using backend.Dto.ProjectDtos;
using backend.Entities;
using backend.GenericRepositories;
using backend.IRepository;
namespace backend.Repositories;

internal sealed class ProjectRepository(
    AppDbContext context,
    IGenericRepository<User> userRepository,
    IGenericRepository<Role> roleRepository,
    IGenericRepository<EmployeeProject> employeeProjectRepository,
    IGenericRepository<Branch> branchRepository,
    IGenericRepository<Department> departmentRepository,
    IGenericRepository<Position> positionRepository) : GenericRepository<Project>(context), IProjectRepository
{
    public async Task<bool> ProjectExistsAsync(string? name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }
        var projects = await GetAllAsync(cancellationToken).ConfigureAwait(false);

        return projects.Any(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<bool> ManagerExistsAsync(Guid managerId, CancellationToken cancellationToken)
    {
        var roles = await roleRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var managerRole = roles.FirstOrDefault(x => string.Equals(x.Name, "Manager", StringComparison.OrdinalIgnoreCase));

        if (managerRole is null)
        {
            return false;
        }
        return await userRepository.AnyAsync(x => x.Id == managerId && x.RoleId == managerRole.Id, cancellationToken).ConfigureAwait(false);
    }

    public async Task AddProjectAsync(Project project, CancellationToken cancellationToken) => await AddAsync(project, cancellationToken).ConfigureAwait(false);

    public async Task<Project?> GetProByIdAsync(Guid id, CancellationToken cancellationToken) => await FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);

    public async Task<bool> DuplicateProjectExistsAsync(Guid projectId, string? name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }
        var projects = await GetAllAsync(cancellationToken).ConfigureAwait(false);

        return projects.Any(x => x.Id != projectId && string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IReadOnlyCollection<ProjectResponseDto>> GetAllProjectsAsync(CancellationToken cancellationToken)
    {
        var projects = await GetAllAsync(cancellationToken).ConfigureAwait(false);
        var users = await userRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var employeeProjects = await employeeProjectRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var userDictionary = users.ToDictionary(x => x.Id, x => x.Name);

        return projects.Select(project => new ProjectResponseDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            ProjectManagerId = project.ProjectManagerId,
            ProjectManagerName = userDictionary.TryGetValue(
                    project.ProjectManagerId,
                    out var managerName)
                        ? managerName
                        : string.Empty,
            TotalUsers = employeeProjects.Count(x => x.ProjectId == project.Id)
        }).ToList();
    }

    public async Task<ProjectResponseDto?> GetProjectByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var project = await FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);
        if (project is null)
        {
            return null;
        }
        var users = await userRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var employeeProjects = await employeeProjectRepository.FindAsync(x => x.ProjectId == project.Id, cancellationToken).ConfigureAwait(false);
        var userDictionary = users.ToDictionary(x => x.Id, x => x.Name);

        return new ProjectResponseDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            ProjectManagerId = project.ProjectManagerId,
            ProjectManagerName = userDictionary.TryGetValue(project.ProjectManagerId, out var managerName) ? managerName : string.Empty,
            TotalUsers = employeeProjects.Count
        };
    }
    public async Task<bool> ProjectExistsByIdAsync(Guid projectId, CancellationToken cancellationToken) => await AnyAsync(x => x.Id == projectId, cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyCollection<ProjectUserResponseDto>> GetProjectEmployeesAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var employeeProjects = await employeeProjectRepository
            .FindAsync(x => x.ProjectId == projectId, cancellationToken)
            .ConfigureAwait(false);

        var users = await userRepository.GetAllAsync(cancellationToken)
            .ConfigureAwait(false);

        var roles = await roleRepository.GetAllAsync(cancellationToken)
            .ConfigureAwait(false);

        var departments = await departmentRepository.GetAllAsync(cancellationToken)
            .ConfigureAwait(false);

        var positions = await positionRepository.GetAllAsync(cancellationToken)
            .ConfigureAwait(false);

        var branches = await branchRepository.GetAllAsync(cancellationToken)
            .ConfigureAwait(false);

        var userDictionary = users.ToDictionary(x => x.Id);
        var roleDictionary = roles.ToDictionary(x => x.Id, x => x.Name);
        var departmentDictionary = departments.ToDictionary(x => x.Id, x => x.Name);
        var positionDictionary = positions.ToDictionary(x => x.Id, x => x.Name);
        var branchDictionary = branches.ToDictionary(x => x.Id, x => x.Name);

        return employeeProjects.Select(employeeProject =>
            {
                userDictionary.TryGetValue(employeeProject.UserId, out var user);
                return new ProjectUserResponseDto
                {
                    UserId = user?.Id ?? Guid.Empty,
                    Name = user?.Name ?? string.Empty,
                    Email = user?.Email ?? string.Empty,
                    DOB = user?.DOB,
                    BranchName =
                        user is not null &&
                        branchDictionary.TryGetValue(user.BranchId, out var branchName)
                            ? branchName
                            : string.Empty,
                    DepartmentName =
                        user is not null &&
                        departmentDictionary.TryGetValue(user.DepartmentId, out var departmentName)
                            ? departmentName
                            : string.Empty,
                    PositionName =
                        user is not null &&
                        positionDictionary.TryGetValue(user.PositionId, out var positionName)
                            ? positionName
                            : string.Empty,
                    RoleName =
                        user is not null &&
                        roleDictionary.TryGetValue(user.RoleId, out var roleName)
                            ? roleName
                            : string.Empty
                };
            })
            .ToList();
    }
}
