using backend.Dto.EmployeeProjectDto;
using backend.Dto.ProjectDto;
using backend.Dto.Common;
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
    public class EmployeeProjectController(IEmployeeProjectService _employeeProjectService, ILogger<EmployeeProjectController> _logger) : ControllerBase
    {

        [HttpPost("CreateEmployeeProject")]
        public async Task<IActionResult> CreateEmployeeProject([FromBody] EmployeeProjectDto dto)
        {
            _logger.LogTrace("CreateEmployeeProject called. with dto: {@EmployeeProjectDto}", dto);
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid request body provided.");
                    return BadRequest(ResponseResults<string>.Failure(null, "Invalid request body"));
                }

                var result = await _employeeProjectService.CreateEmployeeProject(dto);

                if (result.Item1 == 0)
                {
                    _logger.LogWarning("Failed to create employee project.");
                    return BadRequest(ResponseResults<string>.Failure(null, result.Item2));
                }

                _logger.LogInformation("Employee project created successfully.");
                return Ok(ResponseResults<string>.Success(null, result.Item2));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating employee project.");
                return StatusCode(500, ResponseResults<string>.Failure(null, ex.Message));
            }
        }

        [HttpDelete("RemoveEmployeeProject/{id}")]
        public async Task<IActionResult> RemoveEmployeeProject(Guid id)
        {
            _logger.LogTrace("RemoveEmployeeProject called with id: {EmployeeProjectId}", id);
            try
            {
                if (id == Guid.Empty)
                {
                    _logger.LogWarning("Invalid employee project id provided.");
                    return BadRequest(ResponseResults<string>.Failure(null, "Invalid employee project id"));
                }

                var result = await _employeeProjectService.RemoveEmployeeProject(id);

                if (result.Item1 == 0)
                {
                    _logger.LogWarning("Failed to remove employee project.");
                    return BadRequest(ResponseResults<string>.Failure(null, result.Item2));
                }

                _logger.LogInformation("Employee project removed successfully.");
                return Ok(ResponseResults<string>.Success(null, result.Item2));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while removing employee project.");
                return StatusCode(500, ResponseResults<string>.Failure(null, ex.Message));
            }
        }

        [HttpGet("GetAllEmployeeProjects")]
        public async Task<IActionResult> GetAllEmployeeProjects()
        {
            _logger.LogTrace("GetAllEmployeeProjects called.");
            try
            {
                var result = await _employeeProjectService.GetAllEmployeeProjects();

                _logger.LogInformation("Retrieved all employee projects.");
                return Ok(ResponseResults<List<EmployeeProjectResponseDto>>.Success(result.Item2, result.Item3));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all employee projects.");
                return StatusCode(500, ResponseResults<string>.Failure(null, ex.Message));
            }
        }

        [HttpGet("GetUserProjectsByUserId/{userId}")]
        public async Task<IActionResult> GetUserProjectsByUserId(Guid userId, [FromQuery] PaginationDto dto)
        {
            _logger.LogTrace("GetUserProjectsByUserId called with id: {UserId}", userId);
            try
            {
                if (userId == Guid.Empty)
                {
                    _logger.LogWarning("Invalid user id provided: {UserId}", userId);
                    return BadRequest(ResponseResults<string>.Failure(null, "Invalid user id"));
                }

                var result = await _employeeProjectService.GetUserProjectsByUserId(userId, dto);

                if (result.Item1 == 0)
                {
                    _logger.LogWarning("User projects not found for user id: {UserId}", userId);
                    return NotFound(ResponseResults<List<ProjectResponseDto>>.Failure(null, result.Item4));
                }

                _logger.LogInformation("Retrieved projects for user with id: {UserId}", userId);
                return Ok(ResponseResults<List<ProjectResponseDto>>.Success(result.Item2, result.Item3 ?? new PaginationMetaDto(), result.Item4));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching projects for user with id: {UserId}", userId);
                return StatusCode(500, ResponseResults<string>.Failure(null, ex.Message));
            }
        }
    }
}