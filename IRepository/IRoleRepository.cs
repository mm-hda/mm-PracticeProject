using backend.Dto.RoleDtos;
using backend.Entities;

namespace backend.IRepository;

public interface IRoleRepository
{
    Task<bool> RoleExistsAsync(string? name, CancellationToken cancellationToken);
    Task AddAsync(Role role, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<RoleResponseDto>> GetAllRolesAsync();
    Task<RoleResponseDto?> GetRoleByIdAsync(Guid id);
    Task<IReadOnlyCollection<RoleUserResponseDto>> GetUsersByRoleAsync(Guid roleId);
}
