using backend.Data;
using backend.Dto.BranchDtos;
using backend.Entities;
using backend.GenericRepositories;
using backend.IRepository;

namespace backend.Repositories;

internal sealed class BranchRepository(
    AppDbContext context,
    IGenericRepository<User> userRepository,
    IGenericRepository<Role> roleRepository,
    IGenericRepository<Department> departmentRepository,
    IGenericRepository<Position> positionRepository)
    : GenericRepository<Branch>(context), IBranchRepository
{
    public async Task<bool> BranchExistsAsync(string? name, CancellationToken cancellationToken)
    {
        var branches = await GetAllAsync(cancellationToken).ConfigureAwait(false);

        return branches.Any(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<Branch?> BranchByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var branches = await GetAllAsync(cancellationToken).ConfigureAwait(false);

        return branches.FirstOrDefault(x => x.Id == id);
    }

    public async Task AddBranchAsync(Branch branch, CancellationToken cancellationToken)
        => await AddAsync(branch, cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyCollection<BranchResponseDto>> GetAllBranchesAsync(CancellationToken cancellationToken)
    {
        var branches = await GetAllAsync(cancellationToken).ConfigureAwait(false);
        var users = await userRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        return branches.Select(branch => new BranchResponseDto
        {
            Id = branch.Id,
            Name = branch.Name,
            Location = branch.Location,
            TotalUsers = users.Count(x => x.BranchId == branch.Id)
        }).ToList();
    }

    public async Task<BranchResponseDto?> GetBranchByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var branches = await GetAllAsync(cancellationToken).ConfigureAwait(false);
        var branch = branches.FirstOrDefault(x => x.Id == id);

        if (branch is null)
        {
            return null;
        }

        var users = await userRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        return new BranchResponseDto
        {
            Id = branch.Id,
            Name = branch.Name,
            Location = branch.Location,
            TotalUsers = users.Count(x => x.BranchId == branch.Id)
        };
    }

    public async Task<IReadOnlyCollection<BranchUserResponseDto>> GetBranchUsersAsync(Guid branchId, CancellationToken cancellationToken)
    {
        var users = await userRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var branchUsers = users.Where(x => x.BranchId == branchId).ToList();

        var branches = await GetAllAsync(cancellationToken).ConfigureAwait(false);
        var departments = await departmentRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var positions = await positionRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var roles = await roleRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var branchDictionary = branches.ToDictionary(x => x.Id, x => x.Name);
        var departmentDictionary = departments.ToDictionary(x => x.Id, x => x.Name);
        var positionDictionary = positions.ToDictionary(x => x.Id, x => x.Name);
        var roleDictionary = roles.ToDictionary(x => x.Id, x => x.Name);

        return branchUsers.Select(user => new BranchUserResponseDto
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
