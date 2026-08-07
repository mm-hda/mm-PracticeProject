using backend.Dto.RoleDtos;
using backend.Entities;
using backend.GenericRepositories;

namespace backend.IRepository;

public interface IRoleRepository : IGenericRepository<Role>
{
    Task<bool> RoleExistsAsync(string? name, CancellationToken cancellationToken);
    Task AddRoleAsync(Role role, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<RoleResponseDto>> GetAllRolesAsync(CancellationToken cancellationToken);
}
