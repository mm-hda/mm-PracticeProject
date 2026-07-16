using backend.Data;
using backend.Dto.RoleDto;
using backend.Entities;
using backend.IService;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class RoleService(AppDbContext _context) : IRoleService
    {
        public async Task<Tuple<int, string>> CreateRole(RoleDto dto)
        {
            try
            {
                bool exists = await _context.Roles.AnyAsync(x => x.Name.ToLower() == dto.Name.ToLower());

                if (exists)
                {
                    return new Tuple<int, string>(0, "Role already exists");
                }

                Role role = new()
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name
                };

                await _context.Roles.AddAsync(role);

                await _context.SaveChangesAsync();

                return new Tuple<int, string>(1, "Role created successfully");
            }
            catch (Exception ex)
            {
                return new Tuple<int, string>(0, ex.Message);
            }
        }

        public async Task<Tuple<int, List<RoleResponseDto>, string>> GetAllRoles()
        {
            try
            {
                var roles = await _context.Roles.AsNoTracking()
                    .Select(x => new RoleResponseDto
                    {
                        Id = x.Id,
                        Name = x.Name
                    }).ToListAsync();

                return new Tuple<int, List<RoleResponseDto>, string>(1, roles, "Roles retrieved successfully");
            }
            catch (Exception ex)
            {
                return new Tuple<int, List<RoleResponseDto>, string>(0, new List<RoleResponseDto>(), ex.Message);
            }
        }

        public async Task<Tuple<int, RoleResponseDto?, string>> GetRoleById(Guid id)
        {
            try
            {
                var role = await _context.Roles.AsNoTracking()
                    .Where(x => x.Id == id)
                    .Select(x => new RoleResponseDto
                    {
                        Id = x.Id,
                        Name = x.Name
                    }).FirstOrDefaultAsync();

                return new Tuple<int, RoleResponseDto?, string>(role != null ? 1 : 0, role, role != null ? "Role retrieved successfully" : "Role not found");
            }
            catch (Exception ex)
            {
                return new Tuple<int, RoleResponseDto?, string>(0, null, ex.Message);
            }
        }

        public async Task<Tuple<int, List<RoleUserResponseDto>, string>> GetUsersByRole(Guid roleId)
        {
            try
            {
                var users = await _context.Users
                    .Include(x => x.Role)
                    .Include(x => x.Department)
                    .Include(x => x.Position)
                    .Include(x => x.Branch)
                    .Where(x => x.RoleId == roleId)
                    .Select(x => new RoleUserResponseDto
                    {
                        UserId = x.Id,

                        Name = x.Name ?? "",

                        Email = x.Email ?? "",

                        RoleName = x.Role != null ? x.Role.Name : "",

                        DepartmentName = x.Department != null ? x.Department.Name : "",

                        PositionName = x.Position != null ? x.Position.Name : "",

                        BranchName = x.Branch != null ? x.Branch.Name : ""
                    }).ToListAsync();

                return new Tuple<int, List<RoleUserResponseDto>, string>(1, users, "Users retrieved successfully");
            }
            catch (Exception ex)
            {
                return new Tuple<int, List<RoleUserResponseDto>, string>(0, new List<RoleUserResponseDto>(), ex.Message);
            }
        }
    }
}