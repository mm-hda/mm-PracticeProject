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
    public async Task<bool> DepartmentExistsAsync(Guid departmentId, CancellationToken cancellationToken)
    {
        var departments = await departmentRepository.CountAsync(x => x.Id == departmentId, cancellationToken).ConfigureAwait(false);

        return departments > 0;
    }

    public async Task<bool> PositionExistsAsync(string? name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        var positions = await CountAsync(x => x.Name == name, cancellationToken).ConfigureAwait(false);

        return positions > 0;
    }

    public async Task AddPositionAsync(Position position, CancellationToken cancellationToken) => await AddAsync(position, cancellationToken).ConfigureAwait(false);

    public async Task<Position?> PositionByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var positions = await FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);
        return positions;
    }

    public async Task<bool> DuplicatePositionExistsAsync(Guid positionId, string? name, Guid departmentId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        var positions = await CountAsync(x => x.Id == positionId, cancellationToken).ConfigureAwait(false);

        return positions > 0 && await CountAsync(x =>
            x.Id != positionId &&
            x.Name == name, cancellationToken).ConfigureAwait(false) > 0;
    }

    public async Task<IReadOnlyCollection<PositionResponseDto>> GetAllPositionsAsync(CancellationToken cancellationToken)
    {
        var positions = await GetAllAsync(cancellationToken).ConfigureAwait(false);

        var result = new List<PositionResponseDto>();

        foreach (var position in positions)
        {
            var users = await userRepository.CountAsync(x => x.PositionId == position.Id, cancellationToken).ConfigureAwait(false);
            var department = await departmentRepository.FirstOrDefaultAsync(x => x.Id == position.DepartmentId, cancellationToken).ConfigureAwait(false);

            result.Add(new PositionResponseDto
            {
                Id = position.Id,
                Name = position.Name,
                DepartmentId = position.DepartmentId,
                DepartmentName = department?.Name ?? string.Empty,
                TotalUsers = users
            });
        }

        return result;
    }

    public async Task<PositionResponseDto?> GetPositionByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var position = await FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);

        if (position is null)
        {
            return null;
        }

        var department = await departmentRepository.FirstOrDefaultAsync(x => x.Id == position.DepartmentId, cancellationToken).ConfigureAwait(false);
        var users = await userRepository.CountAsync(x => x.PositionId == position.Id, cancellationToken).ConfigureAwait(false);

        return new PositionResponseDto
        {
            Id = position.Id,
            Name = position.Name,
            DepartmentId = position.DepartmentId,
            DepartmentName = department?.Name ?? string.Empty,
            TotalUsers = users
        };
    }

    public async Task<IReadOnlyCollection<PositionResponseDto>> GetPositionsByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken)
    {
        var positions = await FindAsync(x => x.DepartmentId == departmentId, cancellationToken).ConfigureAwait(false);

        var department = await departmentRepository.FirstOrDefaultAsync(x => x.Id == departmentId, cancellationToken).ConfigureAwait(false);

        var users = await userRepository.CountAsync(x => x.PositionId == positions.First().Id, cancellationToken).ConfigureAwait(false);

        var departmentDictionary = new Dictionary<Guid, string>
        {
            { departmentId, department?.Name ?? string.Empty }
        };

        return positions.Select(position => new PositionResponseDto
        {
            Id = position.Id,
            Name = position.Name,
            DepartmentId = position.DepartmentId,
            DepartmentName = department?.Name ?? string.Empty,
            TotalUsers = users
        }).ToList();
    }

    public async Task<bool> PositionExistsAsync(Guid positionId, CancellationToken cancellationToken)
    {
        var positions = await CountAsync(x => x.Id == positionId, cancellationToken).ConfigureAwait(false);

        return positions > 0;
    }

    public async Task<IReadOnlyCollection<PositionUserResponseDto>> GetPositionUsersAsync(Guid positionId, CancellationToken cancellationToken)
    {
        var position = await FirstOrDefaultAsync(x => x.Id == positionId, cancellationToken).ConfigureAwait(false);

        if (position is null)
        {
            return Array.Empty<PositionUserResponseDto>();
        }

        var users = await userRepository.FindAsync(x => x.PositionId == positionId, cancellationToken).ConfigureAwait(false);

        var roles = await roleRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var branches = await branchRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var roleDictionary = roles.ToDictionary(x => x.Id, x => x.Name ?? string.Empty);

        var branchDictionary = branches.ToDictionary(x => x.Id, x => x.Name ?? string.Empty);

        return users.Select(user => new PositionUserResponseDto
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            DOB = user.DOB,

            RoleName = roleDictionary.TryGetValue(user.RoleId, out var roleName) ? roleName : string.Empty,

            BranchName = branchDictionary.TryGetValue(user.BranchId, out var branchName) ? branchName : string.Empty
        }).ToList();
    }
}
