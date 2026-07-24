using backend.Dto.EmployeeProjectDtos;
using backend.Dto.ProjectDtos;
using backend.Dto.CommonDtos;
using backend.GenericResponse;
namespace backend.IService;

public interface IEmployeeProjectService
{
    Task<ServiceResponse<object>> CreateEmployeeProject(EmployeeProjectDto dto, CancellationToken cancellationToken);

    Task<ServiceResponse<object>> RemoveEmployeeProject(Guid id, CancellationToken cancellationToken);

    Task<ServiceResponse<IReadOnlyCollection<EmployeeProjectResponseDto>>> GetAllEmployeeProjects();

    Task<ServiceResponse<IReadOnlyCollection<ProjectResponseDto>>> GetUserProjectsByUserId(Guid userId, PaginationDto dto);
}
