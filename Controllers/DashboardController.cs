using backend.Dto.DashboardDto;
using backend.GenericResponse;
using backend.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Authorization;

namespace backend.Controllers
{
    [Authorize(Roles = RoleConstants.Admin)]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController(IDashboardService _dashboardService, ILogger<DashboardController> _logger) : ControllerBase
    {

        [HttpGet("GetDashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            _logger.LogTrace("GetDashboard called.");
            try
            {
                var result = await _dashboardService.GetDashboard();

                if (result.Item1 == 0)
                {
                    _logger.LogWarning("Failed to retrieve dashboard data.");
                    return BadRequest(ResponseResults<string>.Failure(null, result.Item3));
                }

                _logger.LogInformation("Dashboard data retrieved successfully.");
                return Ok(ResponseResults<DashboardResponseDto>.Success(result.Item2, result.Item3));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching dashboard data.");
                return StatusCode(500, ResponseResults<string>.Failure(null, ex.Message));
            }
        }
    }
}