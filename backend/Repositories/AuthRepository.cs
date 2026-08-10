using backend.Data;
using backend.Entities;
using backend.GenericRepositories;
using backend.IRepository;

namespace backend.Repositories;

internal sealed class AuthRepository(
    AppDbContext context,
    IGenericRepository<Role> roleRepository,
    IGenericRepository<Branch> branchRepository,
    IGenericRepository<Department> departmentRepository,
    IGenericRepository<Position> positionRepository)
    : GenericRepository<User>(context), IAuthRepository
{
    public async Task<User?> GetUserByEmailWithDetailsAsync(string? email, CancellationToken cancellationToken)
    {
        var user = await FirstOrDefaultAsync(x => x.Email == email, cancellationToken).ConfigureAwait(false);

        if (user is null)
        {
            return null;
        }

        var role = await roleRepository.FirstOrDefaultAsync(x => x.Id == user.RoleId, cancellationToken).ConfigureAwait(false);

        user.Role = role;

        var branch = await branchRepository.FirstOrDefaultAsync(x => x.Id == user.BranchId, cancellationToken).ConfigureAwait(false);
        user.Branch = branch;

        return user;
    }

    public async Task<bool> EmailExistsAsync(string? email, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        var user = await CountAsync(x => x.Email == email, cancellationToken).ConfigureAwait(false);

        return user > 0;
    }

    public async Task<bool> BranchExistsAsync(Guid branchId, CancellationToken cancellationToken)
    {
        var branch = await branchRepository.CountAsync(x => x.Id == branchId, cancellationToken).ConfigureAwait(false);

        return branch > 0;
    }

    public async Task<bool> DepartmentExistsAsync(Guid departmentId, CancellationToken cancellationToken)
    {
        var department = await departmentRepository.CountAsync(x => x.Id == departmentId, cancellationToken).ConfigureAwait(false);

        return department > 0;
    }

    public async Task<bool> PositionExistsAsync(Guid positionId, Guid departmentId, CancellationToken cancellationToken)
    {
        var positions = await positionRepository.CountAsync(x => x.Id == positionId && x.DepartmentId == departmentId, cancellationToken).ConfigureAwait(false);

        return positions > 0;
    }

    public async Task<bool> RoleExistsAsync(Guid roleId, CancellationToken cancellationToken)
    {
        var roles = await roleRepository.CountAsync(x => x.Id == roleId, cancellationToken).ConfigureAwait(false);

        return roles > 0;
    }

    public async Task AddUserAsync(User user, CancellationToken cancellationToken) => await AddAsync(user, cancellationToken).ConfigureAwait(false);
}
