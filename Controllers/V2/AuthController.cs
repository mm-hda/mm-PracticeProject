using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using backend.Dto;
using backend.IService;
using backend.Authorization;
using backend.GenericResponse;

using Asp.Versioning;

namespace backend.Controllers.V2;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController(IAuthService authService, ILogger<AuthController> logger) : ControllerBase
{

    [Authorize(Roles = RoleConstants.Admin)]
    [HttpPost("Register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterUserDtoV2 registerDto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(registerDto);
        logger.LogTrace("Register called V2 with dto: {@Email}", registerDto.Email);

        try
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning("Invalid request body provided for user registration.");
                return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
            }

            RegisterUserDto dto = new()
            {
                Name = registerDto.FirstName + " " + registerDto.LastName,
                Email = registerDto.Email,
                Password = registerDto.Password,
                DOB = registerDto.DOB,
                RoleId = registerDto.RoleId,
                BranchId = registerDto.BranchId,
                DepartmentId = registerDto.DepartmentId,
                PositionId = registerDto.PositionId
            };

            var result = await authService.RegisterUser(dto, cancellationToken).ConfigureAwait(false);

            if (!result.IsSuccess)
            {
                logger.LogWarning("User registration failed for email: {Email}", registerDto.Email);
                return BadRequest(ResponseResults<string>.Failure(result.StatusCode));
            }

            if (result.StatusCode == 2)
            {
                logger.LogError("An error occurred while registering user: {Email}", registerDto.Email);
                return StatusCode(500, ResponseResults<string>.Failure(result.StatusCode));
            }

            logger.LogInformation("User registered successfully: {Email}", registerDto.Email);
            return Ok(ResponseResults<string>.Success(result.StatusCode));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while registering user: {Email}", registerDto.Email);
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }
}
