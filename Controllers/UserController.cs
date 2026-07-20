using backend.Dto.UserDtos;
using backend.Dto.CommonDtos;
using backend.GenericResponse;
using backend.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Authorization;

namespace backend.Controllers;

[Authorize(Roles = RoleConstants.Admin)]
[ApiController]
[Route("api/[controller]")]
public class UserController(IUserService userService, ILogger<UserController> logger) : ControllerBase
{

    [HttpGet("GetAllUsers")]
    public async Task<IActionResult> GetAllUsersAsync([FromQuery] PaginationDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        logger.LogTrace("GetAllUsers called. Page:{PageNumber}, PageSize:{PageSize}", dto.PageNumber, dto.PageSize);

        try
        {
            var result = await userService.GetAllUsers(dto).ConfigureAwait(false);

            if (result.Item1 == 0)
            {
                logger.LogWarning("{Status code}", result.Item1);
                return NotFound(ResponseResults<List<UserResponseDto>>.Failure(result.Item1));
            }

            logger.LogInformation("Retrieved all users successfully. {COUNT}", result.Item2?.Count ?? 0);
            return Ok(ResponseResults<List<UserResponseDto>>.Success(result.Item1, result.Item2, result.Item3));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while retrieving users.");
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }

    [HttpGet("GetUserBySearch")]
    public async Task<IActionResult> GetUserBySearchAsync([FromQuery] string searchTerm)
    {
        logger.LogTrace("GetUserBySearch called with searchTerm: {SearchTerm}", searchTerm);
        try
        {
            var result = await userService.GetUserBySearch(searchTerm).ConfigureAwait(false);

            if (result.Item1 == 0)
            {
                logger.LogWarning("{Status code} search term: {SearchTerm}", result.Item1, searchTerm);
                return NotFound(ResponseResults<string>.Failure(result.Item1));
            }

            var logs = result.Item2.Select(u => new { u.UserId, u.Name, u.Email }).ToList();

            logger.LogInformation("Retrieved users for search term: {SearchTerm} and logs: {Logs}", searchTerm, logs);
            return Ok(ResponseResults<List<UserResponseDto>>.Success(result.Item1, result.Item2));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while searching for users with term: {SearchTerm}", searchTerm);
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }

    [HttpGet("GetUserById/{id}")]
    public async Task<IActionResult> GetUserByIdAsync(Guid id)
    {
        logger.LogTrace("GetUserById called with id: {UserId}", id);
        try
        {
            if (id == Guid.Empty)
            {
                logger.LogWarning("Invalid user id provided: {UserId}", id);
                return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
            }

            var result = await userService.GetUserById(id).ConfigureAwait(false);

            if (result.Item1 == 0)
            {
                logger.LogWarning("{Status code} id: {UserId}", result.Item1, id);
                return NotFound(ResponseResults<string>.Failure(result.Item1));
            }

            var logs = new { result.Item2?.UserId, result.Item2?.Name, result.Item2?.Email };
            logger.LogInformation("Retrieved user with id: {UserId} and logs: {Logs}", id, logs);

            return Ok(ResponseResults<UserResponseDto>.Success(result.Item1, result.Item2));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching user with id: {UserId}", id);
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }

    [HttpPost("GetUsersByFilter")]
    public async Task<IActionResult> GetUsersByFilterAsync([FromBody] UserFilterDto dto)
    {
        logger.LogTrace("GetUsersByFilter called.");
        try
        {
            var result = await userService.GetUsersByFilter(dto).ConfigureAwait(false);

            logger.LogInformation("Retrieved users by filter the count: {Count}", result.Item2?.Count ?? 0);
            return Ok(ResponseResults<List<UserResponseDto>>.Success(result.Item1, result.Item2));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching users by filter.");
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }
}
