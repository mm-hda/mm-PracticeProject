using backend.Dto.DashboardDtos;

namespace backend.IService;

public interface IDashboardService
{
    Task<Tuple<int, DashboardResponseDto>> GetDashboard();
}
