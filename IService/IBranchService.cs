using backend.Dto.BranchDtos;
using backend.GenericResponse;
namespace backend.IService;

public interface IBranchService
{
    Task<ServiceResponse<object>> CreateBranch(BranchDto dto, CancellationToken cancellationToken);

    Task<ServiceResponse<object>> UpdateBranch(BranchDto dto, CancellationToken cancellationToken);

    Task<ServiceResponse<IReadOnlyCollection<BranchResponseDto>>> GetAllBranches();

    Task<ServiceResponse<BranchResponseDto?>> GetBranchById(Guid id);

    Task<ServiceResponse<IReadOnlyCollection<BranchUserResponseDto>>> GetBranchUsers(Guid branchId);
}
