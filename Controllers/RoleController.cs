using backend.Dto.RoleDto;
using backend.GenericResponse;
using backend.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Authorization;

namespace backend.Controllers
{
    [Authorize(Roles = RoleConstants.Admin)]
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController(IRoleService _roleService, ILogger<RoleController> _logger) : ControllerBase
    {

        [HttpPost("CreateRole")]
        public async Task<IActionResult> CreateRole(RoleDto dto)
        {
            _logger.LogTrace("CreateRole called with dto: {RoleName}", dto.Name);
            try
            {

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid request body provided.");
                    return BadRequest(ResponseResults<string>.Failure(null, "Invalid request body"));
                }

                var result = await _roleService.CreateRole(dto);

                if (result.Item1 == 0)
                {
                    _logger.LogWarning("{Message}", result.Item2);
                    return BadRequest(ResponseResults<string>.Failure(null, result.Item2));
                }

                _logger.LogInformation("Role created successfully name: {RoleName}", dto.Name);
                return Ok(ResponseResults<string>.Success(null, result.Item2));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating role.");
                return StatusCode(500, ResponseResults<string>.Failure(null, ex.Message));
            }
        }

        [HttpGet("GetAllRoles")]
        public async Task<IActionResult> GetAllRoles()
        {
            _logger.LogTrace("GetAllRoles called.");
            try
            {
                var result = await _roleService.GetAllRoles();

                if (result.Item1 == 0)
                {
                    _logger.LogWarning("{Message}", result.Item3);
                    return NotFound(ResponseResults<string>.Failure(null, result.Item3));
                }

                _logger.LogInformation("Retrieved all roles count: {Count}", result.Item2?.Count ?? 0);
                return Ok(ResponseResults<List<RoleResponseDto>>.Success(result.Item2, result.Item3));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all roles.");
                return StatusCode(500, ResponseResults<string>.Failure(null, ex.Message));
            }
        }

        [HttpGet("GetRoleById/{id}")]
        public async Task<IActionResult> GetRoleById(Guid id)
        {
            _logger.LogTrace("GetRoleById called with id: {RoleId}", id);
            try
            {
                var result = await _roleService.GetRoleById(id);

                if (result.Item1 == 0)
                {
                    _logger.LogWarning("{Message} id: {RoleId}", result.Item3, id);
                    return NotFound(ResponseResults<string>.Failure(null, result.Item3));
                }

                _logger.LogInformation("Retrieved role with id: {RoleId} name: {RoleName}", id, result.Item2?.Name);
                return Ok(ResponseResults<RoleResponseDto>.Success(result.Item2, result.Item3));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching role with id: {RoleId}", id);
                return StatusCode(500, ResponseResults<string>.Failure(null, ex.Message));
            }
        }

        [HttpGet("GetUsersByRole/{roleId}")]
        public async Task<IActionResult> GetUsersByRole(Guid roleId)
        {
            _logger.LogTrace("GetUsersByRole called with id: {RoleId}", roleId);
            try
            {
                var result = await _roleService.GetUsersByRole(roleId);
                if (result.Item1 == 0)
                {
                    _logger.LogWarning("{Message} id: {RoleId}", result.Item3, roleId);
                    return NotFound(ResponseResults<string>.Failure(null, result.Item3));
                }

                _logger.LogInformation("Retrieved users for role with id: {RoleId} count: {Count}", roleId, result.Item2?.Count ?? 0);
                return Ok(ResponseResults<List<RoleUserResponseDto>>.Success(result.Item2, result.Item3));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching users for role with id: {RoleId}", roleId);
                return StatusCode(500, ResponseResults<string>.Failure(null, ex.Message));
            }
        }
    }
}