using backend.Dto.RoleDtos;

namespace backend.IService;

public interface IRoleService
{
    Task<Tuple<int>> CreateRole(RoleDto dto);

    Task<Tuple<int, List<RoleResponseDto>>> GetAllRoles();

    Task<Tuple<int, RoleResponseDto?>> GetRoleById(Guid id);

    Task<Tuple<int, List<RoleUserResponseDto>>> GetUsersByRole(Guid roleId);
}
