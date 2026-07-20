using backend.Dto.ProjectDtos;

namespace backend.IService;

public interface IProjectService
{
    Task<Tuple<int>> CreateProject(ProjectDto dto);

    Task<Tuple<int>> UpdateProject(ProjectDto dto);

    Task<Tuple<int, List<ProjectResponseDto>>> GetAllProjects();

    Task<Tuple<int, ProjectResponseDto?>> GetProjectById(Guid id);

    Task<Tuple<int, List<ProjectUserResponseDto>>> GetProjectEmployees(Guid projectId);
}
