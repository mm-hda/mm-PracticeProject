using backend.Dto.DepartmentDtos;
using backend.Entities;
using backend.GenericRepositories;
namespace backend.IRepository;

public interface IDepartmentRepository : IGenericRepository<Department>
{
    Task<bool> DepartmentExistsAsync(string? name, CancellationToken cancellationToken);
    Task AddDepartmentAsync(Department department, CancellationToken cancellationToken);
    Task<Department?> DepartmentByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> DuplicateDepartmentExistsAsync(Guid id, string? name, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<DepartmentResponseDto>> GetAllDepartmentsAsync(CancellationToken cancellationToken);
    Task<DepartmentResponseDto?> GetDepartmentByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> DepartmentExistsByIdAsync(Guid departmentId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<DepartmentUserResponseDto>> GetDepartmentEmployeesAsync(Guid departmentId, CancellationToken cancellationToken);
}
