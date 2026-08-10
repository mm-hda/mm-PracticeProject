using backend.Dto.RoleDtos;
using backend.GenericResponse;
using backend.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Authorization;

namespace backend.Controllers.V1;

[Authorize(Roles = RoleConstants.Admin + "," + RoleConstants.HR)]
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
    public async Task<IActionResult> GetAllRolesAsync(CancellationToken cancellationToken)
    {
        logger.LogTrace("GetAllRoles called.");

        var result = await roleService.GetAllRoles(cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            logger.LogWarning("{Status code}", result.StatusCode);
            return NotFound(ResponseResults<string>.Failure(result.StatusCode));
        }

        logger.LogInformation("Retrieved all roles count: {Count}", result.Data?.Count ?? 0);
        return Ok(ResponseResults<IReadOnlyCollection<RoleResponseDto>>.Success(result.StatusCode, result.Data));
    }
}
