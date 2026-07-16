using backend.Data;
using backend.Dto.UserDto;
using backend.Dto.Common;
using backend.IService;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class UserService(AppDbContext _context) : IUserService
    {
        public async Task<Tuple<int, List<UserResponseDto>, PaginationMetaDto?, string>> GetAllUsers(PaginationDto dto)
        {
            try
            {
                dto.PageNumber = dto.PageNumber <= 0 ? 1 : dto.PageNumber;
                dto.PageSize = dto.PageSize <= 0 ? 10 : dto.PageSize;

                var query = _context.Users
                    .AsNoTracking()
                    .Include(x => x.Role)
                    .Include(x => x.Branch)
                    .Include(x => x.Department)
                    .Include(x => x.Position)
                    .Where(x => x.Role != null && x.Role.Name != "Admin");

                var totalRecords = await query.CountAsync();

                if (totalRecords == 0)
                {
                    return new(0, new List<UserResponseDto>(), null, "No users found");
                }

                if ((int)Math.Ceiling(totalRecords / (double)dto.PageSize) < dto.PageNumber)
                {
                    return new(0, new List<UserResponseDto>(), null, "Page number exceeds total pages");
                }

                var users = await query
                    .OrderBy(x => x.Name)
                    .Skip((dto.PageNumber - 1) * dto.PageSize)
                    .Take(dto.PageSize)
                    .Select(x => new UserResponseDto
                    {
                        UserId = x.Id,
                        Name = x.Name,
                        Email = x.Email,
                        DOB = x.DOB,
                        RoleName = x.Role != null ? x.Role.Name : "",
                        BranchName = x.Branch != null ? x.Branch.Name : "",
                        DepartmentName = x.Department != null ? x.Department.Name : "",
                        PositionName = x.Position != null ? x.Position.Name : ""
                    }).ToListAsync();

                var meta = new PaginationMetaDto
                {
                    PageNumber = dto.PageNumber,
                    PageSize = dto.PageSize,
                    TotalRecords = totalRecords,
                    TotalPages = (int)Math.Ceiling(totalRecords / (double)dto.PageSize)
                };

                return new(1, users, meta, "Users retrieved successfully");
            }
            catch (Exception ex)
            {
                return new(0, new List<UserResponseDto>(), null, ex.Message);
            }
        }

        public async Task<Tuple<int, List<UserResponseDto>, string>> GetUserBySearch(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return new(0, new List<UserResponseDto>(), "Search term cannot be empty");
                }

                var users = await _context.Users.AsNoTracking()
                    .Include(x => x.Role)
                    .Include(x => x.Branch)
                    .Include(x => x.Department)
                    .Include(x => x.Position)
                    .Where(x => x.Name.ToLower().Contains(searchTerm.ToLower()) || x.Email.ToLower().Contains(searchTerm.ToLower()))
                    .Select(x => new UserResponseDto
                    {
                        UserId = x.Id,
                        Name = x.Name,
                        Email = x.Email,
                        DOB = x.DOB,
                        RoleName = x.Role != null ? x.Role.Name : "",
                        BranchName = x.Branch != null ? x.Branch.Name : "",
                        DepartmentName = x.Department != null ? x.Department.Name : "",
                        PositionName = x.Position != null ? x.Position.Name : ""
                    }).ToListAsync();

                if (!users.Any())
                {
                    return new(0, new List<UserResponseDto>(), "User not found");
                }

                return new(1, users, "User retrieved successfully");
            }
            catch (Exception ex)
            {
                return new(0, new List<UserResponseDto>(), ex.Message);
            }
        }

        public async Task<Tuple<int, UserResponseDto?, string>> GetUserById(Guid id)
        {
            try
            {
                var user = await _context.Users.AsNoTracking()
                    .Include(x => x.Role)
                    .Include(x => x.Branch)
                    .Include(x => x.Department)
                    .Include(x => x.Position)
                    .Where(x => x.Id == id)
                    .Select(x => new UserResponseDto
                    {
                        UserId = x.Id,
                        Name = x.Name,
                        Email = x.Email,
                        DOB = x.DOB,
                        RoleName = x.Role != null ? x.Role.Name : "",
                        BranchName = x.Branch != null ? x.Branch.Name : "",
                        DepartmentName = x.Department != null ? x.Department.Name : "",
                        PositionName = x.Position != null ? x.Position.Name : ""
                    }).FirstOrDefaultAsync();

                if (user == null)
                {
                    return new(0, new UserResponseDto(), "User not found");
                }

                return new(1, user, "User retrieved successfully");
            }
            catch (Exception ex)
            {
                return new(0, new UserResponseDto(), ex.Message);
            }
        }

        public async Task<Tuple<int, List<UserResponseDto>, string>> GetUsersByFilter(UserFilterDto dto)
        {
            try
            {
                var query = _context.Users.AsNoTracking()
                    .Include(x => x.Role)
                    .Include(x => x.Branch)
                    .Include(x => x.Department)
                    .Include(x => x.Position)
                    .Where(x => x.Role != null && x.Role.Name != "Admin")
                    .AsQueryable();

                if (dto.RoleId.HasValue)
                {
                    query = query.Where(x => x.RoleId == dto.RoleId.Value);
                }

                if (dto.BranchId.HasValue)
                {
                    query = query.Where(x => x.BranchId == dto.BranchId.Value);
                }

                if (dto.DepartmentId.HasValue)
                {
                    query = query.Where(x => x.DepartmentId == dto.DepartmentId.Value);
                }

                if (dto.PositionId.HasValue)
                {
                    query = query.Where(x => x.PositionId == dto.PositionId.Value);
                }

                var users = await query.Select(x => new UserResponseDto
                {
                    UserId = x.Id,
                    Name = x.Name,
                    Email = x.Email,
                    DOB = x.DOB,
                    RoleName = x.Role != null ? x.Role.Name : "",
                    BranchName = x.Branch != null ? x.Branch.Name : "",
                    DepartmentName = x.Department != null ? x.Department.Name : "",
                    PositionName = x.Position != null ? x.Position.Name : ""
                }).ToListAsync();

                return new(1, users, "Users retrieved successfully");
            }
            catch (Exception ex)
            {
                return new(0, new List<UserResponseDto>(), ex.Message);
            }
        }
    }
}