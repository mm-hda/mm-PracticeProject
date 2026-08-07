using backend.Dto.CommonDtos;
using backend.Dto.UserDtos;
using backend.Dto;
using backend.GenericResponse;
using backend.IRepository;
using backend.IService;

namespace backend.Services;

internal sealed class UserService(IUserRepository userRepository, IUnitOfWork unitOfWork) : IUserService
{
    public async Task<ServiceResponse<IReadOnlyCollection<UserResponseDto>>> GetAllUsers(PaginationDto dto, CancellationToken cancellationToken)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(dto);

            dto.PageNumber = dto.PageNumber <= 0 ? 1 : dto.PageNumber;
            dto.PageSize = dto.PageSize <= 0 ? 10 : dto.PageSize;

            var totalRecords = await userRepository.GetUsersCountAsync(cancellationToken).ConfigureAwait(false);

            if (totalRecords == 0)
            {
                return new ServiceResponse<IReadOnlyCollection<UserResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.UserNotFound };
            }

            if ((int)Math.Ceiling(totalRecords / (double)dto.PageSize) < dto.PageNumber)
            {
                return new ServiceResponse<IReadOnlyCollection<UserResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.UserNotFound };
            }

            var users = await userRepository.GetAllUsersAsync(dto.PageNumber, dto.PageSize, cancellationToken).ConfigureAwait(false);

            var meta = new PaginationMetaDto
            {
                PageNumber = dto.PageNumber,
                PageSize = dto.PageSize,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)dto.PageSize)
            };

            return new ServiceResponse<IReadOnlyCollection<UserResponseDto>> { IsSuccess = true, StatusCode = CustomCodes.DataRetrieved, Data = users, Meta = meta };
        }
        catch (OperationCanceledException)
        {
            return new ServiceResponse<IReadOnlyCollection<UserResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.InternalServerError };
        }
        catch (Exception)
        {
            return new ServiceResponse<IReadOnlyCollection<UserResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.InternalServerError };
            throw;
        }
    }

    public async Task<ServiceResponse<IReadOnlyCollection<UserResponseDto>>> GetUserBySearch(string searchTerm, CancellationToken cancellationToken)
    {
        try
        {
            var users = await userRepository.GetUserBySearchAsync(searchTerm, cancellationToken).ConfigureAwait(false);

            if (users == null)
            {
                return new ServiceResponse<IReadOnlyCollection<UserResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.UserNotFound };
            }

            return new ServiceResponse<IReadOnlyCollection<UserResponseDto>> { IsSuccess = true, StatusCode = CustomCodes.DataRetrieved, Data = users };
        }
        catch (Exception)
        {
            return new ServiceResponse<IReadOnlyCollection<UserResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.InternalServerError };
            throw;
        }
    }

    public async Task<ServiceResponse<UserResponseDto?>> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userRepository.GetUserByIdAsync(id, cancellationToken).ConfigureAwait(false);

            if (user == null)
            {
                return new ServiceResponse<UserResponseDto?> { IsSuccess = false, StatusCode = CustomCodes.UserNotFound };
            }

            return new ServiceResponse<UserResponseDto?> { IsSuccess = true, StatusCode = CustomCodes.DataRetrieved, Data = user };
        }
        catch (Exception)
        {
            return new ServiceResponse<UserResponseDto?> { IsSuccess = false, StatusCode = CustomCodes.InternalServerError, Data = null };
            throw;
        }
    }

    public async Task<ServiceResponse<IReadOnlyCollection<UserResponseDto>>> GetUsersByFilter(UserFilterDto dto, CancellationToken cancellationToken)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(dto);

            var users = await userRepository.GetUsersByFilterAsync(dto, cancellationToken).ConfigureAwait(false);

            if (users == null || users.Count == 0)
            {
                return new ServiceResponse<IReadOnlyCollection<UserResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.UserNotFound };
            }

            return new ServiceResponse<IReadOnlyCollection<UserResponseDto>> { IsSuccess = true, StatusCode = CustomCodes.DataRetrieved, Data = users };
        }
        catch (Exception)
        {
            return new ServiceResponse<IReadOnlyCollection<UserResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.InternalServerError };
            throw;
        }
    }

    public async Task<ServiceResponse<IReadOnlyCollection<UserResponseDto>>> GetManagers(CancellationToken cancellationToken)
    {
        try
        {
            var users = await userRepository.GetManagersAsync(cancellationToken).ConfigureAwait(false);

            if (users == null || users.Count == 0)
            {
                return new ServiceResponse<IReadOnlyCollection<UserResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.UserNotFound };
            }

            return new ServiceResponse<IReadOnlyCollection<UserResponseDto>> { IsSuccess = true, StatusCode = CustomCodes.DataRetrieved, Data = users };
        }
        catch (Exception)
        {
            return new ServiceResponse<IReadOnlyCollection<UserResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.InternalServerError };
            throw;
        }
    }

    public async Task<ServiceResponse<object?>> UpdateUser(Guid id, RegisterUserDtoV2 dto, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userRepository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);

            if (user == null)
            {
                return new ServiceResponse<object?> { IsSuccess = false, StatusCode = CustomCodes.UserNotFound };
            }

            user.Name = dto.FirstName + " " + dto.LastName;
            user.Email = dto.Email ?? user.Email;
            user.PositionId = dto.PositionId;

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object?> { IsSuccess = true, StatusCode = CustomCodes.UserUpdatedSuccessfully };
        }
        catch (Exception)
        {
            return new ServiceResponse<object?> { IsSuccess = false, StatusCode = CustomCodes.InternalServerError };
            throw;
        }
    }
}
