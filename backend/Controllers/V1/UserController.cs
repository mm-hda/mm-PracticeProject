using backend.Dto;
using backend.Dto.UserDtos;
using backend.Dto.CommonDtos;
using backend.GenericResponse;
using backend.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Authorization;
using Asp.Versioning;

namespace backend.Controllers.V1;

[Authorize(Roles = RoleConstants.Admin)]
[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
public class UserController(IUserService userService, ILogger<UserController> logger) : ControllerBase
{

    [HttpGet("GetAllUsers")]
    public async Task<IActionResult> GetAllUsersAsync([FromQuery] PaginationDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);

        logger.LogTrace("GetAllUsers called. Page:{PageNumber}, PageSize:{PageSize}", dto.PageNumber, dto.PageSize);

        var result = await userService.GetAllUsers(dto, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            logger.LogWarning("{Status code}", result.StatusCode);
            return NotFound(ResponseResults<List<UserResponseDto>>.Failure(result.StatusCode));
        }

        logger.LogInformation("Retrieved all users successfully. Count: {COUNT}", result.Data?.Count ?? 0);
        return Ok(ResponseResults<IReadOnlyCollection<UserResponseDto>>.Success(result.StatusCode, result.Data, result.Meta));
    }

    [HttpGet("GetUserBySearch")]
    public async Task<IActionResult> GetUserBySearchAsync([FromQuery] string searchTerm, CancellationToken cancellationToken)
    {
        logger.LogTrace("GetUserBySearch called with searchTerm: {SearchTerm}", searchTerm);

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            logger.LogWarning("Invalid search term provided: {SearchTerm}", searchTerm);
            return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
        }

        var result = await userService.GetUserBySearch(searchTerm, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            logger.LogWarning("{Status code} search term: {SearchTerm}", result.StatusCode, searchTerm);
            return NotFound(ResponseResults<string>.Failure(result.StatusCode));
        }

        var logs = result.Data?.Select(u => new { u.UserId, u.Name, u.Email });
        logger.LogInformation("Retrieved users for search term: {SearchTerm} and logs: {Logs}", searchTerm, logs);
        return Ok(ResponseResults<IReadOnlyCollection<UserResponseDto>>.Success(result.StatusCode, result.Data, result.Meta));
    }

    [HttpGet("GetUserById/{id}")]
    public async Task<IActionResult> GetUserByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        logger.LogTrace("GetUserById called with id: {UserId}", id);

        if (id == Guid.Empty)
        {
            logger.LogWarning("Invalid user id provided: {UserId}", id);
            return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
        }

        var result = await userService.GetUserById(id, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            logger.LogWarning("{Status code} id: {UserId}", result.StatusCode, id);
            return NotFound(ResponseResults<string>.Failure(result.StatusCode));
        }

        var logs = new { result.Data?.UserId, result.Data?.Name, result.Data?.Email };
        logger.LogInformation("Retrieved user with id: {UserId} and logs: {Logs}", id, logs);

        return Ok(ResponseResults<UserResponseDto>.Success(result.StatusCode, result.Data));

    }

    [HttpPost("GetUsersByFilter")]
    public async Task<IActionResult> GetUsersByFilterAsync([FromBody] UserFilterDto dto, CancellationToken cancellationToken)
    {
        logger.LogTrace("GetUsersByFilter called.");

        var result = await userService.GetUsersByFilter(dto, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            logger.LogWarning("{Status code}", result.StatusCode);
            return NotFound(ResponseResults<IReadOnlyCollection<UserResponseDto>>.Failure(result.StatusCode));
        }

        logger.LogInformation("Retrieved users by filter the count: {Count}", result.Data?.Count ?? 0);
        return Ok(ResponseResults<IReadOnlyCollection<UserResponseDto>>.Success(result.StatusCode, result.Data, result.Meta));

    }

    [HttpGet("GetManagers")]
    public async Task<IActionResult> GetManagersAsync(CancellationToken cancellationToken)
    {
        logger.LogTrace("GetManagers called.");

        var result = await userService.GetManagers(cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            logger.LogWarning("{Status code}", result.StatusCode);
            return NotFound(ResponseResults<IReadOnlyCollection<UserResponseDto>>.Failure(result.StatusCode));
        }

        logger.LogInformation("Retrieved managers successfully. Count: {Count}", result.Data?.Count ?? 0);
        return Ok(ResponseResults<IReadOnlyCollection<UserResponseDto>>.Success(result.StatusCode, result.Data, result.Meta));
    }

    [HttpPut("UpdateUser/{id}")]
    public async Task<IActionResult> UpdateUserAsync(Guid id, [FromBody] RegisterUserDtoV2 dto, CancellationToken cancellationToken)
    {
        logger.LogTrace("UpdateUser called with id: {UserId}", id);

        if (id == Guid.Empty || dto == null)
        {
            logger.LogWarning("Invalid input provided. UserId: {UserId}, DTO: {DTO}", id, dto);
            return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
        }

        var result = await userService.UpdateUser(id, dto, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            logger.LogWarning("{Status code} id: {UserId}", result.StatusCode, id);
            return NotFound(ResponseResults<string>.Failure(result.StatusCode));
        }

        logger.LogInformation("Updated user successfully with id: {UserId}", id);
        return Ok(ResponseResults<string>.Success(result.StatusCode, "User updated successfully."));
    }
}
