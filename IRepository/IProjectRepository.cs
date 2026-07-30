using backend.Dto.ProjectDtos;
using backend.Entities;
using backend.GenericRepositories;

namespace backend.IRepository;

public interface IProjectRepository : IGenericRepository<Project>
{
    Task<bool> ProjectExistsAsync(string? name);
    Task<bool> ManagerExistsAsync(Guid managerId);
    Task AddProjectAsync(Project project, CancellationToken cancellationToken);
    Task<Project?> GetProByIdAsync(Guid id);
    Task<bool> DuplicateProjectExistsAsync(Guid projectId, string? name);
    Task<IReadOnlyCollection<ProjectResponseDto>> GetAllProjectsAsync();
    Task<ProjectResponseDto?> GetProjectByIdAsync(Guid id);
    Task<bool> ProjectExistsByIdAsync(Guid projectId);
    Task<IReadOnlyCollection<ProjectUserResponseDto>> GetProjectEmployeesAsync(Guid projectId);
}
