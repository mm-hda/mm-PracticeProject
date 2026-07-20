using backend.Dto.DashboardDtos;
using backend.GenericResponse;
using backend.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Authorization;

namespace backend.Controllers;

[Authorize(Roles = RoleConstants.Admin)]
[ApiController]
[Route("api/[controller]")]
public class DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger) : ControllerBase
{
    [HttpGet("GetDashboard")]
    public async Task<IActionResult> GetDashboardAsync()
    {
        logger.LogTrace("GetDashboard called.");
        try
        {
            var result = await dashboardService.GetDashboard().ConfigureAwait(false);

            if (result.Item1 == 0)
            {
                logger.LogWarning("Failed to retrieve dashboard data.");
                return BadRequest(ResponseResults<string>.Failure(result.Item1));
            }

            logger.LogInformation("Dashboard data retrieved successfully.");
            return Ok(ResponseResults<DashboardResponseDto>.Success(result.Item1, result.Item2));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching dashboard data.");
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }
}
