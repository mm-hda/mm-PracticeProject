using backend.Dto.PositionDto;
using backend.GenericResponse;
using backend.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Authorization;

namespace backend.Controllers
{
    [Authorize(Roles = RoleConstants.Admin + "," + RoleConstants.HR)]
    [ApiController]
    [Route("api/[controller]")]
    public class PositionController(IPositionService _positionService, ILogger<PositionController> _logger) : ControllerBase
    {

        [HttpPost("CreatePosition")]
        public async Task<IActionResult> CreatePosition([FromBody] PositionDto dto)
        {
            _logger.LogTrace("CreatePosition called with dto: {@PositionName}", dto.Name);
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid request body provided.");
                    return BadRequest(ResponseResults<string>.Failure(null, "Invalid request body"));
                }

                var result = await _positionService.CreatePosition(dto);

                if (result.Item1 == 0)
                {
                    _logger.LogWarning("Failed to create position.");
                    return BadRequest(ResponseResults<string>.Failure(null, result.Item2));
                }

                _logger.LogInformation("Position created successfully name: {PositionName}", dto.Name);
                return Ok(ResponseResults<string>.Success(null, result.Item2));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating position.");
                return StatusCode(500, ResponseResults<string>.Failure(null, ex.Message));
            }
        }

        [HttpPut("UpdatePosition")]
        public async Task<IActionResult> UpdatePosition([FromBody] PositionDto dto)
        {
            _logger.LogTrace("UpdatePosition called with dto: {@PositionName}", dto.Name);
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid request body provided.");
                    return BadRequest(ResponseResults<string>.Failure(null, "Invalid request body"));
                }

                var result = await _positionService.UpdatePosition(dto);

                if (result.Item1 == 0)
                {
                    _logger.LogWarning("Failed to update position.");
                    return BadRequest(ResponseResults<string>.Failure(null, result.Item2));
                }
                else
                {
                    _logger.LogInformation("Position updated successfully name: {PositionName}", dto.Name);
                    return Ok(ResponseResults<string>.Success(null, result.Item2));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating position.");
                return StatusCode(500, ResponseResults<string>.Failure(null, ex.Message));
            }
        }

        [HttpGet("GetAllPositions")]
        public async Task<IActionResult> GetAllPositions()
        {
            _logger.LogTrace("GetAllPositions called.");
            try
            {
                var result = await _positionService.GetAllPositions();


                _logger.LogInformation("Retrieved all positions count: {Count}", result.Item2?.Count ?? 0);
                return Ok(ResponseResults<List<PositionResponseDto>>.Success(result.Item2, result.Item3));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all positions.");
                return StatusCode(500, ResponseResults<string>.Failure(null, ex.Message));
            }
        }

        [HttpGet("GetPositionById/{id}")]
        public async Task<IActionResult> GetPositionById(Guid id)
        {
            _logger.LogTrace("GetPositionById called with id: {PositionId}", id);
            try
            {
                if (id == Guid.Empty)
                {
                    _logger.LogWarning("Invalid position id provided.");
                    return BadRequest(ResponseResults<string>.Failure(null, "Invalid position id"));
                }

                var result = await _positionService.GetPositionById(id);

                if (result.Item1 == 0)
                {
                    _logger.LogWarning("Position not found for id: {PositionId}", id);
                    return NotFound(ResponseResults<string>.Failure(null, result.Item3));
                }

                _logger.LogInformation("Retrieved position with id: {PositionId} name: {PositionName}", id, result.Item2?.Name);
                return Ok(ResponseResults<PositionResponseDto>.Success(result.Item2, result.Item3));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching position with id: {PositionId}", id);
                return StatusCode(500, ResponseResults<string>.Failure(null, ex.Message));
            }
        }

        [HttpGet("GetPositionsByDepartment/{departmentId}")]
        public async Task<IActionResult> GetPositionsByDepartment(Guid departmentId)
        {
            _logger.LogTrace("GetPositionsByDepartment called with id: {DepartmentId}", departmentId);
            try
            {
                if (departmentId == Guid.Empty)
                {
                    _logger.LogWarning("Invalid department id provided.");
                    return BadRequest(ResponseResults<string>.Failure(null, "Invalid department id"));
                }

                var result = await _positionService.GetPositionsByDepartment(departmentId);

                if (result.Item1 == 0)
                {
                    _logger.LogWarning("No positions found for department id: {DepartmentId}", departmentId);
                    return NotFound(ResponseResults<List<PositionResponseDto>>.Failure(null, result.Item3));
                }

                var logs = result.Item2.Select(p => new { p.Id, p.Name }).ToList();
                _logger.LogInformation("Retrieved positions for department id: {DepartmentId} results: {logs}", departmentId, logs);
                return Ok(ResponseResults<List<PositionResponseDto>>.Success(result.Item2, result.Item3));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching positions for department id: {DepartmentId}", departmentId);
                return StatusCode(500, ResponseResults<string>.Failure(null, ex.Message));
            }
        }

        [HttpGet("GetPositionUsers/{positionId}")]
        public async Task<IActionResult> GetPositionUsers(Guid positionId)
        {
            _logger.LogTrace("GetPositionUsers called with id: {PositionId}", positionId);
            try
            {
                if (positionId == Guid.Empty)
                {
                    _logger.LogWarning("Invalid position id provided.");
                    return BadRequest(ResponseResults<string>.Failure(null, "Invalid position id"));
                }

                var result = await _positionService.GetPositionUsers(positionId);

                if (result.Item1 == 0)
                {
                    _logger.LogWarning("{Message} id: {PositionId}", result.Item3, positionId);
                    return NotFound(ResponseResults<List<PositionUserResponseDto>>.Failure(null, result.Item3));
                }

                var logs = result.Item2.Select(u => new { u.UserId, u.Name }).ToList();
                _logger.LogInformation("Retrieved users for position id: {PositionId} results: {logs}", positionId, logs);
                return Ok(ResponseResults<List<PositionUserResponseDto>>.Success(result.Item2, result.Item3));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching users for position id: {PositionId}", positionId);
                return StatusCode(500, ResponseResults<string>.Failure(null, ex.Message));
            }
        }
    }
}