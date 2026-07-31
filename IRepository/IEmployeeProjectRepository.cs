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
    void Remove(EmployeeProject employeeProject, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<EmployeeProjectResponseDto>> GetAllEmployeeProjectsAsync(CancellationToken cancellationToken);
    Task<int> GetUserProjectsCountAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ProjectResponseDto>> GetUserProjectsByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken);
}
