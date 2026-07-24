using backend.Dto.BranchDtos;
using backend.Entities;

namespace backend.IRepository;

public interface IBranchRepository
{
    Task<bool> BranchExistsAsync(string? name, CancellationToken cancellationToken);

    Task<Branch?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(Branch branch, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<BranchResponseDto>> GetAllBranchesAsync();

    Task<BranchResponseDto?> GetBranchByIdAsync(Guid id);

    Task<IReadOnlyCollection<BranchUserResponseDto>> GetBranchUsersAsync(Guid branchId);
}
