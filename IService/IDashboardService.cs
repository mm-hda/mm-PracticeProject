using backend.Dto.DashboardDtos;
using backend.GenericResponse;
namespace backend.IService;

public interface IDashboardService
{
    Task<ServiceResponse<DashboardResponseDto>> GetDashboard();
}
