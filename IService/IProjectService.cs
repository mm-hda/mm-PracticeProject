using backend.Dto.ProjectDtos;

namespace backend.IService;

public interface IProjectService
{
    Task<Tuple<int>> CreateProject(ProjectDto dto);

    Task<Tuple<int>> UpdateProject(ProjectDto dto);

    Task<Tuple<int, IReadOnlyCollection<ProjectResponseDto>>> GetAllProjects();

    Task<Tuple<int, ProjectResponseDto?>> GetProjectById(Guid id);

    Task<Tuple<int, IReadOnlyCollection<ProjectUserResponseDto>>> GetProjectEmployees(Guid projectId);
}
