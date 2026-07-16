using backend.Dto.ProjectDto;

namespace backend.IService
{
    public interface IProjectService
    {
        Task<Tuple<int, string>> CreateProject(ProjectDto dto);

        Task<Tuple<int, string>> UpdateProject(ProjectDto dto);

        Task<Tuple<int, List<ProjectResponseDto>, string>> GetAllProjects();

        Task<Tuple<int, ProjectResponseDto?, string>> GetProjectById(Guid id);

        Task<Tuple<int, List<ProjectUserResponseDto>, string>> GetProjectEmployees(Guid projectId);
    }
}