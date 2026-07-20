namespace backend.Dto.DashboardDtos;

public class RoleUserCountDto
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public int UserCount { get; set; }
}
