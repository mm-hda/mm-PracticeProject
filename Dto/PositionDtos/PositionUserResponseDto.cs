namespace backend.Dto.PositionDtos;

public class PositionUserResponseDto
{
    public Guid UserId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public DateTime? DOB { get; set; }
    public string? BranchName { get; set; }
    public string? RoleName { get; set; }
}
