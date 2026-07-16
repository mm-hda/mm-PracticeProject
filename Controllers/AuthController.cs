using backend.Dto;
using backend.GenericResponse;
using backend.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Authorization;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService _authService, ILogger<AuthController> _logger) : ControllerBase
    {
        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            _logger.LogTrace("Login called with dto: {@Email}", loginDto.Email);
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid request body provided for login.");
                    return BadRequest(ResponseResults<string>.Failure(null, "Invalid request body"));
                }

                var result = await _authService.LoginUser(loginDto);

                if (result.Item1 == 0)
                {
                    _logger.LogWarning("Login failed for user: {Email}", loginDto.Email);
                    return NotFound(ResponseResults<string>.Failure(null, result.Item2.Message));
                }

                if (result.Item1 == 1)
                {
                    _logger.LogWarning("Invalid credentials provided for user: {Email}", loginDto.Email);
                    return BadRequest(ResponseResults<string>.Failure(null, result.Item2.Message));
                }

                if (result.Item1 == 3)
                {
                    _logger.LogError("An error occurred while logging in user: {Email}", loginDto.Email);
                    return StatusCode(500, ResponseResults<string>.Failure(null, result.Item2.Message));
                }

                _logger.LogInformation("User logged in successfully: {Email}", loginDto.Email);
                return Ok(ResponseResults<TokenDto>.Success(result.Item2, result.Item2.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while logging in user: {Email}", loginDto.Email);
                return StatusCode(500, ResponseResults<string>.Failure(null, ex.Message));
            }
        }

        [Authorize(Roles = RoleConstants.Admin)]
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerDto)
        {
            _logger.LogTrace("Register called with dto: {@Email}", registerDto.Email);
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid request body provided for user registration.");
                    return BadRequest(ResponseResults<string>.Failure(null, "Invalid request body"));
                }

                var result = await _authService.RegisterUser(registerDto);

                if (result.Item1 == 0)
                {
                    _logger.LogWarning("User registration failed for email: {Email}", registerDto.Email);
                    return BadRequest(ResponseResults<string>.Failure(null, result.Item2));
                }

                if (result.Item1 == 2)
                {
                    _logger.LogError("An error occurred while registering user: {Email}", registerDto.Email);
                    return StatusCode(500, ResponseResults<string>.Failure(null, result.Item2));
                }

                _logger.LogInformation("User registered successfully: {Email}", registerDto.Email);
                return Ok(ResponseResults<string>.Success(null, result.Item2));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while registering user: {Email}", registerDto.Email);
                return StatusCode(500, ResponseResults<string>.Failure(null, ex.Message));
            }
        }
    }
}