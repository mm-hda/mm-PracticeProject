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
    IGenericRepository<Position> positionRepository,
    IGenericRepository<Branch> branchRepository)
    : GenericRepository<Role>(context), IRoleRepository
{
    public async Task<bool> RoleExistsAsync(string? name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        var roles = await GetAllAsync(cancellationToken).ConfigureAwait(false);

        return roles.Any(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
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
        var roles = await GetAllAsync(cancellationToken).ConfigureAwait(false);
        var role = roles.FirstOrDefault(x => x.Id == id);

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
        var roles = await GetAllAsync(cancellationToken).ConfigureAwait(false);
        var departments = await departmentRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var positions = await positionRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var branches = await branchRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var users = await userRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var roleUsers = users.Where(x => x.RoleId == roleId).ToList();

        var roleDictionary = roles.ToDictionary(x => x.Id, x => x.Name);
        var departmentDictionary = departments.ToDictionary(x => x.Id, x => x.Name);
        var positionDictionary = positions.ToDictionary(x => x.Id, x => x.Name);
        var branchDictionary = branches.ToDictionary(x => x.Id, x => x.Name);

        return roleUsers.Select(x => new RoleUserResponseDto
        {
            UserId = x.Id,
            Name = x.Name,
            Email = x.Email,
            RoleName = roleDictionary.TryGetValue(x.RoleId, out var roleName) ? roleName : string.Empty,
            DepartmentName = departmentDictionary.TryGetValue(x.DepartmentId, out var departmentName) ? departmentName : string.Empty,
            PositionName = positionDictionary.TryGetValue(x.PositionId, out var positionName) ? positionName : string.Empty,
            BranchName = branchDictionary.TryGetValue(x.BranchId, out var branchName) ? branchName : string.Empty
        }).ToList();
    }
}
