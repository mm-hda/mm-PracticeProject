using backend.Dto.DepartmentDtos;
using backend.GenericResponse;
using backend.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Authorization;

namespace backend.Controllers.V1;

[Authorize(Roles = RoleConstants.Admin + "," + RoleConstants.HR)]
[ApiController]
[Route("api/[controller]")]
public class DepartmentController(IDepartmentService departmentService, ILogger<DepartmentController> logger) : ControllerBase
{

    [HttpPost("CreateDepartment")]
    public async Task<IActionResult> CreateDepartmentAsync([FromBody] DepartmentDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        logger.LogTrace("CreateDepartment called with dto: {@DepartmentName}", dto.Name);
        try
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning("Invalid request body for CreateDepartment: {@ModelState}", ModelState);
                return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
            }

            var result = await departmentService.CreateDepartment(dto).ConfigureAwait(false);

            if (result.Item1 == 0)
            {
                logger.LogWarning("Failed to create department. Reason: {StatusCode}", result.Item1);
                return BadRequest(ResponseResults<string>.Failure(result.Item1));
            }

            logger.LogInformation("CreateDepartment called with dto: {@DepartmentDto}", dto);
            return Ok(ResponseResults<string>.Success(result.Item1));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating a department.");
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }

    [HttpPut("UpdateDepartment")]
    public async Task<IActionResult> UpdateDepartmentAsync([FromBody] DepartmentDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        logger.LogTrace("UpdateDepartment called with dto: {@DepartmentName}", dto.Name);
        try
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning("Invalid request body for UpdateDepartment: {@ModelState}", ModelState);
                return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
            }

            var result = await departmentService.UpdateDepartment(dto).ConfigureAwait(false);

            if (result.Item1 == 0)
            {
                logger.LogWarning("Failed to update department. Reason: {StatusCode}", result.Item1);
                return BadRequest(ResponseResults<string>.Failure(result.Item1));
            }

            logger.LogInformation("UpdateDepartment called with dto: {@DepartmentDto}", dto);
            return Ok(ResponseResults<string>.Success(result.Item1));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while updating a department.");
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }

    [HttpGet("GetAllDepartments")]
    public async Task<IActionResult> GetAllDepartmentsAsync()
    {
        logger.LogTrace("GetAllDepartments called");
        try
        {

            var result = await departmentService.GetAllDepartments().ConfigureAwait(false);

            logger.LogInformation("Retrieved all departments successfully. Count: {Count}", result.Item2.Count);
            return Ok(ResponseResults<IReadOnlyCollection<DepartmentResponseDto>>.Success(result.Item1, result.Item2));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving all departments.");
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }

    [HttpGet("GetDepartmentById/{id}")]
    public async Task<IActionResult> GetDepartmentByIdAsync(Guid id)
    {
        logger.LogTrace("GetDepartmentById called with id: {DepartmentId}", id);
        try
        {
            if (id == Guid.Empty)
            {
                logger.LogWarning("Invalid department id provided: {DepartmentId}", id);
                return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
            }

            var result = await departmentService.GetDepartmentById(id).ConfigureAwait(false);

            if (result.Item1 == 0)
            {
                logger.LogWarning("Department not found with id: {DepartmentId}", id);
                return NotFound(ResponseResults<string>.Failure(result.Item1));
            }

            logger.LogInformation("Retrieved department with id: {DepartmentId}", id);
            return Ok(ResponseResults<DepartmentResponseDto>.Success(result.Item1, result.Item2));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving department with id: {DepartmentId}", id);
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }

    [HttpGet("GetDepartmentEmployees/{departmentId}")]
    public async Task<IActionResult> GetDepartmentEmployeesAsync(Guid departmentId)
    {
        logger.LogTrace("GetDepartmentEmployees called with id: {DepartmentId}", departmentId);
        try
        {
            if (departmentId == Guid.Empty)
            {
                logger.LogWarning("Invalid department id provided: {DepartmentId}", departmentId);
                return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
            }

            var result = await departmentService.GetDepartmentEmployees(departmentId).ConfigureAwait(false);

            if (result.Item1 == 0)
            {
                logger.LogWarning("No employees found for department id: {DepartmentId}", departmentId);
                return NotFound(ResponseResults<IReadOnlyCollection<DepartmentUserResponseDto>>.Failure(result.Item1));
            }

            logger.LogInformation("Retrieved employees for department id: {DepartmentId}", departmentId);
            return Ok(ResponseResults<IReadOnlyCollection<DepartmentUserResponseDto>>.Success(result.Item1, result.Item2));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving employees for department id: {DepartmentId}", departmentId);
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }
}
