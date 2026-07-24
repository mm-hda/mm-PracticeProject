using backend.Data;
using backend.Entities;
using backend.IRepository;

using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

internal sealed class AuthRepository(AppDbContext context) : IAuthRepository
{
    public async Task<User?> GetUserByEmailWithDetailsAsync(string? email, CancellationToken cancellationToken)
    {
        return await context.Users
            .Include(x => x.Role)
            .Include(x => x.Branch)
            .Include(x => x.Department)
            .Include(x => x.Position)
            .FirstOrDefaultAsync(
                x => x.Email == email,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> EmailExistsAsync(string? email, CancellationToken cancellationToken)
    {
        return await context.Users
            .AsNoTracking()
            .AnyAsync(
                x => x.Email == email,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> BranchExistsAsync(Guid branchId, CancellationToken cancellationToken)
    {
        return await context.Branches
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == branchId,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> DepartmentExistsAsync(Guid departmentId, CancellationToken cancellationToken)
    {
        return await context.Departments
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == departmentId,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> PositionExistsAsync(Guid positionId, Guid departmentId, CancellationToken cancellationToken)
    {
        return await context.Positions
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == positionId &&
                     x.DepartmentId == departmentId,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> RoleExistsAsync(Guid roleId, CancellationToken cancellationToken)
    {
        return await context.Roles
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == roleId,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task AddUserAsync(User user, CancellationToken cancellationToken)
    {
        await context.Users
            .AddAsync(user, cancellationToken)
            .ConfigureAwait(false);
    }
}
