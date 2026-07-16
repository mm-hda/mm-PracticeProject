using backend.Dto.ProjectDto;
using backend.GenericResponse;
using backend.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Authorization;

namespace backend.Controllers
{
    [Authorize(Roles = RoleConstants.Admin + "," + RoleConstants.Manager)]
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectController(IProjectService _projectService, ILogger<ProjectController> _logger) : ControllerBase
    {
        [HttpPost("CreateProject")]
        public async Task<IActionResult> CreateProject([FromBody] ProjectDto dto)
        {
            _logger.LogTrace("CreateProject called with dto: {ProjectName}", dto.Name);
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid request body provided.");
                    return BadRequest(ResponseResults<string>.Failure(null, "Invalid request body"));
                }

                var result = await _projectService.CreateProject(dto);

                if (result.Item1 == 0)
                {
                    _logger.LogWarning("{Message}", result.Item2);
                    return BadRequest(ResponseResults<string>.Failure(null, result.Item2));
                }

                _logger.LogInformation("Project created successfully name: {ProjectName}", dto.Name);
                return Ok(ResponseResults<string>.Success(null, result.Item2));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating project.");
                return StatusCode(500, ResponseResults<string>.Failure(null, ex.Message));
            }
        }

        [HttpPut("UpdateProject")]
        public async Task<IActionResult> UpdateProject([FromBody] ProjectDto dto)
        {
            _logger.LogTrace("UpdateProject called with dto: {ProjectName}", dto.Name);
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid request body provided.");
                    return BadRequest(ResponseResults<string>.Failure(null, "Invalid request body"));
                }

                var result = await _projectService.UpdateProject(dto);

                if (result.Item1 == 0)
                {
                    _logger.LogWarning("{Message}", result.Item2);
                    return BadRequest(ResponseResults<string>.Failure(null, result.Item2));
                }

                _logger.LogInformation("Project updated successfully name: {ProjectName}", dto.Name);
                return Ok(ResponseResults<string>.Success(null, result.Item2));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating project.");
                return StatusCode(500, ResponseResults<string>.Failure(null, ex.Message));
            }
        }

        [HttpGet("GetAllProjects")]
        public async Task<IActionResult> GetAllProjects()
        {
            _logger.LogTrace("GetAllProjects called.");
            try
            {
                var result = await _projectService.GetAllProjects();

                if (!result.Item2.Any())
                {
                    _logger.LogWarning("No projects found.");
                    return NotFound(ResponseResults<List<ProjectResponseDto>>.Failure(null, "No projects found"));
                }

                _logger.LogInformation("Retrieved all projects count: {Count}", result.Item2?.Count ?? 0);
                return Ok(ResponseResults<List<ProjectResponseDto>>.Success(result.Item2, result.Item3));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all projects.");
                return StatusCode(500, ResponseResults<string>.Failure(null, ex.Message));
            }
        }

        [HttpGet("GetProjectById/{id}")]
        public async Task<IActionResult> GetProjectById(Guid id)
        {
            _logger.LogTrace("GetProjectById called with id: {ProjectId}", id);
            try
            {
                if (id == Guid.Empty)
                {
                    _logger.LogWarning("Invalid project id provided: {ProjectId}", id);
                    return BadRequest(ResponseResults<string>.Failure(null, "Invalid project id"));
                }

                var result = await _projectService.GetProjectById(id);

                if (result.Item1 == 0)
                {
                    _logger.LogWarning("{Message} id: {ProjectId}", result.Item3, id);
                    return NotFound(ResponseResults<string>.Failure(null, result.Item3));
                }

                _logger.LogInformation("Retrieved project with id: {ProjectId} name: {ProjectName}", id, result.Item2?.Name);
                return Ok(ResponseResults<ProjectResponseDto>.Success(result.Item2, result.Item3));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching project with id: {ProjectId}", id);
                return StatusCode(500, ResponseResults<string>.Failure(null, ex.Message));
            }
        }

        [HttpGet("GetProjectEmployees/{projectId}")]
        public async Task<IActionResult> GetProjectEmployees(Guid projectId)
        {
            _logger.LogTrace("GetProjectEmployees called with id: {ProjectId}", projectId);
            try
            {
                if (projectId == Guid.Empty)
                {
                    _logger.LogWarning("Invalid project id provided: {ProjectId}", projectId);
                    return BadRequest(ResponseResults<string>.Failure(null, "Invalid project id"));
                }

                var result = await _projectService.GetProjectEmployees(projectId);

                if (result.Item1 == 0)
                {
                    _logger.LogWarning("{Message} id: {ProjectId}", result.Item3, projectId);
                    return NotFound(ResponseResults<List<ProjectUserResponseDto>>.Failure(null, result.Item3));
                }

                _logger.LogInformation("Retrieved employees for project with id: {ProjectId} count: {Count}", projectId, result.Item2?.Count ?? 0);
                return Ok(ResponseResults<List<ProjectUserResponseDto>>.Success(result.Item2, result.Item3));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching employees for project with id: {ProjectId}", projectId);
                return StatusCode(500, ResponseResults<string>.Failure(null, ex.Message));
            }
        }
    }
}