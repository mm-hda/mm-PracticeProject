using backend.Dto.PositionDtos;
using backend.GenericResponse;
using backend.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Authorization;

namespace backend.Controllers.V1;

// [Authorize(Roles = RoleConstants.Admin + "," + RoleConstants.HR)]
[ApiController]
[Route("api/[controller]")]
public class PositionController(IPositionService positionService, ILogger<PositionController> logger) : ControllerBase
{

    [HttpPost("CreatePosition")]
    public async Task<IActionResult> CreatePositionAsync([FromBody] PositionDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);

        logger.LogTrace("CreatePosition called with dto: {@PositionName}", dto.Name);

        var result = await positionService.CreatePosition(dto, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Failed to create position.");
            return BadRequest(ResponseResults<string>.Failure(result.StatusCode));
        }

        logger.LogInformation("Position created successfully name: {PositionName}", dto.Name);
        return Ok(ResponseResults<string>.Success(result.StatusCode));

    }

    [HttpPut("UpdatePosition")]
    public async Task<IActionResult> UpdatePositionAsync([FromBody] PositionDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);

        logger.LogTrace("UpdatePosition called with dto: {@PositionName}", dto.Name);

        var result = await positionService.UpdatePosition(dto, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Failed to update position.");
            return BadRequest(ResponseResults<string>.Failure(result.StatusCode));
        }

        logger.LogInformation("Position updated successfully name: {PositionName}", dto.Name);
        return Ok(ResponseResults<string>.Success(result.StatusCode));
    }

    [HttpGet("GetAllPositions")]
    public async Task<IActionResult> GetAllPositionsAsync(CancellationToken cancellationToken)
    {
        logger.LogTrace("GetAllPositions called.");

        var result = await positionService.GetAllPositions(cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            logger.LogWarning("No positions found.");
            return NotFound(ResponseResults<string>.Failure(result.StatusCode));
        }
        logger.LogInformation("Retrieved all positions count: {Count}", result.Data?.Count);
        return Ok(ResponseResults<IReadOnlyCollection<PositionResponseDto>>.Success(result.StatusCode, result.Data));

    }

    [HttpGet("GetPositionById/{id}")]
    public async Task<IActionResult> GetPositionByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        logger.LogTrace("GetPositionById called with id: {PositionId}", id);

        if (id == Guid.Empty)
        {
            logger.LogWarning("Invalid position id provided.");
            return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
        }

        var result = await positionService.GetPositionById(id, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Position not found for id: {PositionId}", id);
            return NotFound(ResponseResults<string>.Failure(result.StatusCode));
        }

        logger.LogInformation("Retrieved position with id: {PositionId} name: {PositionName}", id, result.Data?.Name);
        return Ok(ResponseResults<PositionResponseDto?>.Success(result.StatusCode, result.Data));
    }

    [HttpGet("GetPositionsByDepartment/{departmentId}")]
    public async Task<IActionResult> GetPositionsByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken)
    {
        logger.LogTrace("GetPositionsByDepartment called with id: {DepartmentId}", departmentId);

        if (departmentId == Guid.Empty)
        {
            logger.LogWarning("Invalid department id provided.");
            return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
        }

        var result = await positionService.GetPositionsByDepartment(departmentId, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            logger.LogWarning("No positions found for department id: {DepartmentId}", departmentId);
            return NotFound(ResponseResults<IReadOnlyCollection<PositionResponseDto>>.Failure(result.StatusCode));
        }

        var logs = result.Data?.Select(p => new { p.Id, p.Name }).ToList();
        logger.LogInformation("Retrieved positions for department id: {DepartmentId} results: {Logs}", departmentId, logs);
        return Ok(ResponseResults<IReadOnlyCollection<PositionResponseDto>>.Success(result.StatusCode, result.Data));
    }

    [HttpGet("GetPositionUsers/{positionId}")]
    public async Task<IActionResult> GetPositionUsersAsync(Guid positionId, CancellationToken cancellationToken)
    {
        logger.LogTrace("GetPositionUsers called with id: {PositionId}", positionId);

        if (positionId == Guid.Empty)
        {
            logger.LogWarning("Invalid position id provided.");
            return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
        }

        var result = await positionService.GetPositionUsers(positionId, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            logger.LogWarning("StatusCode: {StatusCode}", result.StatusCode);
            return NotFound(ResponseResults<IReadOnlyCollection<PositionUserResponseDto>>.Failure(result.StatusCode));
        }

        var logs = result.Data?.Select(u => new { u.UserId, u.Name }).ToList();
        logger.LogInformation("Retrieved users for position id: {PositionId} results: {Logs}", positionId, logs);
        return Ok(ResponseResults<IReadOnlyCollection<PositionUserResponseDto>>.Success(result.StatusCode, result.Data));
    }
}
