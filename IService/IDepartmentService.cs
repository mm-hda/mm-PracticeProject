using backend.Dto.DepartmentDtos;

namespace backend.IService;

public interface IDepartmentService
{
    Task<Tuple<int>> CreateDepartment(DepartmentDto dto);

    Task<Tuple<int>> UpdateDepartment(DepartmentDto dto);

    Task<Tuple<int, IReadOnlyCollection<DepartmentResponseDto>>> GetAllDepartments();

    Task<Tuple<int, DepartmentResponseDto?>> GetDepartmentById(Guid id);

    Task<Tuple<int, IReadOnlyCollection<DepartmentUserResponseDto>>> GetDepartmentEmployees(Guid departmentId);
}
