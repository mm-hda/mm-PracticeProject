using backend.Data;
using backend.Dto.EmployeeProjectDtos;
using backend.Dto.ProjectDtos;
using backend.Entities;
using backend.GenericRepositories;
using backend.IRepository;

namespace backend.Repositories;

internal sealed class EmployeeProjectRepository(
    AppDbContext context,
    IGenericRepository<User> userRepository,
    IGenericRepository<Role> roleRepository,
    IGenericRepository<Project> projectRepository)
    : GenericRepository<EmployeeProject>(context), IEmployeeProjectRepository
{
    public async Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken)
        => await userRepository.AnyAsync(x => x.Id == userId, cancellationToken).ConfigureAwait(false);

    public async Task<Project?> GetProjectByIdAsync(Guid projectId, CancellationToken cancellationToken)
        => await projectRepository.FirstOrDefaultAsync(x => x.Id == projectId, cancellationToken).ConfigureAwait(false);

    public async Task<bool> EmployeeProjectExistsAsync(Guid userId, Guid projectId, CancellationToken cancellationToken)
        => await AnyAsync(x => x.UserId == userId && x.ProjectId == projectId, cancellationToken).ConfigureAwait(false);

    public async Task AddEmployeeProjectAsync(EmployeeProject employeeProject, CancellationToken cancellationToken)
        => await AddAsync(employeeProject, cancellationToken).ConfigureAwait(false);

    public async Task<EmployeeProject?> GetEmployeeProjectByIdAsync(Guid id, CancellationToken cancellationToken)
        => await FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);

    public void Remove(EmployeeProject employeeProject, CancellationToken cancellationToken) => Delete(employeeProject, cancellationToken);

    public async Task<int> GetUserProjectsCountAsync(Guid userId, CancellationToken cancellationToken)
    {
        var employeeProjects = await FindAsync(x => x.UserId == userId, cancellationToken).ConfigureAwait(false);
        return employeeProjects.Count;
    }

    public async Task<IReadOnlyCollection<EmployeeProjectResponseDto>> GetAllEmployeeProjectsAsync(CancellationToken cancellationToken)
    {
        var employeeProjects = await GetAllAsync(cancellationToken).ConfigureAwait(false);
        var users = await userRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var projects = await projectRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var roles = await roleRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var userDictionary = users.ToDictionary(x => x.Id);
        var projectDictionary = projects.ToDictionary(x => x.Id);
        var roleDictionary = roles.ToDictionary(x => x.Id, x => x.Name);

        return employeeProjects.Select(ep =>
        {
            userDictionary.TryGetValue(ep.UserId, out var user);
            projectDictionary.TryGetValue(ep.ProjectId, out var project);

            return new EmployeeProjectResponseDto
            {
                Id = ep.Id,
                UserId = ep.UserId,
                UserName = user?.Name ?? string.Empty,
                UserEmail = user?.Email ?? string.Empty,
                RoleName = user != null && roleDictionary.TryGetValue(user.RoleId, out var roleName)
                    ? roleName
                    : string.Empty,
                ProjectId = ep.ProjectId,
                ProjectName = project?.Name ?? string.Empty,
                AssignedDate = ep.AssignedDate
            };
        }).ToList();
    }

    public async Task<IReadOnlyCollection<ProjectResponseDto>> GetUserProjectsByUserIdAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var employeeProjects = await FindAsync(x => x.UserId == userId, cancellationToken).ConfigureAwait(false);
        var projects = await projectRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var users = await userRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var projectDictionary = projects.ToDictionary(x => x.Id);
        var userDictionary = users.ToDictionary(x => x.Id);

        var pagedProjects = employeeProjects
            .OrderByDescending(x =>
                projectDictionary.TryGetValue(x.ProjectId, out var project)
                    ? project.StartDate
                    : DateTime.MinValue)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return pagedProjects.Select(ep =>
        {
            projectDictionary.TryGetValue(ep.ProjectId, out var project);

            var totalUsers = employeeProjects.Count(x => x.ProjectId == ep.ProjectId);

            userDictionary.TryGetValue(
                project?.ProjectManagerId ?? Guid.Empty,
                out var manager);

            return new ProjectResponseDto
            {
                Id = project?.Id ?? Guid.Empty,
                Name = project?.Name ?? string.Empty,
                Description = project?.Description ?? string.Empty,
                StartDate = project?.StartDate ?? DateTime.MinValue,
                EndDate = project?.EndDate,
                ProjectManagerId = project?.ProjectManagerId ?? Guid.Empty,
                ProjectManagerName = manager?.Name ?? string.Empty,
                TotalUsers = totalUsers
            };
        }).ToList();
    }
}
