using backend.Dto.DashboardDtos;
using backend.GenericResponse;
using backend.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Authorization;

namespace backend.Controllers.V1;

[Authorize(Roles = RoleConstants.Admin)]
[ApiController]
[Route("api/[controller]")]
public class DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger) : ControllerBase
{
    [HttpGet("GetDashboard")]
    public async Task<IActionResult> GetDashboardAsync()
    {
        logger.LogTrace("GetDashboard called.");

        var result = await dashboardService.GetDashboard(CancellationToken.None).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Failed to retrieve dashboard data.");
            return BadRequest(ResponseResults<string>.Failure(result.StatusCode));
        }

        logger.LogInformation("Dashboard data retrieved successfully.");
        return Ok(ResponseResults<DashboardResponseDto>.Success(result.StatusCode, result.Data));
    }
}
