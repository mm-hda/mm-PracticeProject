using backend.Data;
using backend.Dto.BranchDtos;
using backend.Entities;
using backend.IRepository;
using backend.GenericRepositories;

using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

internal sealed class BranchRepository(AppDbContext context) : GenericRepository<Branch>(context), IBranchRepository
{
    public async Task<bool> BranchExistsAsync(string? name, CancellationToken cancellationToken)
    {
        return await DbSet
            .AsNoTracking()
            .AnyAsync(
                x => string.Equals(
                    x.Name,
                    name,
                    StringComparison.OrdinalIgnoreCase),
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Branch?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => await DbSet.FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);

    public async Task AddBranchAsync(Branch branch, CancellationToken cancellationToken)
    {
        await DbSet
            .AddAsync(branch, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<BranchResponseDto>> GetAllBranchesAsync()
    {
        return await DbSet
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
        return await DbSet
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
