using backend.Dto.DepartmentDto;

namespace backend.IService
{
    public interface IDepartmentService
    {
        Task<Tuple<int, string>> CreateDepartment(DepartmentDto dto);

        Task<Tuple<int, string>> UpdateDepartment(DepartmentDto dto);

        Task<Tuple<int, List<DepartmentResponseDto>, string>> GetAllDepartments();

        Task<Tuple<int, DepartmentResponseDto?, string>> GetDepartmentById(Guid id);

        Task<Tuple<int, List<DepartmentUserResponseDto>, string>> GetDepartmentEmployees(Guid departmentId);
    }
}