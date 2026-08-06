using backend.Data;
using backend.Dto.BranchDtos;
using backend.Entities;
using backend.GenericRepositories;
using backend.IRepository;

namespace backend.Repositories;

internal sealed class BranchRepository(
    AppDbContext context,
    IGenericRepository<User> userRepository,
    IGenericRepository<Department> departmentRepository,
    IGenericRepository<Position> positionRepository)
    : GenericRepository<Branch>(context), IBranchRepository
{
    public async Task<bool> BranchExistsAsync(string? name, CancellationToken cancellationToken)
    {
        var branches = await CountAsync(x => x.Name == name, cancellationToken).ConfigureAwait(false);

        return branches > 0;
    }

    public async Task<Branch?> BranchByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var branch = await FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);

        return branch;
    }

    public async Task AddBranchAsync(Branch branch, CancellationToken cancellationToken) => await AddAsync(branch, cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyCollection<BranchResponseDto>> GetAllBranchesAsync(CancellationToken cancellationToken)
    {
        var branches = await GetAllAsync(cancellationToken).ConfigureAwait(false);
        var result = new List<BranchResponseDto>();

        foreach (var branch in branches)
        {
            var users = await userRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

            result.Add(new BranchResponseDto
            {
                Id = branch.Id,
                Name = branch.Name,
                Location = branch.Location,
                TotalUsers = users.Count(x => x.BranchId == branch.Id)
            });
        }
        return result;
    }

    public async Task<BranchResponseDto?> GetBranchByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var branch = await FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);

        if (branch is null)
        {
            return null;
        }

        var users = await userRepository.CountAsync(x => x.BranchId == branch.Id, cancellationToken).ConfigureAwait(false);

        return new BranchResponseDto
        {
            Id = branch.Id,
            Name = branch.Name,
            Location = branch.Location,
            TotalUsers = users
        };
    }

    public async Task<IReadOnlyCollection<BranchUserResponseDto>> GetBranchUsersAsync(Guid branchId, CancellationToken cancellationToken)
    {
        var branch = await FirstOrDefaultAsync(x => x.Id == branchId, cancellationToken).ConfigureAwait(false);

        if (branch is null)
        {
            return Array.Empty<BranchUserResponseDto>();
        }

        var users = await userRepository.FindAsync(x => x.BranchId == branchId, cancellationToken).ConfigureAwait(false);

        var departments = await departmentRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var positions = await positionRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var departmentDictionary = departments.ToDictionary(x => x.Id, x => x.Name ?? string.Empty);

        var positionDictionary = positions.ToDictionary(x => x.Id, x => x.Name ?? string.Empty);

        return users.Select(user => new BranchUserResponseDto
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            DOB = user.DOB,

            DepartmentName = departmentDictionary.TryGetValue(user.DepartmentId, out var departmentName) ? departmentName : string.Empty,

            PositionName = positionDictionary.TryGetValue(user.PositionId, out var positionName) ? positionName : string.Empty
        }).ToList();
    }
}
