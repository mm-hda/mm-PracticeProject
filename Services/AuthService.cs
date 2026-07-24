using backend.Data;
using backend.Dto;
using backend.Entities;
using backend.IService;
using backend.GenericResponse;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace backend.Services;

internal sealed class AuthService(AppDbContext context, IConfiguration configuration) : IAuthService
{
    public async Task<ServiceResponse<TokenDto>> LoginUser(LoginDto dto, CancellationToken cancellationToken)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(dto);
            TokenDto tokenDto = new();

            var existingUser = await context.Users
                .Include(x => x.Role)
                .Include(x => x.Branch)
                .Include(x => x.Department)
                .Include(x => x.Position)
                .FirstOrDefaultAsync(x => x.Email == dto.Email, cancellationToken)
                .ConfigureAwait(false);

            if (existingUser == null)
            {
                tokenDto.Message = "User not found";
                return new ServiceResponse<TokenDto> { StatusCode = CustomCodes.UserNotFound, IsSuccess = false };
            }

            var passwordHasher = new PasswordHasher<string>();

            var verificationResult = passwordHasher.VerifyHashedPassword(dto.Email ?? "", existingUser.Password ?? "", dto.Password ?? "");

            if (verificationResult == PasswordVerificationResult.Failed)
            {
                tokenDto.Message = "Invalid credentials";
                return new ServiceResponse<TokenDto> { StatusCode = CustomCodes.InvalidCredentials, IsSuccess = false };
            }

            if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
            {
                existingUser.Password = PasswordHashing(existingUser.Email ?? "", dto.Password ?? "");

                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            var token = GetJwtToken(existingUser);

            tokenDto.Token = token;
            tokenDto.Message = "Login successful";
            tokenDto.UserId = existingUser.Id;
            tokenDto.Name = existingUser.Name ?? "";
            tokenDto.Email = existingUser.Email ?? "";
            tokenDto.Role = existingUser.Role != null ? existingUser.Role.Name : "";

            return new ServiceResponse<TokenDto> { StatusCode = CustomCodes.LoginSuccessfully, IsSuccess = true, Data = tokenDto };
        }
        catch (InvalidOperationException)
        {
            return new ServiceResponse<TokenDto> { StatusCode = CustomCodes.InternalServerError, IsSuccess = false };
        }
        catch (Exception)
        {
            return new ServiceResponse<TokenDto> { StatusCode = CustomCodes.InternalServerError, IsSuccess = false };
            throw;
        }
    }

    public async Task<ServiceResponse<object>> RegisterUser(RegisterUserDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);

        using var transaction = await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        try
        {

            var existingUser = await context.Users.FirstOrDefaultAsync(x => x.Email == dto.Email, cancellationToken).ConfigureAwait(false);

            if (existingUser != null)
            {
                return new ServiceResponse<object> { StatusCode = CustomCodes.UserAlreadyExists, IsSuccess = false };
            }

            var branchExists = await context.Branches.AnyAsync(x => x.Id == dto.BranchId, cancellationToken).ConfigureAwait(false);

            if (!branchExists)
            {
                return new ServiceResponse<object> { StatusCode = CustomCodes.BranchNotFound, IsSuccess = false };
            }

            var departmentExists = await context.Departments.AnyAsync(x => x.Id == dto.DepartmentId, cancellationToken).ConfigureAwait(false);

            if (!departmentExists)
            {
                return new ServiceResponse<object> { StatusCode = CustomCodes.DepartmentNotFound, IsSuccess = false };
            }

            var positionExists = await context.Positions.AnyAsync(x => x.Id == dto.PositionId && x.DepartmentId == dto.DepartmentId, cancellationToken).ConfigureAwait(false);

            if (!positionExists)
            {
                return new ServiceResponse<object> { StatusCode = CustomCodes.PositionNotFound, IsSuccess = false };
            }

            var roleExists = await context.Roles.AnyAsync(x => x.Id == dto.RoleId, cancellationToken).ConfigureAwait(false);

            if (!roleExists)
            {
                return new ServiceResponse<object> { StatusCode = CustomCodes.RoleNotFound, IsSuccess = false };
            }

            User newUser = new()
            {
                Id = Guid.NewGuid(),
                Name = dto.Name ?? "",
                Email = dto.Email ?? "",
                Password = PasswordHashing(dto.Email ?? "", dto.Password ?? ""),
                DOB = dto.DOB,
                BranchId = dto.BranchId,
                DepartmentId = dto.DepartmentId,
                PositionId = dto.PositionId,
                RoleId = dto.RoleId
            };

            await context.Users.AddAsync(newUser, cancellationToken).ConfigureAwait(false);

            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return new ServiceResponse<object> { StatusCode = CustomCodes.UserCreatedSuccessfully, IsSuccess = true };
        }
        catch (OperationCanceledException)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { StatusCode = CustomCodes.InternalServerError, IsSuccess = false };
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { StatusCode = CustomCodes.UserCreationFailed, IsSuccess = false };
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { StatusCode = CustomCodes.InternalServerError, IsSuccess = false };
            throw;
        }
    }

    private string GetJwtToken(User user)
    {
        var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Name ?? ""),
                new(ClaimTypes.Email, user.Email ?? ""),
                new(ClaimTypes.Role, user.Role?.Name ?? "")
            };

        if (user.Role != null)
        {
            claims.Add(new Claim(ClaimTypes.Role, user.Role.Name));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(10),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string PasswordHashing(string email, string password)
    {
        var passwordHasher = new PasswordHasher<string>();

        return passwordHasher.HashPassword(email, password);
    }
}

