using backend.Data;
using backend.Dto.PositionDtos;
using backend.Entities;
using backend.IRepository;

using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

internal sealed class PositionRepository(AppDbContext context) : IPositionRepository
{
    public async Task<bool> DepartmentExistsAsync(Guid departmentId, CancellationToken cancellationToken)
    {
        return await context.Departments
            .AnyAsync(x => x.Id == departmentId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> PositionExistsAsync(string? name, Guid departmentId, CancellationToken cancellationToken)
    {
        return await context.Positions
            .AnyAsync(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                           x.DepartmentId == departmentId,
                      cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task AddAsync(Position position, CancellationToken cancellationToken)
    {
        await context.Positions
            .AddAsync(position, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Position?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Positions
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> DuplicatePositionExistsAsync(Guid positionId, string? name, Guid departmentId, CancellationToken cancellationToken)
    {
        return await context.Positions
            .AnyAsync(x => x.Id != positionId &&
                           x.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                           x.DepartmentId == departmentId,
                      cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<PositionResponseDto>> GetAllPositionsAsync()
    {
        return await context.Positions
            .AsNoTracking()
            .Include(x => x.Department)
            .Select(x => new PositionResponseDto
            {
                Id = x.Id,
                Name = x.Name,
                DepartmentId = x.DepartmentId,
                DepartmentName = x.Department != null ? x.Department.Name : "",
                TotalUsers = context.Users.Count(u => u.PositionId == x.Id)
            })
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<PositionResponseDto?> GetPositionByIdAsync(Guid id)
    {
        return await context.Positions
            .AsNoTracking()
            .Include(x => x.Department)
            .Where(x => x.Id == id)
            .Select(x => new PositionResponseDto
            {
                Id = x.Id,
                Name = x.Name,
                DepartmentId = x.DepartmentId,
                DepartmentName = x.Department != null ? x.Department.Name : "",
                TotalUsers = context.Users.Count(u => u.PositionId == x.Id)
            })
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<PositionResponseDto>> GetPositionsByDepartmentAsync(Guid departmentId)
    {
        return await context.Positions
            .AsNoTracking()
            .Include(x => x.Department)
            .Where(x => x.DepartmentId == departmentId)
            .Select(x => new PositionResponseDto
            {
                Id = x.Id,
                Name = x.Name,
                DepartmentId = x.DepartmentId,
                DepartmentName = x.Department != null ? x.Department.Name : "",
                TotalUsers = context.Users.Count(u => u.PositionId == x.Id)
            })
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<bool> PositionExistsAsync(Guid positionId)
    {
        return await context.Positions
            .AnyAsync(x => x.Id == positionId)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<PositionUserResponseDto>> GetPositionUsersAsync(Guid positionId)
    {
        return await context.Users
            .AsNoTracking()
            .Include(x => x.Branch)
            .Include(x => x.Department)
            .Include(x => x.Position)
            .Include(x => x.Role)
            .Where(x => x.PositionId == positionId)
            .Select(x => new PositionUserResponseDto
            {
                UserId = x.Id,
                Name = x.Name ?? "",
                Email = x.Email ?? "",
                DOB = x.DOB,
                BranchName = x.Branch != null ? x.Branch.Name : "",
                DepartmentName = x.Department != null ? x.Department.Name : "",
                PositionName = x.Position != null ? x.Position.Name : "",
                RoleName = x.Role != null ? x.Role.Name : ""
            })
            .ToListAsync()
            .ConfigureAwait(false);
    }
}
