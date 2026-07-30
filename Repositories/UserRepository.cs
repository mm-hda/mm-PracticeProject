using backend.Data;
using backend.Dto.UserDtos;
using backend.Entities;
using backend.GenericRepositories;
using backend.IRepository;

using Microsoft.EntityFrameworkCore;
namespace backend.Repositories;

internal sealed class UserRepository(AppDbContext context) : GenericRepository<User>(context), IUserRepository
{
    public async Task<int> GetUsersCountAsync()
    {
        return await QueryAsNoTracking()
            .Where(x => x.Role != null && x.Role.Name != "Admin")
            .CountAsync()
            .ConfigureAwait(false);
    }
    public async Task<IReadOnlyCollection<UserResponseDto>> GetAllUsersAsync(int pageNumber, int pageSize)
    {
        return await QueryAsNoTracking()
            .Include(x => x.Role)
            .Include(x => x.Branch)
            .Include(x => x.Department)
            .Include(x => x.Position)
            .Where(x => x.Role != null && x.Role.Name != "Admin")
            .OrderBy(x => x.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new UserResponseDto
            {
                UserId = x.Id,
                Name = x.Name,
                Email = x.Email,
                DOB = x.DOB,
                RoleName = x.Role != null ? x.Role.Name : string.Empty,
                BranchName = x.Branch != null ? x.Branch.Name : string.Empty,
                DepartmentName = x.Department != null ? x.Department.Name : string.Empty,
                PositionName = x.Position != null ? x.Position.Name : string.Empty
            })
            .ToListAsync()
            .ConfigureAwait(false);
    }
    public async Task<IReadOnlyCollection<UserResponseDto>> GetUserBySearchAsync(string searchTerm)
    {
        return await QueryAsNoTracking()
            .Include(x => x.Role)
            .Include(x => x.Branch)
            .Include(x => x.Department)
            .Include(x => x.Position)
            .Where(x => EF.Functions.Like(x.Name, $"%{searchTerm}%") || EF.Functions.Like(x.Email, $"%{searchTerm}%"))
            .Select(x => new UserResponseDto
            {
                UserId = x.Id,
                Name = x.Name,
                Email = x.Email,
                DOB = x.DOB,
                RoleName = x.Role != null ? x.Role.Name : string.Empty,
                BranchName = x.Branch != null ? x.Branch.Name : string.Empty,
                DepartmentName = x.Department != null ? x.Department.Name : string.Empty,
                PositionName = x.Position != null ? x.Position.Name : string.Empty
            })
            .ToListAsync()
            .ConfigureAwait(false);
    }
    public async Task<UserResponseDto?> GetUserByIdAsync(Guid id)
    {
        return await QueryAsNoTracking()
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
                RoleName = x.Role != null ? x.Role.Name : string.Empty,
                BranchName = x.Branch != null ? x.Branch.Name : string.Empty,
                DepartmentName = x.Department != null ? x.Department.Name : string.Empty,
                PositionName = x.Position != null ? x.Position.Name : string.Empty
            })
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }
    public async Task<IReadOnlyCollection<UserResponseDto>> GetUsersByFilterAsync(UserFilterDto dto)
    {
        var query = QueryAsNoTracking()
            .Include(x => x.Role)
            .Include(x => x.Branch)
            .Include(x => x.Department)
            .Include(x => x.Position)
            .Where(x => x.Role != null && x.Role.Name != "Admin");
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
        return await query
            .Select(x => new UserResponseDto
            {
                UserId = x.Id,
                Name = x.Name,
                Email = x.Email,
                DOB = x.DOB,
                RoleName = x.Role != null ? x.Role.Name : string.Empty,
                BranchName = x.Branch != null ? x.Branch.Name : string.Empty,
                DepartmentName = x.Department != null ? x.Department.Name : string.Empty,
                PositionName = x.Position != null ? x.Position.Name : string.Empty
            })
            .ToListAsync()
            .ConfigureAwait(false);
    }
}
