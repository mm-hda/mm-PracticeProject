using backend.Dto.RoleDtos;

namespace backend.IService;

public interface IRoleService
{
    Task<Tuple<int>> CreateRole(RoleDto dto);

    Task<Tuple<int, IReadOnlyCollection<RoleResponseDto>>> GetAllRoles();

    Task<Tuple<int, RoleResponseDto?>> GetRoleById(Guid id);

    Task<Tuple<int, IReadOnlyCollection<RoleUserResponseDto>>> GetUsersByRole(Guid roleId);
}
