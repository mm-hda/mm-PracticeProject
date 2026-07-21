using backend.Dto.RoleDtos;
using backend.GenericResponse;
using backend.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Authorization;

namespace backend.Controllers.V1;

[Authorize(Roles = RoleConstants.Admin)]
[ApiController]
[Route("api/[controller]")]
public class RoleController(IRoleService roleService, ILogger<RoleController> logger) : ControllerBase
{

    [HttpPost("CreateRole")]
    public async Task<IActionResult> CreateRoleAsync(RoleDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        logger.LogTrace("CreateRole called with dto: {RoleName}", dto.Name);
        try
        {

            if (!ModelState.IsValid)
            {
                logger.LogWarning("Invalid request body provided.");
                return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
            }

            var result = await roleService.CreateRole(dto).ConfigureAwait(false);

            if (result.Item1 == 0)
            {
                logger.LogWarning("{Status code}", result.Item1);
                return BadRequest(ResponseResults<string>.Failure(result.Item1));
            }

            logger.LogInformation("Role created successfully name: {RoleName}", dto.Name);
            return Ok(ResponseResults<string>.Success(result.Item1));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating role.");
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }

    [HttpGet("GetAllRoles")]
    public async Task<IActionResult> GetAllRolesAsync()
    {
        logger.LogTrace("GetAllRoles called.");
        try
        {
            var result = await roleService.GetAllRoles().ConfigureAwait(false);

            if (result.Item1 == 0)
            {
                logger.LogWarning("{Status code}", result.Item1);
                return NotFound(ResponseResults<string>.Failure(result.Item1));
            }

            logger.LogInformation("Retrieved all roles count: {Count}", result.Item2?.Count ?? 0);
            return Ok(ResponseResults<IReadOnlyCollection<RoleResponseDto>>.Success(result.Item1, result.Item2));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching all roles.");
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }

    [HttpGet("GetRoleById/{id}")]
    public async Task<IActionResult> GetRoleByIdAsync(Guid id)
    {
        logger.LogTrace("GetRoleById called with id: {RoleId}", id);
        try
        {
            var result = await roleService.GetRoleById(id).ConfigureAwait(false);

            if (result.Item1 == 0)
            {
                logger.LogWarning("{Status code} id: {RoleId}", result.Item1, id);
                return NotFound(ResponseResults<string>.Failure(result.Item1));
            }

            logger.LogInformation("Retrieved role with id: {RoleId} name: {RoleName}", id, result.Item2?.Name);
            return Ok(ResponseResults<RoleResponseDto>.Success(result.Item1, result.Item2));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching role with id: {RoleId}", id);
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }

    [HttpGet("GetUsersByRole/{roleId}")]
    public async Task<IActionResult> GetUsersByRoleAsync(Guid roleId)
    {
        logger.LogTrace("GetUsersByRole called with id: {RoleId}", roleId);
        try
        {
            var result = await roleService.GetUsersByRole(roleId).ConfigureAwait(false);
            if (result.Item1 == 0)
            {
                logger.LogWarning("{Status code} id: {RoleId}", result.Item1, roleId);
                return NotFound(ResponseResults<string>.Failure(result.Item1));
            }

            logger.LogInformation("Retrieved users for role with id: {RoleId} count: {Count}", roleId, result.Item2?.Count ?? 0);
            return Ok(ResponseResults<IReadOnlyCollection<RoleUserResponseDto>>.Success(result.Item1, result.Item2));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching users for role with id: {RoleId}", roleId);
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }
}
