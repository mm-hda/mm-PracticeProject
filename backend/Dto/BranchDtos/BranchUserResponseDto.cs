namespace backend.Dto.BranchDtos;

public class BranchUserResponseDto
{
    public Guid UserId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? DepartmentName { get; set; }
    public string? PositionName { get; set; }
}
