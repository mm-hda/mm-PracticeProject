using backend.Entities;

namespace backend.IRepository;

public interface IAuthRepository
{
    Task<User?> GetUserByEmailWithDetailsAsync(string? email, CancellationToken cancellationToken);

    Task<bool> EmailExistsAsync(string? email, CancellationToken cancellationToken);

    Task<bool> BranchExistsAsync(Guid branchId, CancellationToken cancellationToken);

    Task<bool> DepartmentExistsAsync(Guid departmentId, CancellationToken cancellationToken);

    Task<bool> PositionExistsAsync(Guid positionId, Guid departmentId, CancellationToken cancellationToken);

    Task<bool> RoleExistsAsync(Guid roleId, CancellationToken cancellationToken);

    Task AddUserAsync(User user, CancellationToken cancellationToken);
}
