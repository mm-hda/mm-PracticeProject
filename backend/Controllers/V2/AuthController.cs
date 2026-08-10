using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using backend.Dto;
using backend.IService;
using backend.Authorization;
using backend.GenericResponse;

using Asp.Versioning;

namespace backend.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("2.0")]
public class AuthController(IAuthService authService, ILogger<AuthController> logger) : ControllerBase
{

    [AllowAnonymous]
    [HttpPost("Login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginDto loginDto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(loginDto);
        logger.LogTrace("Login called with dto: {@Email}", loginDto.Email);

        var result = await authService.LoginUser(loginDto, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Login failed for user: {Email}", loginDto.Email);
            return NotFound(ResponseResults<string>.Failure(result.StatusCode));
        }

        logger.LogInformation("User logged in successfully: {Email}", loginDto.Email);
        return Ok(ResponseResults<TokenDto>.Success(result.StatusCode, result.Data));
    }

    [Authorize(Roles = RoleConstants.Admin + "," + RoleConstants.HR)]
    [HttpPost("Register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterUserDtoV2 registerDto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(registerDto);
        logger.LogTrace("Register called V2 with dto: {@Email}", registerDto.Email);
        Console.WriteLine($"Register called V2 with dto: {registerDto.Email}, {registerDto.FirstName}, {registerDto.LastName}, {registerDto.DOB}, {registerDto.RoleId}, {registerDto.BranchId}, {registerDto.DepartmentId}, {registerDto.PositionId}");
        try
        {
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

    [HttpPost("logout")]
    public IActionResult Logout([FromBody] LogoutDto logoutDto)
    {
        ArgumentNullException.ThrowIfNull(logoutDto);
        logger.LogInformation("User logged out successfully. email: {Email}", logoutDto.Email);
        Response.Cookies.Delete("jwt");
        return Ok();
    }
}
