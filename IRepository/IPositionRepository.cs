using backend.Dto.PositionDtos;
using backend.Entities;
using backend.GenericRepositories;
namespace backend.IRepository;

public interface IPositionRepository : IGenericRepository<Position>
{
    Task<bool> DepartmentExistsAsync(Guid departmentId, CancellationToken cancellationToken);
    Task<bool> PositionExistsAsync(string? name, CancellationToken cancellationToken);
    Task AddPositionAsync(Position position, CancellationToken cancellationToken);
    Task<Position?> PositionByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> DuplicatePositionExistsAsync(Guid positionId, string? name, Guid departmentId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PositionResponseDto>> GetAllPositionsAsync(CancellationToken cancellationToken);
    Task<PositionResponseDto?> GetPositionByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PositionResponseDto>> GetPositionsByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken);
    Task<bool> PositionExistsAsync(Guid positionId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PositionUserResponseDto>> GetPositionUsersAsync(Guid positionId, CancellationToken cancellationToken);
}
