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
    public async Task<IActionResult> CreateEmployeeProjectAsync([FromBody] EmployeeProjectDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);

        logger.LogTrace("CreateEmployeeProject called. with dto: {@EmployeeProjectDto}", dto);

        var result = await employeeProjectService.CreateEmployeeProject(dto, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Failed to create employee project.");
            return BadRequest(ResponseResults<string>.Failure(result.StatusCode));
        }

        logger.LogInformation("Employee project created successfully.");
        return Ok(ResponseResults<string>.Success(result.StatusCode));

    }

    [HttpDelete("RemoveEmployeeProject/{id}")]
    public async Task<IActionResult> RemoveEmployeeProjectAsync(Guid id, CancellationToken cancellationToken)
    {
        logger.LogTrace("RemoveEmployeeProject called with id: {EmployeeProjectId}", id);

        if (id == Guid.Empty)
        {
            logger.LogWarning("Invalid employee project id provided.");
            return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
        }

        var result = await employeeProjectService.RemoveEmployeeProject(id, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Failed to remove employee project.");
            return BadRequest(ResponseResults<string>.Failure(result.StatusCode));
        }

        logger.LogInformation("Employee project removed successfully.");
        return Ok(ResponseResults<string>.Success(result.StatusCode));
    }

    [HttpGet("GetAllEmployeeProjects")]
    public async Task<IActionResult> GetAllEmployeeProjectsAsync(CancellationToken cancellationToken)
    {
        logger.LogTrace("GetAllEmployeeProjects called.");

        var result = await employeeProjectService.GetAllEmployeeProjects(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Retrieved all employee projects.");
        return Ok(ResponseResults<IReadOnlyCollection<EmployeeProjectResponseDto>>.Success(result.StatusCode, result.Data));
    }

    [HttpGet("GetUserProjectsByUserId/{userId}")]
    public async Task<IActionResult> GetUserProjectsByUserIdAsync(Guid userId, [FromQuery] PaginationDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        logger.LogTrace("GetUserProjectsByUserId called with id: {UserId}", userId);

        if (userId == Guid.Empty)
        {
            logger.LogWarning("Invalid user id provided: {UserId}", userId);
            return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
        }

        var result = await employeeProjectService.GetUserProjectsByUserId(userId, dto, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            logger.LogWarning("User projects not found for user id: {UserId}", userId);
            return NotFound(ResponseResults<IReadOnlyCollection<ProjectResponseDto>>.Failure(result.StatusCode));
        }

        logger.LogInformation("Retrieved projects for user with id: {UserId}", userId);
        return Ok(ResponseResults<IReadOnlyCollection<ProjectResponseDto>>.Success(result.StatusCode, result.Data, result.Meta));
    }
}
