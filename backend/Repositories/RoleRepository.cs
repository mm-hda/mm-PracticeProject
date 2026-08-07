using backend.Data;
using backend.Dto.RoleDtos;
using backend.Entities;
using backend.GenericRepositories;
using backend.IRepository;

namespace backend.Repositories;

internal sealed class RoleRepository(AppDbContext context) : GenericRepository<Role>(context), IRoleRepository
{
    public async Task<bool> RoleExistsAsync(string? name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        return await FirstOrDefaultAsync(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase), cancellationToken).ConfigureAwait(false) is not null;

    }

    public async Task AddRoleAsync(Role role, CancellationToken cancellationToken) => await AddAsync(role, cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyCollection<RoleResponseDto>> GetAllRolesAsync(CancellationToken cancellationToken)
    {
        var roles = await GetAllAsync(cancellationToken).ConfigureAwait(false);

        return roles.Select(x => new RoleResponseDto
        {
            Id = x.Id,
            Name = x.Name
        }).ToList();
    }
}
