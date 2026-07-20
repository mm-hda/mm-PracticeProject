using backend.Dto.EmployeeProjectDtos;
using backend.Dto.ProjectDtos;
using backend.Dto.CommonDtos;

namespace backend.IService;

public interface IEmployeeProjectService
{
    Task<Tuple<int>> CreateEmployeeProject(EmployeeProjectDto dto);

    Task<Tuple<int>> RemoveEmployeeProject(Guid id);

    Task<Tuple<int, List<EmployeeProjectResponseDto>>> GetAllEmployeeProjects();

    Task<Tuple<int, List<ProjectResponseDto>, PaginationMetaDto?>> GetUserProjectsByUserId(Guid userId, PaginationDto dto);
}
