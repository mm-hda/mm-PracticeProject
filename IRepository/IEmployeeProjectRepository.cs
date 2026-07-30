using backend.Dto.EmployeeProjectDtos;
using backend.Dto.ProjectDtos;
using backend.Entities;
using backend.GenericRepositories;
namespace backend.IRepository;

public interface IEmployeeProjectRepository : IGenericRepository<EmployeeProject>
{
    Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken);
    Task<Project?> GetProjectByIdAsync(Guid projectId, CancellationToken cancellationToken);
    Task<bool> EmployeeProjectExistsAsync(Guid userId, Guid projectId, CancellationToken cancellationToken);
    Task AddEmployeeProjectAsync(EmployeeProject employeeProject, CancellationToken cancellationToken);
    Task<EmployeeProject?> GetEmployeeProjectByIdAsync(Guid id, CancellationToken cancellationToken);
    void Remove(EmployeeProject employeeProject);
    Task<IReadOnlyCollection<EmployeeProjectResponseDto>> GetAllEmployeeProjectsAsync();
    Task<bool> UserExistsAsync(Guid userId);
    Task<int> GetUserProjectsCountAsync(Guid userId);
    Task<IReadOnlyCollection<ProjectResponseDto>> GetUserProjectsByUserIdAsync(Guid userId, int pageNumber, int pageSize);
}
