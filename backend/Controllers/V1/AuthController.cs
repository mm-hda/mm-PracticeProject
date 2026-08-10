using backend.Dto;
using backend.GenericResponse;
using backend.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Authorization;
using Asp.Versioning;

namespace backend.Controllers.V1;

[ApiController]
[Route("api/V{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Route("api/[controller]")]
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
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterUserDto registerDto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(registerDto);
        Console.WriteLine($"Register called V1 with dto: {registerDto.Email}, {registerDto.Name}, {registerDto.Password}, {registerDto.DOB}, {registerDto.RoleId}, {registerDto.BranchId}, {registerDto.DepartmentId}, {registerDto.PositionId}");
        logger.LogTrace("Register called V1 with dto: {@Email}", registerDto.Email);

        var result = await authService.RegisterUser(registerDto, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            logger.LogWarning("User registration failed for email: {Email}", registerDto.Email);
            return BadRequest(ResponseResults<string>.Failure(result.StatusCode));
        }

        logger.LogInformation("User registered successfully: {Email}", registerDto.Email);
        return Ok(ResponseResults<string>.Success(result.StatusCode));
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
