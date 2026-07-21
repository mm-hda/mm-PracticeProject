namespace backend.Dto.EmployeeProjectDtos;

public class EmployeeProjectResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public string? RoleName { get; set; }
    public Guid ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public DateTime AssignedDate { get; set; }
}
