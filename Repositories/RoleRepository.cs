using backend.Data;
using backend.Dto.RoleDtos;
using backend.Entities;
using backend.GenericRepositories;
using backend.IRepository;

namespace backend.Repositories;

internal sealed class RoleRepository(
    AppDbContext context,
    IGenericRepository<User> userRepository,
    IGenericRepository<Department> departmentRepository,
    IGenericRepository<Position> positionRepository)
    : GenericRepository<Role>(context), IRoleRepository
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

    public async Task<RoleResponseDto?> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var role = await FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);

        if (role is null)
        {
            return null;
        }

        return new RoleResponseDto
        {
            Id = role.Id,
            Name = role.Name
        };
    }

    public async Task<IReadOnlyCollection<RoleUserResponseDto>> GetUsersByRoleAsync(Guid roleId, CancellationToken cancellationToken)
    {
        var roleTask = FirstOrDefaultAsync(x => x.Id == roleId, cancellationToken);

        var departmentsTask = departmentRepository.GetAllAsync(cancellationToken);
        var positionsTask = positionRepository.GetAllAsync(cancellationToken);
        var usersTask = userRepository.FindAsync(x => x.RoleId == roleId, cancellationToken);

        await Task.WhenAll(
            roleTask,
            departmentsTask,
            positionsTask,
            usersTask).ConfigureAwait(false);

        var role = await roleTask.ConfigureAwait(false);

        if (role is null)
        {
            return Array.Empty<RoleUserResponseDto>();
        }

        var departmentDictionary = (await departmentsTask.ConfigureAwait(false)).ToDictionary(x => x.Id, x => x.Name);

        var positionDictionary = (await positionsTask.ConfigureAwait(false)).ToDictionary(x => x.Id, x => x.Name);

        var roleUsers = await usersTask.ConfigureAwait(false);

        return roleUsers.Select(x => new RoleUserResponseDto
        {
            UserId = x.Id,
            Name = x.Name,
            Email = x.Email,
            RoleName = role.Name,
            DepartmentName = departmentDictionary.TryGetValue(x.DepartmentId, out var departmentName) ? departmentName : string.Empty,
            PositionName = positionDictionary.TryGetValue(x.PositionId, out var positionName) ? positionName : string.Empty,
        }).ToList();
    }
}
