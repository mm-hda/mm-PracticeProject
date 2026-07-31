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
        => await FirstOrDefaultAsync(x => x.Email == email, cancellationToken).ConfigureAwait(false);

    public async Task<bool> EmailExistsAsync(
        string? email,
        CancellationToken cancellationToken)
        => await AnyAsync(x => x.Email == email, cancellationToken).ConfigureAwait(false);

    public async Task<bool> BranchExistsAsync(Guid branchId, CancellationToken cancellationToken)
        => await branchRepository.AnyAsync(x => x.Id == branchId, cancellationToken).ConfigureAwait(false);

    public async Task<bool> DepartmentExistsAsync(
        Guid departmentId,
        CancellationToken cancellationToken)
        => await departmentRepository.AnyAsync(x => x.Id == departmentId, cancellationToken).ConfigureAwait(false);

    public async Task<bool> PositionExistsAsync(
        Guid positionId,
        Guid departmentId,
        CancellationToken cancellationToken)
        => await positionRepository.AnyAsync(
            x => x.Id == positionId &&
                 x.DepartmentId == departmentId,
            cancellationToken).ConfigureAwait(false);

    public async Task<bool> RoleExistsAsync(
        Guid roleId,
        CancellationToken cancellationToken)
        => await roleRepository.AnyAsync(x => x.Id == roleId, cancellationToken).ConfigureAwait(false);

    public async Task AddUserAsync(
        User user,
        CancellationToken cancellationToken)
        => await AddAsync(user, cancellationToken).ConfigureAwait(false);
}
