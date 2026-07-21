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
    public async Task<Tuple<int, TokenDto>> LoginUser(LoginDto dto)
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
                .FirstOrDefaultAsync(x => x.Email == dto.Email)
                .ConfigureAwait(false);

            if (existingUser == null)
            {
                tokenDto.Message = "User not found";
                return new Tuple<int, TokenDto>(CustomCodes.UserNotFound, tokenDto);
            }

            var passwordHasher = new PasswordHasher<string>();

            var verificationResult = passwordHasher.VerifyHashedPassword(dto.Email ?? "", existingUser.Password ?? "", dto.Password ?? "");

            if (verificationResult == PasswordVerificationResult.Failed)
            {
                tokenDto.Message = "Invalid credentials";
                return new Tuple<int, TokenDto>(CustomCodes.InvalidCredentials, tokenDto);
            }

            if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
            {
                existingUser.Password = PasswordHashing(existingUser.Email ?? "", dto.Password ?? "");

                await context.SaveChangesAsync().ConfigureAwait(false);
            }

            var token = GetJwtToken(existingUser);

            tokenDto.Token = token;
            tokenDto.Message = "Login successful";
            tokenDto.UserId = existingUser.Id;
            tokenDto.Name = existingUser.Name ?? "";
            tokenDto.Email = existingUser.Email ?? "";
            tokenDto.Role = existingUser.Role != null ? existingUser.Role.Name : "";

            return new Tuple<int, TokenDto>(CustomCodes.LoginSuccessfully, tokenDto);
        }
        catch (InvalidOperationException ex)
        {
            TokenDto tokenDto = new() { Message = ex.Message };

            return new Tuple<int, TokenDto>(CustomCodes.InternalServerError, tokenDto);
        }
        catch (Exception ex)
        {
            TokenDto tokenDto = new() { Message = ex.Message };

            return new Tuple<int, TokenDto>(CustomCodes.InternalServerError, tokenDto);
            throw;
        }
    }

    public async Task<Tuple<int>> RegisterUser(RegisterUserDto dto)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(dto);

            ArgumentNullException.ThrowIfNull(dto);

            var existingUser = await context.Users.FirstOrDefaultAsync(x => x.Email == dto.Email).ConfigureAwait(false);

            if (existingUser != null)
            {
                return new Tuple<int>(CustomCodes.UserAlreadyExists);
            }

            var branchExists = await context.Branches.AnyAsync(x => x.Id == dto.BranchId).ConfigureAwait(false);

            if (!branchExists)
            {
                return new Tuple<int>(CustomCodes.BranchNotFound);
            }

            var departmentExists = await context.Departments.AnyAsync(x => x.Id == dto.DepartmentId).ConfigureAwait(false);

            if (!departmentExists)
            {
                return new Tuple<int>(CustomCodes.DepartmentNotFound);
            }

            var positionExists = await context.Positions.AnyAsync(x => x.Id == dto.PositionId && x.DepartmentId == dto.DepartmentId).ConfigureAwait(false);

            if (!positionExists)
            {
                return new Tuple<int>(CustomCodes.PositionNotFound);
            }

            var roleExists = await context.Roles.AnyAsync(x => x.Id == dto.RoleId).ConfigureAwait(false);

            if (!roleExists)
            {
                return new Tuple<int>(CustomCodes.RoleNotFound);
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

            await context.Users.AddAsync(newUser).ConfigureAwait(false);

            await context.SaveChangesAsync().ConfigureAwait(false);

            return new Tuple<int>(CustomCodes.UserCreatedSuccessfully);
        }
        catch (DbUpdateException)
        {
            return new Tuple<int>(CustomCodes.UserCreationFailed);
        }
        catch (Exception)
        {
            return new Tuple<int>(CustomCodes.InternalServerError);
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

