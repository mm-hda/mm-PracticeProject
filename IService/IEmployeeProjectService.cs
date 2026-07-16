using backend.Dto.EmployeeProjectDto;
using backend.Dto.ProjectDto;
using backend.Dto.Common;

namespace backend.IService
{
    public interface IEmployeeProjectService
    {
        Task<Tuple<int, string>> CreateEmployeeProject(EmployeeProjectDto dto);

        Task<Tuple<int, string>> RemoveEmployeeProject(Guid id);

        Task<Tuple<int, List<EmployeeProjectResponseDto>, string>> GetAllEmployeeProjects();

        Task<Tuple<int, List<ProjectResponseDto>, PaginationMetaDto?, string>> GetUserProjectsByUserId(Guid userId, PaginationDto dto);
    }
}