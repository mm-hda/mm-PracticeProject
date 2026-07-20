using System.Diagnostics.CodeAnalysis;
namespace backend.Dto.DashboardDtos;

[SuppressMessage("Design", "CA1002:Do not expose generic lists")]
[SuppressMessage("Usage", "CA2227:Collection properties should be read only")]
public class DepartmentDashboardDto
{
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int UserCount { get; set; }
    public int TotalPositions { get; set; }
    public List<PositionDashboardDto> Positions { get; set; } = [];
}
