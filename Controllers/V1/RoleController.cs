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
    public async Task<IActionResult> CreateRoleAsync(RoleDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);

        logger.LogTrace("CreateRole called with dto: {RoleName}", dto.Name);

        var result = await roleService.CreateRole(dto, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            logger.LogWarning("{Status code}", result.StatusCode);
            return BadRequest(ResponseResults<string>.Failure(result.StatusCode));
        }

        logger.LogInformation("Role created successfully name: {RoleName}", dto.Name);
        return Ok(ResponseResults<string>.Success(result.StatusCode));

    }

    [HttpGet("GetAllRoles")]
    public async Task<IActionResult> GetAllRolesAsync()
    {
        logger.LogTrace("GetAllRoles called.");

        var result = await roleService.GetAllRoles().ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            logger.LogWarning("{Status code}", result.StatusCode);
            return NotFound(ResponseResults<string>.Failure(result.StatusCode));
        }

        logger.LogInformation("Retrieved all roles count: {Count}", result.Data?.Count ?? 0);
        return Ok(ResponseResults<IReadOnlyCollection<RoleResponseDto>>.Success(result.StatusCode, result.Data));

    }

    [HttpGet("GetRoleById/{id}")]
    public async Task<IActionResult> GetRoleByIdAsync(Guid id)
    {
        logger.LogTrace("GetRoleById called with id: {RoleId}", id);

        var result = await roleService.GetRoleById(id).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            logger.LogWarning("{Status code} id: {RoleId}", result.StatusCode, id);
            return NotFound(ResponseResults<string>.Failure(result.StatusCode));
        }

        logger.LogInformation("Retrieved role with id: {RoleId} name: {RoleName}", id, result.Data?.Name);
        return Ok(ResponseResults<RoleResponseDto>.Success(result.StatusCode, result.Data));

    }

    [HttpGet("GetUsersByRole/{roleId}")]
    public async Task<IActionResult> GetUsersByRoleAsync(Guid roleId)
    {
        logger.LogTrace("GetUsersByRole called with id: {RoleId}", roleId);

        var result = await roleService.GetUsersByRole(roleId).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            logger.LogWarning("{Status code} id: {RoleId}", result.StatusCode, roleId);
            return NotFound(ResponseResults<string>.Failure(result.StatusCode));
        }

        logger.LogInformation("Retrieved users for role with id: {RoleId} count: {Count}", roleId, result.Data?.Count ?? 0);
        return Ok(ResponseResults<IReadOnlyCollection<RoleUserResponseDto>>.Success(result.StatusCode, result.Data));
    }
}
