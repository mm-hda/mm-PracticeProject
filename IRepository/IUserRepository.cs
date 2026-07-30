using backend.Dto.UserDtos;
using backend.Entities;
using backend.GenericRepositories;

namespace backend.IRepository;

public interface IUserRepository : IGenericRepository<User>
{
    Task<int> GetUsersCountAsync();
    Task<IReadOnlyCollection<UserResponseDto>> GetAllUsersAsync(int pageNumber, int pageSize);
    Task<IReadOnlyCollection<UserResponseDto>> GetUserBySearchAsync(string searchTerm);
    Task<UserResponseDto?> GetUserByIdAsync(Guid id);
    Task<IReadOnlyCollection<UserResponseDto>> GetUsersByFilterAsync(UserFilterDto dto);
}
