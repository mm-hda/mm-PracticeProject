using backend.Dto.DepartmentDtos;
using backend.GenericResponse;
namespace backend.IService;

public interface IDepartmentService
{
    Task<ServiceResponse<object>> CreateDepartment(DepartmentDto dto, CancellationToken cancellationToken);

    Task<ServiceResponse<object>> UpdateDepartment(DepartmentDto dto, CancellationToken cancellationToken);

    Task<ServiceResponse<IReadOnlyCollection<DepartmentResponseDto>>> GetAllDepartments();

    Task<ServiceResponse<DepartmentResponseDto?>> GetDepartmentById(Guid id);

    Task<ServiceResponse<IReadOnlyCollection<DepartmentUserResponseDto>>> GetDepartmentEmployees(Guid departmentId);
}
