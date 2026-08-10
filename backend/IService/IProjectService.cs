using backend.Dto.ProjectDtos;
using backend.GenericResponse;

namespace backend.IService;

public interface IProjectService
{
    Task<ServiceResponse<object>> CreateProject(ProjectDto dto, CancellationToken cancellationToken);

    Task<ServiceResponse<object>> UpdateProject(ProjectDto dto, CancellationToken cancellationToken);

    Task<ServiceResponse<IReadOnlyCollection<ProjectResponseDto>>> GetAllProjects(CancellationToken cancellationToken);

    Task<ServiceResponse<ProjectResponseDto?>> GetProjectById(Guid id, CancellationToken cancellationToken);

    Task<ServiceResponse<IReadOnlyCollection<ProjectUserResponseDto>>> GetProjectEmployees(Guid projectId, CancellationToken cancellationToken);

    Task<ServiceResponse<IReadOnlyCollection<ProjectResponseDto>>> GetProjectsByManagerId(Guid managerId, CancellationToken cancellationToken);

    Task<ServiceResponse<IReadOnlyCollection<ProjectResponseDto>>> GetEmployeeProjects(Guid userId, CancellationToken cancellationToken);
}
