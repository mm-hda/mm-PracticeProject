using backend.Dto.UserDtos;
using backend.Dto.CommonDtos;
using backend.GenericResponse;
using backend.Dto;

namespace backend.IService;

public interface IUserService
{
    Task<ServiceResponse<IReadOnlyCollection<UserResponseDto>>> GetAllUsers(PaginationDto dto, CancellationToken cancellationToken);
    Task<ServiceResponse<IReadOnlyCollection<UserResponseDto>>> GetUserBySearch(string searchTerm, CancellationToken cancellationToken);
    Task<ServiceResponse<UserResponseDto?>> GetUserById(Guid id, CancellationToken cancellationToken);
    Task<ServiceResponse<IReadOnlyCollection<UserResponseDto>>> GetUsersByFilter(UserFilterDto dto, CancellationToken cancellationToken);
    Task<ServiceResponse<IReadOnlyCollection<UserResponseDto>>> GetManagers(CancellationToken cancellationToken);
    Task<ServiceResponse<object?>> UpdateUser(Guid id, RegisterUserDtoV2 dto, CancellationToken cancellationToken);
}
