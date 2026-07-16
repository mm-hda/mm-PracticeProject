using backend.Data;
using backend.Dto;
using backend.Entities;
using backend.IService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<Tuple<int, TokenDto>> LoginUser(LoginDto dto)
        {
            try
            {
                TokenDto tokenDto = new();

                var existingUser = await _context.Users
                    .Include(x => x.Role)
                    .Include(x => x.Branch)
                    .Include(x => x.Department)
                    .Include(x => x.Position)
                    .FirstOrDefaultAsync(x => x.Email == dto.Email);

                if (existingUser == null)
                {
                    tokenDto.Message = "User not found";
                    return new Tuple<int, TokenDto>(0, tokenDto);
                }

                var passwordHasher = new PasswordHasher<string>();

                var verificationResult = passwordHasher.VerifyHashedPassword(dto.Email, existingUser.Password ?? "", dto.Password);

                if (verificationResult == PasswordVerificationResult.Failed)
                {
                    tokenDto.Message = "Invalid credentials";
                    return new Tuple<int, TokenDto>(1, tokenDto);
                }

                if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    existingUser.Password = PasswordHashing(existingUser.Email ?? "", dto.Password);

                    await _context.SaveChangesAsync();
                }

                var token = GetJwtToken(existingUser);

                tokenDto.Token = token;
                tokenDto.Message = "Login successful";
                tokenDto.UserId = existingUser.Id;
                tokenDto.Name = existingUser.Name ?? "";
                tokenDto.Email = existingUser.Email ?? "";
                tokenDto.Role = existingUser.Role != null ? existingUser.Role.Name : "";

                return new Tuple<int, TokenDto>(2, tokenDto);
            }
            catch (InvalidOperationException ex)
            {
                TokenDto tokenDto = new() { Message = ex.Message };

                return new Tuple<int, TokenDto>(3, tokenDto);
            }
            catch (Exception ex)
            {
                TokenDto tokenDto = new() { Message = ex.Message };

                return new Tuple<int, TokenDto>(3, tokenDto);
            }
        }

        public async Task<Tuple<int, string>> RegisterUser(RegisterUserDto dto)
        {
            try
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);

                if (existingUser != null)
                {
                    return new Tuple<int, string>(0, "User already exists");
                }

                bool branchExists = await _context.Branches.AnyAsync(x => x.Id == dto.BranchId);

                if (!branchExists)
                {
                    return new Tuple<int, string>(0, "Branch not found");
                }

                bool departmentExists = await _context.Departments.AnyAsync(x => x.Id == dto.DepartmentId);

                if (!departmentExists)
                {
                    return new Tuple<int, string>(0, "Department not found");
                }

                bool positionExists = await _context.Positions.AnyAsync(x => x.Id == dto.PositionId && x.DepartmentId == dto.DepartmentId);

                if (!positionExists)
                {
                    return new Tuple<int, string>(0, "Position not found for selected department");
                }

                bool roleExists = await _context.Roles.AnyAsync(x => x.Id == dto.RoleId);

                if (!roleExists)
                {
                    return new Tuple<int, string>(0, "Role not found");
                }

                User newUser = new()
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Email = dto.Email,
                    Password = PasswordHashing(dto.Email, dto.Password),
                    DOB = dto.DOB,
                    BranchId = dto.BranchId,
                    DepartmentId = dto.DepartmentId,
                    PositionId = dto.PositionId,
                    RoleId = dto.RoleId
                };

                await _context.Users.AddAsync(newUser);

                await _context.SaveChangesAsync();

                return new Tuple<int, string>(1, "User registered successfully");
            }
            catch (DbUpdateException ex)
            {
                return new Tuple<int, string>(2, ex.Message);
            }
            catch (Exception ex)
            {
                return new Tuple<int, string>(3, ex.Message);
            }
        }

        private string GetJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, user.Role != null ? user.Role.Name: "")
            };

            if (user.Role != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, user.Role.Name));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(10),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string PasswordHashing(string email, string password)
        {
            var passwordHasher = new PasswordHasher<string>();

            return passwordHasher.HashPassword(email, password);
        }
    }
}