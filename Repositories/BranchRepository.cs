using backend.Data;
using backend.Dto.BranchDtos;
using backend.Entities;
using backend.IRepository;

using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

internal sealed class BranchRepository(AppDbContext context) : IBranchRepository
{
    public async Task<bool> BranchExistsAsync(string? name, CancellationToken cancellationToken)
    {
        return await context.Branches
            .AsNoTracking()
            .AnyAsync(
                x => string.Equals(
                    x.Name,
                    name,
                    StringComparison.OrdinalIgnoreCase),
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Branch?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => await context.Branches.FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);

    public async Task AddAsync(Branch branch, CancellationToken cancellationToken)
    {
        await context.Branches
            .AddAsync(branch, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<BranchResponseDto>> GetAllBranchesAsync()
    {
        return await context.Branches
            .AsNoTracking()
            .Select(x => new BranchResponseDto
            {
                Id = x.Id,
                Name = x.Name,
                Location = x.Location,
                TotalUsers = context.Users.Count(u => u.BranchId == x.Id)
            })
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task<BranchResponseDto?> GetBranchByIdAsync(Guid id)
    {
        return await context.Branches
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new BranchResponseDto
            {
                Id = x.Id,
                Name = x.Name,
                Location = x.Location,
                TotalUsers = context.Users.Count(u => u.BranchId == x.Id)
            })
            .FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<BranchUserResponseDto>> GetBranchUsersAsync(Guid branchId)
    {
        return await context.Users
            .AsNoTracking()
            .Where(x => x.BranchId == branchId)
            .Select(x => new BranchUserResponseDto
            {
                UserId = x.Id,
                Name = x.Name ?? string.Empty,
                Email = x.Email ?? string.Empty,
                DOB = x.DOB,
                BranchName = x.Branch != null ? x.Branch.Name : string.Empty,
                DepartmentName = x.Department != null ? x.Department.Name : string.Empty,
                PositionName = x.Position != null ? x.Position.Name : string.Empty,
                RoleName = x.Role != null ? x.Role.Name : string.Empty
            })
            .ToListAsync()
            .ConfigureAwait(false);
    }
}
