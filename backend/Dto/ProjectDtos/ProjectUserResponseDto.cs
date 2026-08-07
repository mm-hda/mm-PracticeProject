namespace backend.Dto.ProjectDtos;

public class ProjectUserResponseDto
{
    public Guid UserId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public DateTime? DOB { get; set; }
    public string? BranchName { get; set; }
    public string? DepartmentName { get; set; }
    public string? PositionName { get; set; }
    public string? RoleName { get; set; }
}
