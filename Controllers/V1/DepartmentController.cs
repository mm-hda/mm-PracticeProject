using backend.Dto.DepartmentDtos;
using backend.GenericResponse;
using backend.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Authorization;

namespace backend.Controllers.V1;

// [Authorize(Roles = RoleConstants.Admin + "," + RoleConstants.HR)]
[ApiController]
[Route("api/[controller]")]
public class DepartmentController(IDepartmentService departmentService, ILogger<DepartmentController> logger) : ControllerBase
{

    [HttpPost("CreateDepartment")]
    public async Task<IActionResult> CreateDepartmentAsync([FromBody] DepartmentDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        logger.LogTrace("CreateDepartment called with dto: {@DepartmentName}", dto.Name);

        var result = await departmentService.CreateDepartment(dto, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Failed to create department. Reason: {StatusCode}", result.StatusCode);
            return BadRequest(ResponseResults<string>.Failure(result.StatusCode));
        }

        logger.LogInformation("CreateDepartment called with dto: {@DepartmentDto}", dto);
        return Ok(ResponseResults<string>.Success(result.StatusCode));

    }

    [HttpPut("UpdateDepartment")]
    public async Task<IActionResult> UpdateDepartmentAsync([FromBody] DepartmentDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        logger.LogTrace("UpdateDepartment called with dto: {@DepartmentName}", dto.Name);

        var result = await departmentService.UpdateDepartment(dto, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Failed to update department. Reason: {StatusCode}", result.StatusCode);
            return BadRequest(ResponseResults<string>.Failure(result.StatusCode));
        }

        logger.LogInformation("UpdateDepartment called with dto: {@DepartmentDto}", dto);
        return Ok(ResponseResults<string>.Success(result.StatusCode));

    }

    [HttpGet("GetAllDepartments")]
    public async Task<IActionResult> GetAllDepartmentsAsync(CancellationToken cancellationToken)
    {
        logger.LogTrace("GetAllDepartments called");

        var result = await departmentService.GetAllDepartments(cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Failed to retrieve departments. Status code: {StatusCode}", result.StatusCode);
            return NotFound(ResponseResults<string>.Failure(result.StatusCode));
        }

        logger.LogInformation("Retrieved all departments successfully. Count: {Count}", result.Data?.Count ?? 0);
        return Ok(ResponseResults<IReadOnlyCollection<DepartmentResponseDto>>.Success(result.StatusCode, result.Data));

    }

    [HttpGet("GetDepartmentById/{id}")]
    public async Task<IActionResult> GetDepartmentByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        logger.LogTrace("GetDepartmentById called with id: {DepartmentId}", id);

        if (id == Guid.Empty)
        {
            logger.LogWarning("Invalid department id provided: {DepartmentId}", id);
            return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
        }

        var result = await departmentService.GetDepartmentById(id, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Department not found with id: {DepartmentId}", id);
            return NotFound(ResponseResults<string>.Failure(result.StatusCode));
        }

        logger.LogInformation("Retrieved department with id: {DepartmentId}", id);
        return Ok(ResponseResults<DepartmentResponseDto>.Success(result.StatusCode, result.Data));

    }

    [HttpGet("GetDepartmentEmployees/{departmentId}")]
    public async Task<IActionResult> GetDepartmentEmployeesAsync(Guid departmentId, CancellationToken cancellationToken)
    {
        logger.LogTrace("GetDepartmentEmployees called with id: {DepartmentId}", departmentId);

        if (departmentId == Guid.Empty)
        {
            logger.LogWarning("Invalid department id provided: {DepartmentId}", departmentId);
            return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
        }

        var result = await departmentService.GetDepartmentEmployees(departmentId, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Failed to retrieve employees for department id: {DepartmentId}", departmentId);
            return NotFound(ResponseResults<IReadOnlyCollection<DepartmentUserResponseDto>>.Failure(result.StatusCode));
        }

        logger.LogInformation("Retrieved employees for department id: {DepartmentId}", departmentId);
        return Ok(ResponseResults<IReadOnlyCollection<DepartmentUserResponseDto>>.Success(result.StatusCode, result.Data));

    }
}
