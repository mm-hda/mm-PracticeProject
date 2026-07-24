using backend.Dto.RoleDtos;
using backend.GenericResponse;

namespace backend.IService;

public interface IRoleService
{
    Task<ServiceResponse<object>> CreateRole(RoleDto dto, CancellationToken cancellationToken);

    Task<ServiceResponse<IReadOnlyCollection<RoleResponseDto>>> GetAllRoles();

    Task<ServiceResponse<RoleResponseDto?>> GetRoleById(Guid id);

    Task<ServiceResponse<IReadOnlyCollection<RoleUserResponseDto>>> GetUsersByRole(Guid roleId);
}
