using backend.Dto.PositionDto;

namespace backend.IService
{
    public interface IPositionService
    {
        Task<Tuple<int, string>> CreatePosition(PositionDto dto);

        Task<Tuple<int, string>> UpdatePosition(PositionDto dto);

        Task<Tuple<int, List<PositionResponseDto>, string>> GetAllPositions();

        Task<Tuple<int, PositionResponseDto?, string>> GetPositionById(Guid id);

        Task<Tuple<int, List<PositionResponseDto>, string>> GetPositionsByDepartment(Guid departmentId);

        Task<Tuple<int, List<PositionUserResponseDto>, string>> GetPositionUsers(Guid positionId);
    }
}