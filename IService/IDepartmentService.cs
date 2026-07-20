using backend.Dto.DepartmentDtos;

namespace backend.IService;

public interface IDepartmentService
{
    Task<Tuple<int>> CreateDepartment(DepartmentDto dto);

    Task<Tuple<int>> UpdateDepartment(DepartmentDto dto);

    Task<Tuple<int, List<DepartmentResponseDto>>> GetAllDepartments();

    Task<Tuple<int, DepartmentResponseDto?>> GetDepartmentById(Guid id);

    Task<Tuple<int, List<DepartmentUserResponseDto>>> GetDepartmentEmployees(Guid departmentId);
}
