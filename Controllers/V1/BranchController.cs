using backend.Dto.BranchDtos;
using backend.GenericResponse;
using backend.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Authorization;

namespace backend.Controllers.V1;

// [Authorize(Roles = RoleConstants.Admin + "," + RoleConstants.HR)]
[ApiController]
[Route("api/[controller]")]
public class BranchController(IBranchService branchService, ILogger<BranchController> logger) : ControllerBase
{

    [HttpPost("CreateBranch")]
    public async Task<IActionResult> CreateBranchAsync([FromBody] BranchDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);

        logger.LogTrace("CreateBranch called with dto: {@BranchName}", dto.Name);

        ArgumentNullException.ThrowIfNull(dto);

        var result = await branchService.CreateBranch(dto, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Branch creation failed.");
            return BadRequest(ResponseResults<string>.Failure(result.StatusCode));
        }

        logger.LogInformation("Branch created successfully.");
        return Ok(ResponseResults<string>.Success(result.StatusCode));

    }

    [HttpPut("UpdateBranch")]
    public async Task<IActionResult> UpdateBranchAsync([FromBody] BranchDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        logger.LogTrace("UpdateBranch called with dto: {@BranchName}", dto.Name);

        var result = await branchService.UpdateBranch(dto, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Branch update failed.");
            return BadRequest(ResponseResults<string>.Failure(result.StatusCode));
        }

        logger.LogInformation("Branch updated successfully.");
        return Ok(ResponseResults<string>.Success(result.StatusCode));
    }

    [HttpGet("GetAllBranches")]
    public async Task<IActionResult> GetAllBranchesAsync(CancellationToken cancellationToken)
    {
        logger.LogTrace("GetAllBranches called.");

        var result = await branchService.GetAllBranches(cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            logger.LogWarning("No branches found.");
            return NotFound(ResponseResults<string>.Failure(result.StatusCode));
        }

        logger.LogInformation("Retrieved all branches.");
        return Ok(ResponseResults<IReadOnlyCollection<BranchResponseDto>>.Success(result.StatusCode, result.Data));

    }

    [HttpGet("GetBranchById/{id}")]
    public async Task<IActionResult> GetBranchByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        logger.LogTrace("GetBranchById called with id: {BranchId}", id);

        if (id == Guid.Empty)
        {
            logger.LogWarning("Invalid branch id provided.");
            return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
        }

        var result = await branchService.GetBranchById(id, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Branch not found with id: {BranchId}", id);
            return NotFound(ResponseResults<string>.Failure(result.StatusCode));
        }

        logger.LogInformation("Retrieved branch with id: {BranchId}", id);
        return Ok(ResponseResults<BranchResponseDto>.Success(result.StatusCode, result.Data));

    }

    [HttpGet("GetBranchUsers/{branchId}")]
    public async Task<IActionResult> GetBranchUsersAsync(Guid branchId, CancellationToken cancellationToken)
    {
        logger.LogTrace("GetBranchUsers called with id: {BranchId}", branchId);

        if (branchId == Guid.Empty)
        {
            logger.LogWarning("Invalid branch id provided.");
            return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
        }

        var result = await branchService.GetBranchUsers(branchId, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Branch users not found with id: {BranchId}", branchId);
            return NotFound(ResponseResults<string>.Failure(result.StatusCode));
        }

        logger.LogInformation("Retrieved users for branch with id: {BranchId}", branchId);
        return Ok(ResponseResults<IReadOnlyCollection<BranchUserResponseDto>>.Success(result.StatusCode, result.Data));

    }
}
