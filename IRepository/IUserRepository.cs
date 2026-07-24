using backend.Dto.UserDtos;

namespace backend.IRepository;

public interface IUserRepository
{
    Task<int> GetUsersCountAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<UserResponseDto>> GetAllUsersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<UserResponseDto>> GetUserBySearchAsync(string searchTerm);
    Task<UserResponseDto?> GetUserByIdAsync(Guid id);
    Task<IReadOnlyCollection<UserResponseDto>> GetUsersByFilterAsync(UserFilterDto dto);
}
