using backend.Dto.ProjectDtos;
using backend.GenericResponse;
using backend.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Authorization;

namespace backend.Controllers;

[Authorize(Roles = RoleConstants.Admin + "," + RoleConstants.Manager)]
[ApiController]
[Route("api/[controller]")]
public class ProjectController(IProjectService projectService, ILogger<ProjectController> logger) : ControllerBase
{
    [HttpPost("CreateProject")]
    public async Task<IActionResult> CreateProjectAsync([FromBody] ProjectDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        logger.LogTrace("CreateProject called with dto: {ProjectName}", dto.Name);
        try
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning("Invalid request body provided.");
                return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
            }

            var result = await projectService.CreateProject(dto).ConfigureAwait(false);

            if (result.Item1 == 0)
            {
                logger.LogWarning("StatusCode: {StatusCode}", result.Item1);
                return BadRequest(ResponseResults<string>.Failure(result.Item1));
            }

            logger.LogInformation("Project created successfully name: {ProjectName}", dto.Name);
            return Ok(ResponseResults<string>.Success(result.Item1));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating project.");
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }

    [HttpPut("UpdateProject")]
    public async Task<IActionResult> UpdateProjectAsync([FromBody] ProjectDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        logger.LogTrace("UpdateProject called with dto: {ProjectName}", dto.Name);
        try
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning("Invalid request body provided.");
                return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
            }

            var result = await projectService.UpdateProject(dto).ConfigureAwait(false);

            if (result.Item1 == 0)
            {
                logger.LogWarning("StatusCode: {StatusCode}", result.Item1);
                return BadRequest(ResponseResults<string>.Failure(result.Item1));
            }

            logger.LogInformation("Project updated successfully name: {ProjectName}", dto.Name);
            return Ok(ResponseResults<string>.Success(result.Item1));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while updating project.");
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }

    [HttpGet("GetAllProjects")]
    public async Task<IActionResult> GetAllProjectsAsync()
    {
        logger.LogTrace("GetAllProjects called.");
        try
        {
            var result = await projectService.GetAllProjects().ConfigureAwait(false);

            if (result.Item2 == null || result.Item2.Count == 0)
            {
                logger.LogWarning(" StatusCode : {StatusCode}", result.Item1);
                return NotFound(ResponseResults<IReadOnlyCollection<ProjectResponseDto>>.Failure(result.Item1));
            }

            logger.LogInformation("Retrieved all projects count: {Count}", result.Item2?.Count ?? 0);
            return Ok(ResponseResults<IReadOnlyCollection<ProjectResponseDto>>.Success(result.Item1, result.Item2));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching all projects.");
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }

    [HttpGet("GetProjectById/{id}")]
    public async Task<IActionResult> GetProjectByIdAsync(Guid id)
    {
        logger.LogTrace("GetProjectById called with id: {ProjectId}", id);
        try
        {
            if (id == Guid.Empty)
            {
                logger.LogWarning("Invalid project id provided: {ProjectId}", id);
                return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
            }

            var result = await projectService.GetProjectById(id).ConfigureAwait(false);

            if (result.Item1 == 0)
            {
                logger.LogWarning("{StatusCode} id: {ProjectId}", result.Item1, id);
                return NotFound(ResponseResults<string>.Failure(result.Item1));
            }

            logger.LogInformation("Retrieved project with id: {ProjectId} name: {ProjectName}", id, result.Item2?.Name);
            return Ok(ResponseResults<ProjectResponseDto>.Success(result.Item1, result.Item2));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching project with id: {ProjectId}", id);
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }

    [HttpGet("GetProjectEmployees/{projectId}")]
    public async Task<IActionResult> GetProjectEmployeesAsync(Guid projectId)
    {
        logger.LogTrace("GetProjectEmployees called with id: {ProjectId}", projectId);
        try
        {
            if (projectId == Guid.Empty)
            {
                logger.LogWarning("Invalid project id provided: {ProjectId}", projectId);
                return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
            }

            var result = await projectService.GetProjectEmployees(projectId).ConfigureAwait(false);

            if (result.Item1 == 0)
            {
                logger.LogWarning("{StatusCode} id: {ProjectId}", result.Item1, projectId);
                return NotFound(ResponseResults<IReadOnlyCollection<ProjectUserResponseDto>>.Failure(result.Item1));
            }

            logger.LogInformation("Retrieved employees for project with id: {ProjectId} count: {Count}", projectId, result.Item2?.Count ?? 0);
            return Ok(ResponseResults<IReadOnlyCollection<ProjectUserResponseDto>>.Success(result.Item1, result.Item2));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching employees for project with id: {ProjectId}", projectId);
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }
}
