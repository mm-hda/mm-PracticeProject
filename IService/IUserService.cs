using backend.Dto.UserDtos;
using backend.Dto.CommonDtos;

namespace backend.IService;

public interface IUserService
{
    Task<Tuple<int, IReadOnlyCollection<UserResponseDto>, PaginationMetaDto?>> GetAllUsers(PaginationDto dto);
    Task<Tuple<int, IReadOnlyCollection<UserResponseDto>>> GetUserBySearch(string searchTerm);
    Task<Tuple<int, UserResponseDto?>> GetUserById(Guid id);
    Task<Tuple<int, IReadOnlyCollection<UserResponseDto>>> GetUsersByFilter(UserFilterDto dto);
}
