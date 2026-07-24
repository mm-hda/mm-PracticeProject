using backend.Dto.RoleDtos;
using backend.Entities;
using backend.GenericResponse;
using backend.IRepository;
using backend.IService;

using Microsoft.EntityFrameworkCore;

namespace backend.Services;

internal sealed class RoleService(IRoleRepository roleRepository, IUnitOfWork unitOfWork) : IRoleService
{
    public async Task<ServiceResponse<object>> CreateRole(RoleDto dto, CancellationToken cancellationToken)
    {
        using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ArgumentNullException.ThrowIfNull(dto);

            var exists = await roleRepository.RoleExistsAsync(dto.Name, cancellationToken).ConfigureAwait(false);

            if (exists)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.RoleAlreadyExists };
            }

            Role role = new()
            {
                Id = Guid.NewGuid(),
                Name = dto.Name ?? ""
            };

            await roleRepository.AddAsync(role, cancellationToken).ConfigureAwait(false);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return new ServiceResponse<object> { IsSuccess = true, StatusCode = CustomCodes.RoleCreatedSuccessfully };
        }
        catch (OperationCanceledException)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.OperationCancelled };
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.RoleCreationFailed };
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.RoleCreationFailed };
            throw;
        }
    }

    public async Task<ServiceResponse<IReadOnlyCollection<RoleResponseDto>>> GetAllRoles()
    {
        try
        {
            var roles = await roleRepository.GetAllRolesAsync().ConfigureAwait(false);

            if (roles.Count == 0)
            {
                return new ServiceResponse<IReadOnlyCollection<RoleResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.RoleNotFound };
            }

            return new ServiceResponse<IReadOnlyCollection<RoleResponseDto>> { IsSuccess = true, StatusCode = CustomCodes.DataRetrieved, Data = roles };
        }
        catch (Exception)
        {
            return new ServiceResponse<IReadOnlyCollection<RoleResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.InternalServerError };
            throw;
        }
    }

    public async Task<ServiceResponse<RoleResponseDto?>> GetRoleById(Guid id)
    {
        try
        {
            var role = await roleRepository.GetRoleByIdAsync(id).ConfigureAwait(false);

            if (role == null)
            {
                return new ServiceResponse<RoleResponseDto?> { IsSuccess = false, StatusCode = CustomCodes.RoleNotFound, Data = null };
            }

            return new ServiceResponse<RoleResponseDto?> { IsSuccess = true, StatusCode = CustomCodes.DataRetrieved, Data = role };
        }
        catch (Exception)
        {
            return new ServiceResponse<RoleResponseDto?> { IsSuccess = false, StatusCode = CustomCodes.InternalServerError, Data = null };
            throw;
        }
    }

    public async Task<ServiceResponse<IReadOnlyCollection<RoleUserResponseDto>>> GetUsersByRole(Guid roleId)
    {
        try
        {
            var users = await roleRepository.GetUsersByRoleAsync(roleId).ConfigureAwait(false);

            if (users.Count == 0)
            {
                return new ServiceResponse<IReadOnlyCollection<RoleUserResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.UserNotFound };
            }

            return new ServiceResponse<IReadOnlyCollection<RoleUserResponseDto>> { IsSuccess = true, StatusCode = CustomCodes.DataRetrieved, Data = users };
        }
        catch (Exception)
        {
            return new ServiceResponse<IReadOnlyCollection<RoleUserResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.InternalServerError };
            throw;
        }
    }
}
