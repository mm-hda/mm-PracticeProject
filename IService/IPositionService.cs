using backend.Dto.PositionDtos;
using backend.GenericResponse;
namespace backend.IService;

public interface IPositionService
{
    Task<ServiceResponse<object>> CreatePosition(PositionDto dto, CancellationToken cancellationToken);

    Task<ServiceResponse<object>> UpdatePosition(PositionDto dto, CancellationToken cancellationToken);

    Task<ServiceResponse<IReadOnlyCollection<PositionResponseDto>>> GetAllPositions(CancellationToken cancellationToken);

    Task<ServiceResponse<PositionResponseDto?>> GetPositionById(Guid id, CancellationToken cancellationToken);

    Task<ServiceResponse<IReadOnlyCollection<PositionResponseDto>>> GetPositionsByDepartment(Guid departmentId, CancellationToken cancellationToken);

    Task<ServiceResponse<IReadOnlyCollection<PositionUserResponseDto>>> GetPositionUsers(Guid positionId, CancellationToken cancellationToken);
}
