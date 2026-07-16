using backend.Dto.RoleDto;

namespace backend.IService
{
    public interface IRoleService
    {
        Task<Tuple<int, string>> CreateRole(RoleDto dto);

        Task<Tuple<int, List<RoleResponseDto>, string>> GetAllRoles();

        Task<Tuple<int, RoleResponseDto?, string>> GetRoleById(Guid id);

        Task<Tuple<int, List<RoleUserResponseDto>, string>> GetUsersByRole(Guid roleId);
    }
}