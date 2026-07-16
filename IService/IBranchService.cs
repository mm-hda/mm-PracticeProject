using backend.Dto.BranchDto;

namespace backend.IService
{
    public interface IBranchService
    {
        Task<Tuple<int, string>> CreateBranch(BranchDto dto);

        Task<Tuple<int, string>> UpdateBranch(BranchDto dto);

        Task<Tuple<int, List<BranchResponseDto>, string>> GetAllBranches();

        Task<Tuple<int, BranchResponseDto?, string>> GetBranchById(Guid id);

        Task<Tuple<int, List<BranchUserResponseDto>, string>> GetBranchUsers(Guid branchId);
    }
}