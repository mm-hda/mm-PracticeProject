using backend.Dto.PositionDtos;
using backend.Entities;

namespace backend.IRepository;

public interface IPositionRepository
{
    Task<bool> DepartmentExistsAsync(Guid departmentId, CancellationToken cancellationToken);
    Task<bool> PositionExistsAsync(string? name, Guid departmentId, CancellationToken cancellationToken);
    Task AddAsync(Position position, CancellationToken cancellationToken);
    Task<Position?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> DuplicatePositionExistsAsync(Guid positionId, string? name, Guid departmentId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PositionResponseDto>> GetAllPositionsAsync();
    Task<PositionResponseDto?> GetPositionByIdAsync(Guid id);
    Task<IReadOnlyCollection<PositionResponseDto>> GetPositionsByDepartmentAsync(Guid departmentId);
    Task<bool> PositionExistsAsync(Guid positionId);
    Task<IReadOnlyCollection<PositionUserResponseDto>> GetPositionUsersAsync(Guid positionId);
}
