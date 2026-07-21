namespace backend.Dto.DashboardDtos;

public class RoleUserCountDto
{
    public Guid RoleId { get; set; }
    public string? RoleName { get; set; }
    public int UserCount { get; set; }
}
