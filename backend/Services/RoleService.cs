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

            await roleRepository.AddRoleAsync(role, cancellationToken).ConfigureAwait(false);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new ServiceResponse<object> { IsSuccess = true, StatusCode = CustomCodes.RoleCreatedSuccessfully };
        }
        catch (OperationCanceledException)
        {
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.OperationCancelled };
        }
        catch (DbUpdateException)
        {
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.RoleCreationFailed };
        }

    }

    public async Task<ServiceResponse<IReadOnlyCollection<RoleResponseDto>>> GetAllRoles(CancellationToken cancellationToken)
    {
        var roles = await roleRepository.GetAllRolesAsync(cancellationToken).ConfigureAwait(false);

        if (roles.Count == 0)
        {
            return new ServiceResponse<IReadOnlyCollection<RoleResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.RoleNotFound };
        }

        return new ServiceResponse<IReadOnlyCollection<RoleResponseDto>> { IsSuccess = true, StatusCode = CustomCodes.DataRetrieved, Data = roles };
    }
}
