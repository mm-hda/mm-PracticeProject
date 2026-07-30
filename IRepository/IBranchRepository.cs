using backend.Dto.BranchDtos;
using backend.Entities;
using backend.GenericRepositories;

namespace backend.IRepository;

public interface IBranchRepository : IGenericRepository<Branch>
{
    Task<bool> BranchExistsAsync(string? name, CancellationToken cancellationToken);

    Task<Branch?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddBranchAsync(Branch branch, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<BranchResponseDto>> GetAllBranchesAsync();

    Task<BranchResponseDto?> GetBranchByIdAsync(Guid id);

    Task<IReadOnlyCollection<BranchUserResponseDto>> GetBranchUsersAsync(Guid branchId);
}
