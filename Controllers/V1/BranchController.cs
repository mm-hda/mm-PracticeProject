using backend.Dto.BranchDtos;
using backend.GenericResponse;
using backend.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Authorization;

namespace backend.Controllers.V1;

[Authorize(Roles = RoleConstants.Admin + "," + RoleConstants.HR)]
[ApiController]
[Route("api/[controller]")]
public class BranchController(IBranchService branchService, ILogger<BranchController> logger) : ControllerBase
{

    [HttpPost("CreateBranch")]
    public async Task<IActionResult> CreateBranchAsync([FromBody] BranchDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        logger.LogTrace("CreateBranch called with dto: {@BranchName}", dto.Name);
        try
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (!ModelState.IsValid)
            {
                logger.LogWarning("Invalid request body provided for branch creation.");
                return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
            }

            var result = await branchService.CreateBranch(dto).ConfigureAwait(false);

            if (result.Item1 == 0)
            {
                logger.LogWarning("Branch creation failed.");
                return BadRequest(ResponseResults<string>.Failure(result.Item1));
            }

            logger.LogInformation("Branch created successfully.");
            return Ok(ResponseResults<string>.Success(result.Item1));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating branch.");
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }

    [HttpPut("UpdateBranch")]
    public async Task<IActionResult> UpdateBranchAsync([FromBody] BranchDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        logger.LogTrace("UpdateBranch called with dto: {@BranchName}", dto.Name);
        try
        {

            if (!ModelState.IsValid)
            {
                logger.LogWarning("Invalid request body provided for branch update.");
                return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
            }

            var result = await branchService.UpdateBranch(dto).ConfigureAwait(false);

            if (result.Item1 == 0)
            {
                logger.LogWarning("Branch update failed.");
                return BadRequest(ResponseResults<string>.Failure(result.Item1));
            }

            logger.LogInformation("Branch updated successfully.");
            return Ok(ResponseResults<string>.Success(result.Item1));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while updating branch.");
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }

    [HttpGet("GetAllBranches")]
    public async Task<IActionResult> GetAllBranchesAsync()
    {
        logger.LogTrace("GetAllBranches called.");
        try
        {
            var result = await branchService.GetAllBranches().ConfigureAwait(false);

            logger.LogInformation("Retrieved all branches.");
            return Ok(ResponseResults<IReadOnlyCollection<BranchResponseDto>>.Success(result.Item1, result.Item2));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching all branches.");
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }

    [HttpGet("GetBranchById/{id}")]
    public async Task<IActionResult> GetBranchByIdAsync(Guid id)
    {
        logger.LogTrace("GetBranchById called with id: {BranchId}", id);
        try
        {
            if (id == Guid.Empty)
            {
                logger.LogWarning("Invalid branch id provided.");
                return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
            }

            var result = await branchService.GetBranchById(id).ConfigureAwait(false);

            if (result.Item1 == 0)
            {
                logger.LogWarning("Branch not found with id: {BranchId}", id);
                return NotFound(ResponseResults<string>.Failure(result.Item1));
            }

            logger.LogInformation("Retrieved branch with id: {BranchId}", id);
            return Ok(ResponseResults<BranchResponseDto>.Success(result.Item1, result.Item2));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching branch with id: {BranchId}", id);
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }

    [HttpGet("GetBranchUsers/{branchId}")]
    public async Task<IActionResult> GetBranchUsersAsync(Guid branchId)
    {
        logger.LogTrace("GetBranchUsers called with id: {BranchId}", branchId);
        try
        {
            if (branchId == Guid.Empty)
            {
                logger.LogWarning("Invalid branch id provided.");
                return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
            }

            var result = await branchService.GetBranchUsers(branchId).ConfigureAwait(false);

            if (result.Item1 == 0)
            {
                logger.LogWarning("Branch users not found with id: {BranchId}", branchId);
                return NotFound(ResponseResults<string>.Failure(result.Item1));
            }

            logger.LogInformation("Retrieved users for branch with id: {BranchId}", branchId);
            return Ok(ResponseResults<IReadOnlyCollection<BranchUserResponseDto>>.Success(result.Item1, result.Item2));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching users for branch with id: {BranchId}", branchId);
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }
}
