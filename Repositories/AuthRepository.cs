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
        var users = await GetAllAsync(cancellationToken).ConfigureAwait(false);

        return users.FirstOrDefault(x => string.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<bool> EmailExistsAsync(string? email, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        var users = await GetAllAsync(cancellationToken).ConfigureAwait(false);

        return users.Any(x => string.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<bool> BranchExistsAsync(Guid branchId, CancellationToken cancellationToken)
    {
        var branches = await branchRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        return branches.Any(x => x.Id == branchId);
    }

    public async Task<bool> DepartmentExistsAsync(Guid departmentId, CancellationToken cancellationToken)
    {
        var departments = await departmentRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        return departments.Any(x => x.Id == departmentId);
    }

    public async Task<bool> PositionExistsAsync(Guid positionId, Guid departmentId, CancellationToken cancellationToken)
    {
        var positions = await positionRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        return positions.Any(x => x.Id == positionId && x.DepartmentId == departmentId);
    }

    public async Task<bool> RoleExistsAsync(Guid roleId, CancellationToken cancellationToken)
    {
        var roles = await roleRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        return roles.Any(x => x.Id == roleId);
    }

    public async Task AddUserAsync(User user, CancellationToken cancellationToken)
        => await AddAsync(user, cancellationToken).ConfigureAwait(false);
}
