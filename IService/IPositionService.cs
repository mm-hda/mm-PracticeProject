using backend.Dto.PositionDtos;

namespace backend.IService;

public interface IPositionService
{
    Task<Tuple<int>> CreatePosition(PositionDto dto);

    Task<Tuple<int>> UpdatePosition(PositionDto dto);

    Task<Tuple<int, List<PositionResponseDto>>> GetAllPositions();

    Task<Tuple<int, PositionResponseDto?>> GetPositionById(Guid id);

    Task<Tuple<int, List<PositionResponseDto>>> GetPositionsByDepartment(Guid departmentId);

    Task<Tuple<int, List<PositionUserResponseDto>>> GetPositionUsers(Guid positionId);
}
