using backend.Dto.DepartmentDto;
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
    public class DepartmentController(IDepartmentService _departmentService, ILogger<DepartmentController> _logger) : ControllerBase
    {

        [HttpPost("CreateDepartment")]
        public async Task<IActionResult> CreateDepartment([FromBody] DepartmentDto dto)
        {
            _logger.LogTrace("CreateDepartment called with dto: {@DepartmentName}", dto.Name);
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid request body for CreateDepartment: {@ModelState}", ModelState);
                    return BadRequest(ResponseResults<string>.Failure(null, "Invalid request body"));
                }

                var result = await _departmentService.CreateDepartment(dto);

                if (result.Item1 == 0)
                {
                    _logger.LogWarning("Failed to create department. Reason: {Reason}", result.Item2);
                    return BadRequest(ResponseResults<string>.Failure(null, result.Item2));
                }

                _logger.LogInformation("CreateDepartment called with dto: {@DepartmentDto}", dto);
                return Ok(ResponseResults<string>.Success(null, result.Item2));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a department.");
                return StatusCode(500, ResponseResults<string>.Failure(null, ex.Message));
            }

        }

        [HttpPut("UpdateDepartment")]
        public async Task<IActionResult> UpdateDepartment([FromBody] DepartmentDto dto)
        {
            _logger.LogTrace("UpdateDepartment called with dto: {@DepartmentName}", dto.Name);
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid request body for UpdateDepartment: {@ModelState}", ModelState);
                    return BadRequest(ResponseResults<string>.Failure(null, "Invalid request body"));
                }

                var result = await _departmentService.UpdateDepartment(dto);

                if (result.Item1 == 0)
                {
                    _logger.LogWarning("Failed to update department. Reason: {Reason}", result.Item2);
                    return BadRequest(ResponseResults<string>.Failure(null, result.Item2));
                }

                _logger.LogInformation("UpdateDepartment called with dto: {@DepartmentDto}", dto);
                return Ok(
                    ResponseResults<string>.Success(null, result.Item2));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating a department.");
                return StatusCode(500, ResponseResults<string>.Failure(null, ex.Message));
            }
        }

        [HttpGet("GetAllDepartments")]
        public async Task<IActionResult> GetAllDepartments()
        {
            _logger.LogTrace("GetAllDepartments called");
            try
            {

                var result = await _departmentService.GetAllDepartments();

                _logger.LogInformation("Retrieved all departments successfully. Count: {Count}", result.Item2.Count);
                return Ok(ResponseResults<List<DepartmentResponseDto>>.Success(result.Item2, result.Item3));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all departments.");
                return StatusCode(500, ResponseResults<string>.Failure(null, ex.Message));
            }
        }

        [HttpGet("GetDepartmentById/{id}")]
        public async Task<IActionResult> GetDepartmentById(Guid id)
        {
            _logger.LogTrace("GetDepartmentById called with id: {DepartmentId}", id);
            try
            {
                if (id == Guid.Empty)
                {
                    _logger.LogWarning("Invalid department id provided: {DepartmentId}", id);
                    return BadRequest(ResponseResults<string>.Failure(null, "Invalid department id"));
                }

                var result = await _departmentService.GetDepartmentById(id);

                if (result.Item1 == 0)
                {
                    _logger.LogWarning("Department not found with id: {DepartmentId}", id);
                    return NotFound(ResponseResults<string>.Failure(null, result.Item3));
                }

                _logger.LogInformation("Retrieved department with id: {DepartmentId}", id);
                return Ok(ResponseResults<DepartmentResponseDto>.Success(result.Item2, result.Item3));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving department with id: {DepartmentId}", id);
                return StatusCode(500, ResponseResults<string>.Failure(null, ex.Message));
            }
        }

        [HttpGet("GetDepartmentEmployees/{departmentId}")]
        public async Task<IActionResult> GetDepartmentEmployees(Guid departmentId)
        {
            _logger.LogTrace("GetDepartmentEmployees called with id: {DepartmentId}", departmentId);
            try
            {
                if (departmentId == Guid.Empty)
                {
                    _logger.LogWarning("Invalid department id provided: {DepartmentId}", departmentId);
                    return BadRequest(ResponseResults<string>.Failure(null, "Invalid department id"));
                }

                var result = await _departmentService.GetDepartmentEmployees(departmentId);

                if (result.Item1 == 0)
                {
                    _logger.LogWarning("No employees found for department id: {DepartmentId}", departmentId);
                    return NotFound(ResponseResults<List<DepartmentUserResponseDto>>.Failure(null, result.Item3));
                }

                _logger.LogInformation("Retrieved employees for department id: {DepartmentId}", departmentId);
                return Ok(ResponseResults<List<DepartmentUserResponseDto>>.Success(result.Item2, result.Item3));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving employees for department id: {DepartmentId}", departmentId);
                return StatusCode(500, ResponseResults<string>.Failure(null, ex.Message));
            }
        }
    }
}