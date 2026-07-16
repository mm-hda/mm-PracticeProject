using backend.Dto.BranchDto;
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
    public class BranchController(IBranchService _branchService, ILogger<BranchController> _logger) : ControllerBase
    {

        [HttpPost("CreateBranch")]
        public async Task<IActionResult> CreateBranch([FromBody] BranchDto dto)
        {
            _logger.LogTrace("CreateBranch called with dto: {@BranchName}", dto.Name);
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid request body provided for branch creation.");
                    return BadRequest(ResponseResults<string>.Failure(null, "Invalid request body"));
                }

                var result = await _branchService.CreateBranch(dto);

                if (result.Item1 == 0)
                {
                    _logger.LogWarning("Branch creation failed.");
                    return BadRequest(ResponseResults<string>.Failure(null, result.Item2));
                }

                _logger.LogInformation("Branch created successfully.");
                return Ok(ResponseResults<string>.Success(null, result.Item2));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating branch.");
                return StatusCode(500, ResponseResults<string>.Failure(null, ex.Message));
            }
        }

        [HttpPut("UpdateBranch")]
        public async Task<IActionResult> UpdateBranch([FromBody] BranchDto dto)
        {
            _logger.LogTrace("UpdateBranch called with dto: {@BranchName}", dto.Name);
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid request body provided for branch update.");
                    return BadRequest(ResponseResults<string>.Failure(null, "Invalid request body"));
                }

                var result = await _branchService.UpdateBranch(dto);

                if (result.Item1 == 0)
                {
                    _logger.LogWarning("Branch update failed.");
                    return BadRequest(ResponseResults<string>.Failure(null, result.Item2));
                }

                _logger.LogInformation("Branch updated successfully.");
                return Ok(ResponseResults<string>.Success(null, result.Item2));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating branch.");
                return StatusCode(500, ResponseResults<string>.Failure(null, ex.Message));
            }
        }

        [HttpGet("GetAllBranches")]
        public async Task<IActionResult> GetAllBranches()
        {
            _logger.LogTrace("GetAllBranches called.");
            try
            {
                var result = await _branchService.GetAllBranches();

                _logger.LogInformation("Retrieved all branches.");
                return Ok(ResponseResults<List<BranchResponseDto>>.Success(result.Item2, result.Item3));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all branches.");
                return StatusCode(500, ResponseResults<string>.Failure(null, ex.Message));
            }
        }

        [HttpGet("GetBranchById/{id}")]
        public async Task<IActionResult> GetBranchById(Guid id)
        {
            _logger.LogTrace("GetBranchById called with id: {BranchId}", id);
            try
            {
                if (id == Guid.Empty)
                {
                    _logger.LogWarning("Invalid branch id provided.");
                    return BadRequest(ResponseResults<string>.Failure(null, "Invalid branch id"));
                }

                var result = await _branchService.GetBranchById(id);

                if (result.Item1 == 0)
                {
                    _logger.LogWarning("Branch not found with id: {BranchId}", id);
                    return NotFound(ResponseResults<string>.Failure(null, result.Item3));
                }

                _logger.LogInformation("Retrieved branch with id: {BranchId}", id);
                return Ok(ResponseResults<BranchResponseDto>.Success(result.Item2, result.Item3));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching branch with id: {BranchId}", id);
                return StatusCode(500, ResponseResults<string>.Failure(null, ex.Message));
            }
        }

        [HttpGet("GetBranchUsers/{branchId}")]
        public async Task<IActionResult> GetBranchUsers(Guid branchId)
        {
            _logger.LogTrace("GetBranchUsers called with id: {BranchId}", branchId);
            try
            {
                if (branchId == Guid.Empty)
                {
                    _logger.LogWarning("Invalid branch id provided.");
                    return BadRequest(ResponseResults<string>.Failure(null, "Invalid branch id"));
                }

                var result = await _branchService.GetBranchUsers(branchId);

                if (result.Item1 == 0)
                {
                    _logger.LogWarning("Branch users not found with id: {BranchId}", branchId);
                    return NotFound(ResponseResults<string>.Failure(null, result.Item3));
                }

                _logger.LogInformation("Retrieved users for branch with id: {BranchId}", branchId);
                return Ok(ResponseResults<List<BranchUserResponseDto>>.Success(result.Item2, result.Item3));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching users for branch with id: {BranchId}", branchId);
                return StatusCode(500, ResponseResults<string>.Failure(null, ex.Message));
            }
        }
    }
}