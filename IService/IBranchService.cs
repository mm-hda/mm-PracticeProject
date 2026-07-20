using backend.Dto.BranchDtos;

namespace backend.IService;

public interface IBranchService
{
    Task<Tuple<int>> CreateBranch(BranchDto dto);

    Task<Tuple<int>> UpdateBranch(BranchDto dto);

    Task<Tuple<int, List<BranchResponseDto>>> GetAllBranches();

    Task<Tuple<int, BranchResponseDto?>> GetBranchById(Guid id);

    Task<Tuple<int, List<BranchUserResponseDto>>> GetBranchUsers(Guid branchId);
}
