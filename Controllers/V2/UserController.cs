using backend.Dto.UserDtos;
using backend.Dto.CommonDtos;
using backend.GenericResponse;
using backend.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Authorization;
using Asp.Versioning;

namespace backend.Controllers.V2;

[Authorize(Roles = RoleConstants.Admin)]
[ApiController]
[Route("api/[controller]")]
public class UserController(IUserService userService, ILogger<UserController> logger) : ControllerBase
{

    [ApiVersion("2.0")]
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

            UserResponseDtoV2 userResponseV2 = new()
            {
                UserId = result.Item2?.UserId ?? Guid.Empty,
                Name = result.Item2?.Name,
                Email = result.Item2?.Email
            };

            var logs = new { userResponseV2.UserId, userResponseV2.Name, userResponseV2.Email };
            logger.LogInformation("Retrieved user with id: {UserId} and logs: {Logs}", id, logs);

            return Ok(ResponseResults<UserResponseDtoV2>.Success(result.Item1, userResponseV2));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching user with id: {UserId}", id);
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }
}
