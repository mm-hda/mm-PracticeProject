using backend.Data;
using backend.Dto.UserDtos;
using backend.IRepository;

using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

internal sealed class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task<int> GetUsersCountAsync(CancellationToken cancellationToken)
    {
        return await context.Users.AsNoTracking()
            .Include(x => x.Role)
            .Include(x => x.Branch)
            .Include(x => x.Department)
            .Include(x => x.Position)
            .Where(x => x.Role != null && x.Role.Name != "Admin")
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<UserResponseDto>> GetAllUsersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        return await context.Users.AsNoTracking()
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
                RoleName = x.Role != null ? x.Role.Name : "",
                BranchName = x.Branch != null ? x.Branch.Name : "",
                DepartmentName = x.Department != null ? x.Department.Name : "",
                PositionName = x.Position != null ? x.Position.Name : ""
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<UserResponseDto>> GetUserBySearchAsync(string searchTerm)
    {
        return await context.Users.AsNoTracking()
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
                RoleName = x.Role != null ? x.Role.Name : "",
                BranchName = x.Branch != null ? x.Branch.Name : "",
                DepartmentName = x.Department != null ? x.Department.Name : "",
                PositionName = x.Position != null ? x.Position.Name : ""
            })
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<UserResponseDto?> GetUserByIdAsync(Guid id)
    {
        return await context.Users.AsNoTracking()
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
            })
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<UserResponseDto>> GetUsersByFilterAsync(UserFilterDto dto)
    {
        var query = context.Users.AsNoTracking()
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

        return await query.Select(x => new UserResponseDto
        {
            UserId = x.Id,
            Name = x.Name,
            Email = x.Email,
            DOB = x.DOB,
            RoleName = x.Role != null ? x.Role.Name : "",
            BranchName = x.Branch != null ? x.Branch.Name : "",
            DepartmentName = x.Department != null ? x.Department.Name : "",
            PositionName = x.Position != null ? x.Position.Name : ""
        })
            .ToListAsync()
            .ConfigureAwait(false);
    }
}
