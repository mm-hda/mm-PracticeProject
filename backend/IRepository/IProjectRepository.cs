using backend.Dto.ProjectDtos;
using backend.Entities;
using backend.GenericRepositories;

namespace backend.IRepository;

public interface IProjectRepository : IGenericRepository<Project>
{
    Task<bool> ProjectExistsAsync(string? name, CancellationToken cancellationToken);
    Task<bool> ManagerExistsAsync(Guid managerId, CancellationToken cancellationToken);
    Task AddProjectAsync(Project project, CancellationToken cancellationToken);
    Task<Project?> GetProByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> DuplicateProjectExistsAsync(Guid projectId, string? name, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ProjectResponseDto>> GetAllProjectsAsync(CancellationToken cancellationToken);
    Task<ProjectResponseDto?> GetProjectByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ProjectExistsByIdAsync(Guid projectId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ProjectUserResponseDto>> GetProjectEmployeesAsync(Guid projectId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ProjectResponseDto>> GetProjectsByManagerIdAsync(Guid managerId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ProjectResponseDto>> GetEmployeeProjectsAsync(Guid userId, CancellationToken cancellationToken);
}
