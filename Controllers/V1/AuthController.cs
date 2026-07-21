using backend.Dto;
using backend.GenericResponse;
using backend.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Authorization;
using Asp.Versioning;

namespace backend.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/V{version:apiVersion}/[controller]")]
[Route("api/[controller]")]
public class AuthController(IAuthService authService, ILogger<AuthController> logger) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("Login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginDto loginDto)
    {
        ArgumentNullException.ThrowIfNull(loginDto);
        logger.LogTrace("Login called with dto: {@Email}", loginDto.Email);
        try
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning("Invalid request body provided for login.");
                return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
            }

            var result = await authService.LoginUser(loginDto).ConfigureAwait(false);

            if (result.Item1 == 0)
            {
                logger.LogWarning("Login failed for user: {Email}", loginDto.Email);
                return NotFound(ResponseResults<string>.Failure(result.Item1));
            }

            if (result.Item1 == 1)
            {
                logger.LogWarning("Invalid credentials provided for user: {Email}", loginDto.Email);
                return BadRequest(ResponseResults<string>.Failure(result.Item1));
            }

            if (result.Item1 == 3)
            {
                logger.LogError("An error occurred while logging in user: {Email}", loginDto.Email);
                return StatusCode(500, ResponseResults<string>.Failure(result.Item1));
            }

            logger.LogInformation("User logged in successfully: {Email}", loginDto.Email);
            return Ok(ResponseResults<TokenDto>.Success(result.Item1, result.Item2));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while logging in user: {Email}", loginDto.Email);
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }

    [Authorize(Roles = RoleConstants.Admin)]
    [HttpPost("Register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterUserDto registerDto)
    {
        ArgumentNullException.ThrowIfNull(registerDto);
        logger.LogTrace("Register called V1 with dto: {@Email}", registerDto.Email);
        try
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning("Invalid request body provided for user registration.");
                return BadRequest(ResponseResults<string>.Failure(CustomCodes.InvalidInput));
            }

            var result = await authService.RegisterUser(registerDto).ConfigureAwait(false);

            if (result.Item1 == 0)
            {
                logger.LogWarning("User registration failed for email: {Email}", registerDto.Email);
                return BadRequest(ResponseResults<string>.Failure(result.Item1));
            }

            if (result.Item1 == 2)
            {
                logger.LogError("An error occurred while registering user: {Email}", registerDto.Email);
                return StatusCode(500, ResponseResults<string>.Failure(result.Item1));
            }

            logger.LogInformation("User registered successfully: {Email}", registerDto.Email);
            return Ok(ResponseResults<string>.Success(result.Item1));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while registering user: {Email}", registerDto.Email);
            return StatusCode(500, ResponseResults<string>.Failure(CustomCodes.InternalServerError));
            throw;
        }
    }
}
