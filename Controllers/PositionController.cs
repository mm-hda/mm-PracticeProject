using backend.Dto.PositionDtos;
using backend.GenericResponse;
using backend.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Authorization;

namespace backend.Controllers;

[Authorize(Roles = RoleConstants.Admin + "," + RoleConstants.HR)]
[ApiController]
[Route("api/[controller]")]
public class PositionController(IPositionService positionService, ILogger<PositionController> logger) : ControllerBase
{

    [HttpPost("CreatePosition")]
    public async Task<IActionResult> CreatePositionAsync([FromBody] PositionDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        logger.LogTrace("CreatePosition called with dto: {@PositionName}", dto.Name);
        try
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning("Invalid request body provided.");
                return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
            }

            var result = await positionService.CreatePosition(dto).ConfigureAwait(false);

            if (result.Item1 == 0)
            {
                logger.LogWarning("Failed to create position.");
                return BadRequest(ResponseResults<string>.Failure(result.Item1));
            }

            logger.LogInformation("Position created successfully name: {PositionName}", dto.Name);
            return Ok(ResponseResults<string>.Success(result.Item1));

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating position.");
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }

    [HttpPut("UpdatePosition")]
    public async Task<IActionResult> UpdatePositionAsync([FromBody] PositionDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        logger.LogTrace("UpdatePosition called with dto: {@PositionName}", dto.Name);
        try
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning("Invalid request body provided.");
                return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
            }

            var result = await positionService.UpdatePosition(dto).ConfigureAwait(false);

            if (result.Item1 == 0)
            {
                logger.LogWarning("Failed to update position.");
                return BadRequest(ResponseResults<string>.Failure(result.Item1));
            }
            else
            {
                logger.LogInformation("Position updated successfully name: {PositionName}", dto.Name);
                return Ok(ResponseResults<string>.Success(result.Item1));
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while updating position.");
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }

    [HttpGet("GetAllPositions")]
    public async Task<IActionResult> GetAllPositionsAsync()
    {
        logger.LogTrace("GetAllPositions called.");
        try
        {
            var result = await positionService.GetAllPositions().ConfigureAwait(false);

            logger.LogInformation("Retrieved all positions count: {Count}", result.Item2?.Count ?? 0);
            return Ok(ResponseResults<List<PositionResponseDto>>.Success(result.Item1, result.Item2));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching all positions.");
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }

    [HttpGet("GetPositionById/{id}")]
    public async Task<IActionResult> GetPositionByIdAsync(Guid id)
    {
        logger.LogTrace("GetPositionById called with id: {PositionId}", id);
        try
        {
            if (id == Guid.Empty)
            {
                logger.LogWarning("Invalid position id provided.");
                return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
            }

            var result = await positionService.GetPositionById(id).ConfigureAwait(false);

            if (result.Item1 == 0)
            {
                logger.LogWarning("Position not found for id: {PositionId}", id);
                return NotFound(ResponseResults<string>.Failure(result.Item1));
            }

            logger.LogInformation("Retrieved position with id: {PositionId} name: {PositionName}", id, result.Item2?.Name);
            return Ok(ResponseResults<PositionResponseDto>.Success(result.Item1, result.Item2));

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching position with id: {PositionId}", id);
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }

    [HttpGet("GetPositionsByDepartment/{departmentId}")]
    public async Task<IActionResult> GetPositionsByDepartmentAsync(Guid departmentId)
    {
        logger.LogTrace("GetPositionsByDepartment called with id: {DepartmentId}", departmentId);
        try
        {
            if (departmentId == Guid.Empty)
            {
                logger.LogWarning("Invalid department id provided.");
                return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
            }

            var result = await positionService.GetPositionsByDepartment(departmentId).ConfigureAwait(false);

            if (result.Item1 == 0)
            {
                logger.LogWarning("No positions found for department id: {DepartmentId}", departmentId);
                return NotFound(ResponseResults<List<PositionResponseDto>>.Failure(result.Item1));
            }

            var logs = result.Item2.Select(p => new { p.Id, p.Name }).ToList();
            logger.LogInformation("Retrieved positions for department id: {DepartmentId} results: {Logs}", departmentId, logs);
            return Ok(ResponseResults<List<PositionResponseDto>>.Success(result.Item1, result.Item2));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching positions for department id: {DepartmentId}", departmentId);
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }

    [HttpGet("GetPositionUsers/{positionId}")]
    public async Task<IActionResult> GetPositionUsersAsync(Guid positionId)
    {
        logger.LogTrace("GetPositionUsers called with id: {PositionId}", positionId);
        try
        {
            if (positionId == Guid.Empty)
            {
                logger.LogWarning("Invalid position id provided.");
                return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
            }

            var result = await positionService.GetPositionUsers(positionId).ConfigureAwait(false);

            if (result.Item1 == 0)
            {
                logger.LogWarning("StatusCode: {StatusCode}", result.Item1);
                return NotFound(ResponseResults<List<PositionUserResponseDto>>.Failure(result.Item1));
            }

            var logs = result.Item2.Select(u => new { u.UserId, u.Name }).ToList();
            logger.LogInformation("Retrieved users for position id: {PositionId} results: {Logs}", positionId, logs);
            return Ok(ResponseResults<List<PositionUserResponseDto>>.Success(result.Item1, result.Item2));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching users for position id: {PositionId}", positionId);
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }
}
