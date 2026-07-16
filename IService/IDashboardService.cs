using backend.Dto.DashboardDto;

namespace backend.IService
{
    public interface IDashboardService
    {
        Task<Tuple<int, DashboardResponseDto, string>> GetDashboard();
    }
}