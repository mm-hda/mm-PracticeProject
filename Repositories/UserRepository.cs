using backend.Data;
using backend.Dto.UserDtos;
using backend.Entities;
using backend.GenericRepositories;
using backend.IRepository;

namespace backend.Repositories;

internal sealed class UserRepository(
    AppDbContext context,
    IGenericRepository<Role> roleRepository,
    IGenericRepository<Branch> branchRepository,
    IGenericRepository<Department> departmentRepository,
    IGenericRepository<Position> positionRepository)
    : GenericRepository<User>(context), IUserRepository
{
    public async Task<int> GetUsersCountAsync(CancellationToken cancellationToken)
    {
        var roles = await roleRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var adminRole = roles.FirstOrDefault(x =>
            string.Equals(x.Name, "Admin", StringComparison.OrdinalIgnoreCase));

        if (adminRole is null)
        {
            return await CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        var users = await FindAsync(x => x.RoleId != adminRole.Id, cancellationToken).ConfigureAwait(false);

        return users.Count;
    }

    public async Task<IReadOnlyCollection<UserResponseDto>> GetAllUsersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var roles = await roleRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var adminRole = roles.FirstOrDefault(x =>
            string.Equals(x.Name, "Admin", StringComparison.OrdinalIgnoreCase));

        var users = await GetAllAsync(cancellationToken).ConfigureAwait(false);

        if (adminRole is not null)
        {
            users = users.Where(x => x.RoleId != adminRole.Id).ToList();
        }

        users = users.OrderBy(x => x.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return await MapUsersAsync(users, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<UserResponseDto>> GetUserBySearchAsync(string searchTerm, CancellationToken cancellationToken)
    {
        var users = await GetAllAsync(cancellationToken).ConfigureAwait(false);

        var filteredUsers = users
            .Where(x =>
                x.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                x.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.Name)
            .ToList();

        return await MapUsersAsync(filteredUsers, cancellationToken).ConfigureAwait(false);
    }

    public async Task<UserResponseDto?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);

        if (user is null)
        {
            return null;
        }

        return (await MapUsersAsync([user], cancellationToken).ConfigureAwait(false)).FirstOrDefault();
    }

    public async Task<IReadOnlyCollection<UserResponseDto>> GetUsersByFilterAsync(UserFilterDto dto, CancellationToken cancellationToken)
    {
        var roles = await roleRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var adminRole = roles.FirstOrDefault(x =>
            string.Equals(x.Name, "Admin", StringComparison.OrdinalIgnoreCase));

        IEnumerable<User> users = await GetAllAsync(cancellationToken).ConfigureAwait(false);

        if (adminRole is not null)
        {
            users = users.Where(x => x.RoleId != adminRole.Id);
        }

        if (dto.RoleId.HasValue)
        {
            users = users.Where(x => x.RoleId == dto.RoleId.Value);
        }

        if (dto.BranchId.HasValue)
        {
            users = users.Where(x => x.BranchId == dto.BranchId.Value);
        }

        if (dto.DepartmentId.HasValue)
        {
            users = users.Where(x => x.DepartmentId == dto.DepartmentId.Value);
        }

        if (dto.PositionId.HasValue)
        {
            users = users.Where(x => x.PositionId == dto.PositionId.Value);
        }

        return await MapUsersAsync(users.OrderBy(x => x.Name).ToList(), cancellationToken).ConfigureAwait(false);
    }

    private async Task<IReadOnlyCollection<UserResponseDto>> MapUsersAsync(IReadOnlyCollection<User> users, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(users);

        var roles = await roleRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var branches = await branchRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var departments = await departmentRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var positions = await positionRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var roleDictionary = roles.ToDictionary(x => x.Id, x => x.Name);
        var branchDictionary = branches.ToDictionary(x => x.Id, x => x.Name);
        var departmentDictionary = departments.ToDictionary(x => x.Id, x => x.Name);
        var positionDictionary = positions.ToDictionary(x => x.Id, x => x.Name);

        return users.Select(user => new UserResponseDto
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            DOB = user.DOB,
            RoleName = roleDictionary.TryGetValue(user.RoleId, out var roleName) ? roleName : string.Empty,
            BranchName = branchDictionary.TryGetValue(user.BranchId, out var branchName) ? branchName : string.Empty,
            DepartmentName = departmentDictionary.TryGetValue(user.DepartmentId, out var departmentName) ? departmentName : string.Empty,
            PositionName = positionDictionary.TryGetValue(user.PositionId, out var positionName) ? positionName : string.Empty
        }).ToList();
    }
}
