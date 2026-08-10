using backend.Dto;
using backend.Entities;
using backend.GenericResponse;
using backend.IRepository;
using backend.IService;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace backend.Services;

internal sealed class AuthService(IAuthRepository authRepository, IUnitOfWork unitOfWork, IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : IAuthService
{
    public async Task<ServiceResponse<TokenDto>> LoginUser(LoginDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);

        try
        {
            TokenDto tokenDto = new();

            var existingUser = await authRepository.GetUserByEmailWithDetailsAsync(dto.Email, cancellationToken).ConfigureAwait(false);

            if (existingUser == null)
            {
                return new ServiceResponse<TokenDto>
                {
                    StatusCode = CustomCodes.UserNotFound,
                    IsSuccess = false,
                    Data = tokenDto
                };
            }

            var passwordHasher = new PasswordHasher<string>();

            var verificationResult = passwordHasher.VerifyHashedPassword(
                dto.Email ?? string.Empty,
                existingUser.Password ?? string.Empty,
                dto.Password ?? string.Empty);

            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return new ServiceResponse<TokenDto>
                {
                    StatusCode = CustomCodes.InvalidCredentials,
                    IsSuccess = false,
                    Data = tokenDto
                };
            }

            if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
            {
                existingUser.Password = PasswordHashing(existingUser.Email ?? string.Empty, dto.Password ?? string.Empty);

                await unitOfWork
                    .SaveChangesAsync(cancellationToken)
                    .ConfigureAwait(false);

            }

            var token = GetJwtToken(existingUser);

            httpContextAccessor.HttpContext?.Response.Cookies.Append(
                "jwt",
                token,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(10)
                });

            tokenDto.UserId = existingUser.Id;
            tokenDto.Name = existingUser.Name ?? string.Empty;
            tokenDto.Email = existingUser.Email ?? string.Empty;
            tokenDto.Role = existingUser.Role?.Name ?? string.Empty;
            tokenDto.Branch = existingUser.Branch?.Name ?? string.Empty;

            return new ServiceResponse<TokenDto>
            {
                StatusCode = CustomCodes.LoginSuccessfully,
                IsSuccess = true,
                Data = tokenDto
            };
        }
        catch (OperationCanceledException)
        {

            throw;
        }
        catch (InvalidOperationException)
        {

            return new ServiceResponse<TokenDto>
            {
                StatusCode = CustomCodes.InternalServerError,
                IsSuccess = false
            };
        }
        catch (Exception)
        {
            return new ServiceResponse<TokenDto>
            {
                StatusCode = CustomCodes.InternalServerError,
                IsSuccess = false
            };
            throw;
        }
    }

    public async Task<ServiceResponse<object>> RegisterUser(RegisterUserDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        try
        {
            var emailExists = await authRepository.EmailExistsAsync(dto.Email, cancellationToken).ConfigureAwait(false);

            if (emailExists)
            {
                return new ServiceResponse<object>
                {
                    StatusCode = CustomCodes.UserAlreadyExists,
                    IsSuccess = false
                };
            }

            var branchExists = await authRepository.BranchExistsAsync(dto.BranchId, cancellationToken).ConfigureAwait(false);

            if (!branchExists)
            {
                return new ServiceResponse<object>
                {
                    StatusCode = CustomCodes.BranchNotFound,
                    IsSuccess = false
                };
            }

            var departmentExists = await authRepository.DepartmentExistsAsync(dto.DepartmentId, cancellationToken).ConfigureAwait(false);

            if (!departmentExists)
            {
                return new ServiceResponse<object>
                {
                    StatusCode = CustomCodes.DepartmentNotFound,
                    IsSuccess = false
                };
            }

            var positionExists = await authRepository.PositionExistsAsync(dto.PositionId, dto.DepartmentId, cancellationToken).ConfigureAwait(false);

            if (!positionExists)
            {
                return new ServiceResponse<object>
                {
                    StatusCode = CustomCodes.PositionNotFound,
                    IsSuccess = false
                };
            }

            var roleExists = await authRepository
                .RoleExistsAsync(dto.RoleId, cancellationToken)
                .ConfigureAwait(false);

            if (!roleExists)
            {
                return new ServiceResponse<object>
                {
                    StatusCode = CustomCodes.RoleNotFound,
                    IsSuccess = false
                };
            }

            cancellationToken.ThrowIfCancellationRequested();

            User newUser = new()
            {
                Id = Guid.NewGuid(),
                Name = dto.Name ?? string.Empty,
                Email = dto.Email ?? string.Empty,
                Password = PasswordHashing(
                    dto.Email ?? string.Empty,
                    dto.Password ?? string.Empty),
                DOB = dto.DOB,
                BranchId = dto.BranchId,
                DepartmentId = dto.DepartmentId,
                PositionId = dto.PositionId,
                RoleId = dto.RoleId
            };

            await authRepository.AddUserAsync(newUser, cancellationToken).ConfigureAwait(false);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new ServiceResponse<object>
            {
                StatusCode = CustomCodes.UserCreatedSuccessfully,
                IsSuccess = true
            };
        }
        catch (OperationCanceledException)
        {
            return new ServiceResponse<object>
            {
                StatusCode = CustomCodes.OperationCancelled,
                IsSuccess = false
            };
        }
        catch (DbUpdateException)
        {
            return new ServiceResponse<object>
            {
                StatusCode = CustomCodes.UserCreationFailed,
                IsSuccess = false
            };
        }
        catch (Exception)
        {
            return new ServiceResponse<object>
            {
                StatusCode = CustomCodes.InternalServerError,
                IsSuccess = false
            };
            throw;
        }
    }

    private string GetJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty)
        };
        if (!string.IsNullOrWhiteSpace(user.Role?.Name))
        {
            claims.Add(new Claim(ClaimTypes.Role, user.Role.Name));
        }

        var jwtKey = configuration["Jwt:Key"];

        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            throw new InvalidOperationException("JWT key is missing.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(10),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string PasswordHashing(string email, string password)
    {
        var passwordHasher = new PasswordHasher<string>();

        return passwordHasher.HashPassword(email, password);
    }
}
