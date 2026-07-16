using backend.Dto.UserDto;
using backend.Dto.Common;

namespace backend.IService
{
    public interface IUserService
    {
        Task<Tuple<int, List<UserResponseDto>, PaginationMetaDto?, string>> GetAllUsers(PaginationDto dto);
        Task<Tuple<int, List<UserResponseDto>, string>> GetUserBySearch(string searchTerm);
        Task<Tuple<int, UserResponseDto?, string>> GetUserById(Guid id);
        Task<Tuple<int, List<UserResponseDto>, string>> GetUsersByFilter(UserFilterDto dto);
    }
}