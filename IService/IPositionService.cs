using backend.Dto.PositionDtos;
using backend.GenericResponse;
namespace backend.IService;

public interface IPositionService
{
    Task<ServiceResponse<object>> CreatePosition(PositionDto dto, CancellationToken cancellationToken);

    Task<ServiceResponse<object>> UpdatePosition(PositionDto dto, CancellationToken cancellationToken);

    Task<ServiceResponse<IReadOnlyCollection<PositionResponseDto>>> GetAllPositions();

    Task<ServiceResponse<PositionResponseDto?>> GetPositionById(Guid id);

    Task<ServiceResponse<IReadOnlyCollection<PositionResponseDto>>> GetPositionsByDepartment(Guid departmentId);

    Task<ServiceResponse<IReadOnlyCollection<PositionUserResponseDto>>> GetPositionUsers(Guid positionId);
}
