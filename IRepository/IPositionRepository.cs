using backend.Dto.PositionDtos;
using backend.Entities;
using backend.GenericRepositories;
namespace backend.IRepository;

public interface IPositionRepository : IGenericRepository<Position>
{
    Task<bool> DepartmentExistsAsync(Guid departmentId);
    Task<bool> PositionExistsAsync(string? name);
    Task AddPositionAsync(Position position, CancellationToken cancellationToken);
    Task<Position?> PositionByIdAsync(Guid id);
    Task<bool> DuplicatePositionExistsAsync(Guid positionId, string? name, Guid departmentId);
    Task<IReadOnlyCollection<PositionResponseDto>> GetAllPositionsAsync();
    Task<PositionResponseDto?> GetPositionByIdAsync(Guid id);
    Task<IReadOnlyCollection<PositionResponseDto>> GetPositionsByDepartmentAsync(Guid departmentId);
    Task<bool> PositionExistsAsync(Guid positionId);
    Task<IReadOnlyCollection<PositionUserResponseDto>> GetPositionUsersAsync(Guid positionId);
}
