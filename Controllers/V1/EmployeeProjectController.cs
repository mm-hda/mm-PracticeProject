using backend.Dto.EmployeeProjectDtos;
using backend.Dto.ProjectDtos;
using backend.Dto.CommonDtos;
using backend.GenericResponse;
using backend.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Authorization;

namespace backend.Controllers.V1;

[Authorize(Roles = RoleConstants.Admin + "," + RoleConstants.HR)]
[ApiController]
[Route("api/[controller]")]
public class EmployeeProjectController(IEmployeeProjectService employeeProjectService, ILogger<EmployeeProjectController> logger) : ControllerBase
{

    [HttpPost("CreateEmployeeProject")]
    public async Task<IActionResult> CreateEmployeeProjectAsync([FromBody] EmployeeProjectDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        logger.LogTrace("CreateEmployeeProject called. with dto: {@EmployeeProjectDto}", dto);
        try
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning("Invalid request body provided.");
                return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
            }

            var result = await employeeProjectService.CreateEmployeeProject(dto).ConfigureAwait(false);

            if (result.Item1 == 0)
            {
                logger.LogWarning("Failed to create employee project.");
                return BadRequest(ResponseResults<string>.Failure(result.Item1));
            }

            logger.LogInformation("Employee project created successfully.");
            return Ok(ResponseResults<string>.Success(result.Item1));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating employee project.");
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }

    [HttpDelete("RemoveEmployeeProject/{id}")]
    public async Task<IActionResult> RemoveEmployeeProjectAsync(Guid id)
    {
        logger.LogTrace("RemoveEmployeeProject called with id: {EmployeeProjectId}", id);
        try
        {
            if (id == Guid.Empty)
            {
                logger.LogWarning("Invalid employee project id provided.");
                return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
            }

            var result = await employeeProjectService.RemoveEmployeeProject(id).ConfigureAwait(false);

            if (result.Item1 == 0)
            {
                logger.LogWarning("Failed to remove employee project.");
                return BadRequest(ResponseResults<string>.Failure(result.Item1));
            }

            logger.LogInformation("Employee project removed successfully.");
            return Ok(ResponseResults<string>.Success(result.Item1));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while removing employee project.");
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }

    [HttpGet("GetAllEmployeeProjects")]
    public async Task<IActionResult> GetAllEmployeeProjectsAsync()
    {
        logger.LogTrace("GetAllEmployeeProjects called.");
        try
        {
            var result = await employeeProjectService.GetAllEmployeeProjects().ConfigureAwait(false);

            logger.LogInformation("Retrieved all employee projects.");
            return Ok(ResponseResults<IReadOnlyCollection<EmployeeProjectResponseDto>>.Success(result.Item1, result.Item2));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching all employee projects.");
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }

    [HttpGet("GetUserProjectsByUserId/{userId}")]
    public async Task<IActionResult> GetUserProjectsByUserIdAsync(Guid userId, [FromQuery] PaginationDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        logger.LogTrace("GetUserProjectsByUserId called with id: {UserId}", userId);
        try
        {
            if (userId == Guid.Empty)
            {
                logger.LogWarning("Invalid user id provided: {UserId}", userId);
                return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
            }

            var result = await employeeProjectService.GetUserProjectsByUserId(userId, dto).ConfigureAwait(false);

            if (result.Item1 == 0)
            {
                logger.LogWarning("User projects not found for user id: {UserId}", userId);
                return NotFound(ResponseResults<IReadOnlyCollection<ProjectResponseDto>>.Failure(result.Item1));
            }

            logger.LogInformation("Retrieved projects for user with id: {UserId}", userId);
            return Ok(ResponseResults<IReadOnlyCollection<ProjectResponseDto>>.Success(result.Item1, result.Item2, result.Item3));

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching projects for user with id: {UserId}", userId);
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }
}
