using backend.Dto.UserDto;
using backend.Dto.Common;
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
    public class UserController(IUserService _userService, ILogger<UserController> _logger) : ControllerBase
    {

        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers([FromQuery] PaginationDto dto)
        {
            _logger.LogTrace("GetAllUsers called. Page:{PageNumber}, PageSize:{PageSize}", dto.PageNumber, dto.PageSize);

            try
            {
                var result = await _userService.GetAllUsers(dto);

                if (result.Item1 == 0)
                {
                    _logger.LogWarning("{Message}", result.Item3);
                    return NotFound(ResponseResults<List<UserResponseDto>>.Failure(null, result.Item4));
                }

                _logger.LogInformation("Retrieved all users successfully. {COUNT}", result.Item2?.Count ?? 0);
                return Ok(ResponseResults<List<UserResponseDto>>.Success(result.Item2, result.Item3 ?? new PaginationMetaDto(), result.Item4));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving users.");

                return StatusCode(500, ResponseResults<string>.Failure(null, ex.Message));
            }
        }

        [HttpGet("GetUserBySearch")]
        public async Task<IActionResult> GetUserBySearch([FromQuery] string searchTerm)
        {
            _logger.LogTrace("GetUserBySearch called with searchTerm: {SearchTerm}", searchTerm);
            try
            {
                var result = await _userService.GetUserBySearch(searchTerm);

                if (result.Item1 == 0)
                {
                    _logger.LogWarning("{Message} search term: {SearchTerm}", result.Item3, searchTerm);
                    return NotFound(ResponseResults<string>.Failure(null, result.Item3));
                }

                var logs = result.Item2.Select(u => new { u.UserId, u.Name, u.Email }).ToList();

                _logger.LogInformation("Retrieved users for search term: {SearchTerm} and logs: {Logs}", searchTerm, logs);
                return Ok(ResponseResults<List<UserResponseDto>>.Success(result.Item2, result.Item3));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while searching for users with term: {SearchTerm}", searchTerm);
                return StatusCode(500, ResponseResults<string>.Failure(null, ex.Message));
            }
        }


        [HttpGet("GetUserById/{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            _logger.LogTrace("GetUserById called with id: {UserId}", id);
            try
            {
                if (id == Guid.Empty)
                {
                    _logger.LogWarning("Invalid user id provided: {UserId}", id);
                    return BadRequest(ResponseResults<string>.Failure(null, "Invalid user id"));
                }

                var result = await _userService.GetUserById(id);

                if (result.Item1 == 0)
                {
                    _logger.LogWarning("{Message} id: {UserId}", result.Item3, id);
                    return NotFound(ResponseResults<string>.Failure(null, result.Item3));
                }

                var logs = new { result.Item2?.UserId, result.Item2?.Name, result.Item2?.Email };
                _logger.LogInformation("Retrieved user with id: {UserId} and logs: {Logs}", id, logs);

                return Ok(ResponseResults<UserResponseDto>.Success(result.Item2, result.Item3));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching user with id: {UserId}", id);
                return StatusCode(500, ResponseResults<string>.Failure(null, ex.Message));
            }
        }

        [HttpPost("GetUsersByFilter")]
        public async Task<IActionResult> GetUsersByFilter([FromBody] UserFilterDto dto)
        {
            _logger.LogTrace("GetUsersByFilter called.");
            try
            {
                var result = await _userService.GetUsersByFilter(dto);

                _logger.LogInformation("Retrieved users by filter the count: {Count}", result.Item2?.Count ?? 0);
                return Ok(ResponseResults<List<UserResponseDto>>.Success(result.Item2, result.Item3));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching users by filter.");
                return StatusCode(500, ResponseResults<string>.Failure(null, ex.Message));
            }
        }
    }
}