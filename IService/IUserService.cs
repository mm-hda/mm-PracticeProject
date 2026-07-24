using backend.Dto.UserDtos;
using backend.Dto.CommonDtos;
using backend.GenericResponse;

namespace backend.IService;

public interface IUserService
{
    Task<ServiceResponse<IReadOnlyCollection<UserResponseDto>>> GetAllUsers(PaginationDto dto, CancellationToken cancellationToken);
    Task<ServiceResponse<IReadOnlyCollection<UserResponseDto>>> GetUserBySearch(string searchTerm);
    Task<ServiceResponse<UserResponseDto?>> GetUserById(Guid id);
    Task<ServiceResponse<IReadOnlyCollection<UserResponseDto>>> GetUsersByFilter(UserFilterDto dto);
}
