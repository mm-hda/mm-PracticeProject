using backend.Dto.ProjectDtos;
using backend.Entities;

namespace backend.IRepository;

public interface IProjectRepository
{
    Task<bool> ProjectExistsAsync(string? name, CancellationToken cancellationToken);
    Task<bool> ManagerExistsAsync(Guid managerId, CancellationToken cancellationToken);
    Task AddAsync(Project project, CancellationToken cancellationToken);
    Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> DuplicateProjectExistsAsync(Guid projectId, string? name, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ProjectResponseDto>> GetAllProjectsAsync();
    Task<ProjectResponseDto?> GetProjectByIdAsync(Guid id);
    Task<bool> ProjectExistsByIdAsync(Guid projectId);
    Task<IReadOnlyCollection<ProjectUserResponseDto>> GetProjectEmployeesAsync(Guid projectId);
}
