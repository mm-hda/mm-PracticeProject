using backend.Dto.DepartmentDtos;
using backend.Entities;
using backend.GenericRepositories;
namespace backend.IRepository;

public interface IDepartmentRepository : IGenericRepository<Department>
{
    Task<bool> DepartmentExistsAsync(string? name, CancellationToken cancellationToken);
    Task AddDepartmentAsync(Department department, CancellationToken cancellationToken);
    Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> DuplicateDepartmentExistsAsync(Guid id, string? name, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<DepartmentResponseDto>> GetAllDepartmentsAsync();
    Task<DepartmentResponseDto?> GetDepartmentByIdAsync(Guid id);
    Task<bool> DepartmentExistsByIdAsync(Guid departmentId);
    Task<IReadOnlyCollection<DepartmentUserResponseDto>> GetDepartmentEmployeesAsync(Guid departmentId);
}
