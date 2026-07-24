using backend.Dto.EmployeeProjectDtos;
using backend.Dto.ProjectDtos;
using backend.Entities;

namespace backend.IRepository;

public interface IEmployeeProjectRepository
{
    Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken);
    Task<Project?> GetProjectByIdAsync(Guid projectId, CancellationToken cancellationToken);
    Task<bool> EmployeeProjectExistsAsync(Guid userId, Guid projectId, CancellationToken cancellationToken);
    Task AddAsync(EmployeeProject employeeProject, CancellationToken cancellationToken);
    Task<EmployeeProject?> GetEmployeeProjectByIdAsync(Guid id, CancellationToken cancellationToken);
    void Remove(EmployeeProject employeeProject);
    Task<IReadOnlyCollection<EmployeeProjectResponseDto>> GetAllEmployeeProjectsAsync();
    Task<bool> UserExistsAsync(Guid userId);
    Task<int> GetUserProjectsCountAsync(Guid userId);
    Task<IReadOnlyCollection<ProjectResponseDto>> GetUserProjectsByUserIdAsync(Guid userId, int pageNumber, int pageSize);
}
