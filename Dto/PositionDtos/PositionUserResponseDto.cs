namespace backend.Dto.PositionDtos;

public class PositionUserResponseDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime? DOB { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string PositionName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
}
