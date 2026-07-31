using backend.Dto.DepartmentDtos;
using backend.GenericResponse;
namespace backend.IService;

public interface IDepartmentService
{
    Task<ServiceResponse<object>> CreateDepartment(DepartmentDto dto, CancellationToken cancellationToken);

    Task<ServiceResponse<object>> UpdateDepartment(DepartmentDto dto, CancellationToken cancellationToken);

    Task<ServiceResponse<IReadOnlyCollection<DepartmentResponseDto>>> GetAllDepartments(CancellationToken cancellationToken);

    Task<ServiceResponse<DepartmentResponseDto?>> GetDepartmentById(Guid id, CancellationToken cancellationToken);

    Task<ServiceResponse<IReadOnlyCollection<DepartmentUserResponseDto>>> GetDepartmentEmployees(Guid departmentId, CancellationToken cancellationToken);
}
