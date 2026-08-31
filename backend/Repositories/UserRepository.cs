using backend.Data;
using backend.Dto.UserDtos;
using backend.Entities;
using backend.GenericRepositories;
using backend.IRepository;

using Microsoft.EntityFrameworkCore;

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
        var adminRole = await roleRepository.FirstOrDefaultAsync(x => x.Name == "Admin", cancellationToken).ConfigureAwait(false);

        if (adminRole is null)
        {
            return 0;
        }

        return await CountAsync(x => x.RoleId != adminRole.Id, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<UserResponseDto>> GetAllUsersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var adminRole = await roleRepository.FirstOrDefaultAsync(x => x.Name == "Admin", cancellationToken).ConfigureAwait(false);

        var users = await GetPagedAsync(
            x => adminRole == null || x.RoleId != adminRole.Id,
            x => x.Name,
            pageNumber,
            pageSize,
            cancellationToken).ConfigureAwait(false);

        return await MapUsersAsync(users, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<UserResponseDto>> GetUserBySearchAsync(string searchTerm, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return Array.Empty<UserResponseDto>();
        }

        var search = searchTerm.Trim();

        var users = await GetAsync(
            x =>
                x.Name.Contains(search) ||
                x.Email.Contains(search),
            x => x.Name,
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);

        return await MapUsersAsync(
            users,
            cancellationToken
        ).ConfigureAwait(false);
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
        var adminRole = await roleRepository.FirstOrDefaultAsync(x => x.Name == "Admin", cancellationToken).ConfigureAwait(false);

        IEnumerable<User> users = await FindAsync(x => adminRole == null || x.RoleId != adminRole.Id, cancellationToken).ConfigureAwait(false);

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

    public async Task<IReadOnlyCollection<UserResponseDto>> GetManagersAsync(CancellationToken cancellationToken)
    {
        var managerRole = await roleRepository.FirstOrDefaultAsync(x => x.Name == "Manager", cancellationToken).ConfigureAwait(false);

        if (managerRole is null)
        {
            return Array.Empty<UserResponseDto>();
        }

        var managers = await FindAsync(x => x.RoleId == managerRole.Id, cancellationToken).ConfigureAwait(false);

        return await MapUsersAsync(managers, cancellationToken).ConfigureAwait(false);
    }
}
