using backend.Dto.UserDtos;
using backend.Entities;
using backend.GenericRepositories;

namespace backend.IRepository;

public interface IUserRepository : IGenericRepository<User>
{
    Task<int> GetUsersCountAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<UserResponseDto>> GetAllUsersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<UserResponseDto>> GetUserBySearchAsync(string searchTerm, CancellationToken cancellationToken);
    Task<UserResponseDto?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<UserResponseDto>> GetUsersByFilterAsync(UserFilterDto dto, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<UserResponseDto>> GetManagersAsync(CancellationToken cancellationToken);
}
