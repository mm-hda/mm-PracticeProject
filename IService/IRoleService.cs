using backend.Dto.RoleDtos;
using backend.GenericResponse;

namespace backend.IService;

public interface IRoleService
{
    Task<ServiceResponse<object>> CreateRole(RoleDto dto, CancellationToken cancellationToken);

    Task<ServiceResponse<IReadOnlyCollection<RoleResponseDto>>> GetAllRoles(CancellationToken cancellationToken);

    Task<ServiceResponse<RoleResponseDto?>> GetRoleById(Guid id, CancellationToken cancellationToken);

    Task<ServiceResponse<IReadOnlyCollection<RoleUserResponseDto>>> GetUsersByRole(Guid roleId, CancellationToken cancellationToken);
}
