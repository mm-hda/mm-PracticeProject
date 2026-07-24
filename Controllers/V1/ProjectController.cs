using backend.Dto.ProjectDtos;
using backend.GenericResponse;
using backend.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Authorization;

namespace backend.Controllers.V1;

[Authorize(Roles = RoleConstants.Admin + "," + RoleConstants.Manager)]
[ApiController]
[Route("api/[controller]")]
public class ProjectController(IProjectService projectService, ILogger<ProjectController> logger) : ControllerBase
{
    [HttpPost("CreateProject")]
    public async Task<IActionResult> CreateProjectAsync([FromBody] ProjectDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        logger.LogTrace("CreateProject called with dto: {ProjectName}", dto.Name);

        var result = await projectService.CreateProject(dto, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            logger.LogWarning("StatusCode: {StatusCode}", result.StatusCode);
            return BadRequest(ResponseResults<string>.Failure(result.StatusCode));
        }

        logger.LogInformation("Project created successfully name: {ProjectName}", dto.Name);
        return Ok(ResponseResults<string>.Success(result.StatusCode));

    }

    [HttpPut("UpdateProject")]
    public async Task<IActionResult> UpdateProjectAsync([FromBody] ProjectDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);

        logger.LogTrace("UpdateProject called with dto: {ProjectName}", dto.Name);

        var result = await projectService.UpdateProject(dto, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            logger.LogWarning("StatusCode: {StatusCode}", result.StatusCode);
            return BadRequest(ResponseResults<string>.Failure(result.StatusCode));
        }

        logger.LogInformation("Project updated successfully name: {ProjectName}", dto.Name);
        return Ok(ResponseResults<string>.Success(result.StatusCode));
    }

    [HttpGet("GetAllProjects")]
    public async Task<IActionResult> GetAllProjectsAsync()
    {
        logger.LogTrace("GetAllProjects called.");

        var result = await projectService.GetAllProjects().ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            logger.LogWarning("StatusCode: {StatusCode}", result.StatusCode);
            return NotFound(ResponseResults<IReadOnlyCollection<ProjectResponseDto>>.Failure(result.StatusCode));
        }

        logger.LogInformation("Retrieved all projects count: {Count}", result.Data?.Count ?? 0);
        return Ok(ResponseResults<IReadOnlyCollection<ProjectResponseDto>>.Success(result.StatusCode, result.Data));
    }

    [HttpGet("GetProjectById/{id}")]
    public async Task<IActionResult> GetProjectByIdAsync(Guid id)
    {
        logger.LogTrace("GetProjectById called with id: {ProjectId}", id);

        if (id == Guid.Empty)
        {
            logger.LogWarning("Invalid project id provided: {ProjectId}", id);
            return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
        }

        var result = await projectService.GetProjectById(id).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            logger.LogWarning("StatusCode: {StatusCode}", result.StatusCode);
            return NotFound(ResponseResults<string>.Failure(result.StatusCode));
        }

        logger.LogInformation("Retrieved project with id: {ProjectId} name: {ProjectName}", id, result.Data?.Name);
        return Ok(ResponseResults<ProjectResponseDto>.Success(result.StatusCode, result.Data));
    }

    [HttpGet("GetProjectEmployees/{projectId}")]
    public async Task<IActionResult> GetProjectEmployeesAsync(Guid projectId)
    {
        logger.LogTrace("GetProjectEmployees called with id: {ProjectId}", projectId);

        if (projectId == Guid.Empty)
        {
            logger.LogWarning("Invalid project id provided: {ProjectId}", projectId);
            return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
        }

        var result = await projectService.GetProjectEmployees(projectId).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            logger.LogWarning("StatusCode: {StatusCode}", result.StatusCode);
            return NotFound(ResponseResults<IReadOnlyCollection<ProjectUserResponseDto>>.Failure(result.StatusCode));
        }

        logger.LogInformation("Retrieved employees for project with id: {ProjectId} count: {Count}", projectId, result.Data?.Count ?? 0);
        return Ok(ResponseResults<IReadOnlyCollection<ProjectUserResponseDto>>.Success(result.StatusCode, result.Data));
    }
}
