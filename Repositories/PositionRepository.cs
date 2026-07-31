using backend.Data;
using backend.Dto.PositionDtos;
using backend.Entities;
using backend.GenericRepositories;
using backend.IRepository;

namespace backend.Repositories;

internal sealed class PositionRepository(
    AppDbContext context,
    IGenericRepository<Department> departmentRepository,
    IGenericRepository<User> userRepository,
    IGenericRepository<Role> roleRepository,
    IGenericRepository<Branch> branchRepository)
    : GenericRepository<Position>(context), IPositionRepository
{
    public async Task<bool> DepartmentExistsAsync(Guid departmentId, CancellationToken cancellationToken) => await departmentRepository.AnyAsync(x => x.Id == departmentId, cancellationToken).ConfigureAwait(false);

    public async Task<bool> PositionExistsAsync(string? name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        return await AnyAsync(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase), cancellationToken).ConfigureAwait(false);
    }

    public async Task AddPositionAsync(Position position, CancellationToken cancellationToken) => await AddAsync(position, cancellationToken).ConfigureAwait(false);

    public async Task<Position?> PositionByIdAsync(Guid id, CancellationToken cancellationToken) => await FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);

    public async Task<bool> DuplicatePositionExistsAsync(Guid positionId, string? name, Guid departmentId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        var positions = await FindAsync(x => x.DepartmentId == departmentId, cancellationToken).ConfigureAwait(false);

        return positions.Any(x =>
            x.Id != positionId &&
            x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IReadOnlyCollection<PositionResponseDto>> GetAllPositionsAsync(CancellationToken cancellationToken)
    {
        var positions = await GetAllAsync(cancellationToken).ConfigureAwait(false);
        var departments = await departmentRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var users = await userRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var departmentDictionary = departments.ToDictionary(x => x.Id, x => x.Name);

        return positions.Select(position => new PositionResponseDto
        {
            Id = position.Id,
            Name = position.Name,
            DepartmentId = position.DepartmentId,
            DepartmentName = departmentDictionary.TryGetValue(position.DepartmentId, out var departmentName)
                ? departmentName
                : string.Empty,
            TotalUsers = users.Count(x => x.PositionId == position.Id)
        }).ToList();
    }

    public async Task<PositionResponseDto?> GetPositionByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var position = await FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);

        if (position is null)
        {
            return null;
        }

        var departments = await departmentRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var users = await userRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var departmentDictionary = departments.ToDictionary(x => x.Id, x => x.Name);

        return new PositionResponseDto
        {
            Id = position.Id,
            Name = position.Name,
            DepartmentId = position.DepartmentId,
            DepartmentName = departmentDictionary.TryGetValue(position.DepartmentId, out var departmentName)
                ? departmentName
                : string.Empty,
            TotalUsers = users.Count(x => x.PositionId == position.Id)
        };
    }

    public async Task<IReadOnlyCollection<PositionResponseDto>> GetPositionsByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken)
    {
        var positions = await FindAsync(x => x.DepartmentId == departmentId, cancellationToken).ConfigureAwait(false);
        var departments = await departmentRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var users = await userRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var departmentDictionary = departments.ToDictionary(x => x.Id, x => x.Name);

        return positions.Select(position => new PositionResponseDto
        {
            Id = position.Id,
            Name = position.Name,
            DepartmentId = position.DepartmentId,
            DepartmentName = departmentDictionary.TryGetValue(position.DepartmentId, out var departmentName)
                ? departmentName
                : string.Empty,
            TotalUsers = users.Count(x => x.PositionId == position.Id)
        }).ToList();
    }

    public async Task<bool> PositionExistsAsync(Guid positionId, CancellationToken cancellationToken) => await AnyAsync(x => x.Id == positionId, cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyCollection<PositionUserResponseDto>> GetPositionUsersAsync(Guid positionId, CancellationToken cancellationToken)
    {
        var users = await userRepository.FindAsync(x => x.PositionId == positionId, cancellationToken).ConfigureAwait(false);

        var branches = await branchRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var departments = await departmentRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var positions = await GetAllAsync(cancellationToken).ConfigureAwait(false);
        var roles = await roleRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var branchDictionary = branches.ToDictionary(x => x.Id, x => x.Name);
        var departmentDictionary = departments.ToDictionary(x => x.Id, x => x.Name);
        var positionDictionary = positions.ToDictionary(x => x.Id, x => x.Name);
        var roleDictionary = roles.ToDictionary(x => x.Id, x => x.Name);

        return users.Select(user => new PositionUserResponseDto
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            DOB = user.DOB,
            BranchName = branchDictionary.TryGetValue(user.BranchId, out var branchName) ? branchName : string.Empty,
            DepartmentName = departmentDictionary.TryGetValue(user.DepartmentId, out var departmentName) ? departmentName : string.Empty,
            PositionName = positionDictionary.TryGetValue(user.PositionId, out var positionName) ? positionName : string.Empty,
            RoleName = roleDictionary.TryGetValue(user.RoleId, out var roleName) ? roleName : string.Empty
        }).ToList();
    }
}
