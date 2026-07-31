using backend.Data;
using backend.Dto.DepartmentDtos;
using backend.Entities;
using backend.GenericRepositories;
using backend.IRepository;

namespace backend.Repositories;

internal sealed class DepartmentRepository(
    AppDbContext context,
    IGenericRepository<User> userRepository,
    IGenericRepository<Position> positionRepository,
    IGenericRepository<Role> roleRepository,
    IGenericRepository<Branch> branchRepository)
    : GenericRepository<Department>(context), IDepartmentRepository
{
    public async Task<bool> DepartmentExistsAsync(string? name, CancellationToken cancellationToken)
        => !string.IsNullOrWhiteSpace(name) &&
           await AnyAsync(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase), cancellationToken).ConfigureAwait(false);

    public async Task AddDepartmentAsync(Department department, CancellationToken cancellationToken)
        => await AddAsync(department, cancellationToken).ConfigureAwait(false);

    public async Task<Department?> DepartmentByIdAsync(Guid id, CancellationToken cancellationToken)
        => await FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);

    public async Task<bool> DuplicateDepartmentExistsAsync(Guid id, string? name, CancellationToken cancellationToken)
        => !string.IsNullOrWhiteSpace(name) &&
           await AnyAsync(x => x.Id != id && x.Name.Equals(name, StringComparison.OrdinalIgnoreCase), cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyCollection<DepartmentResponseDto>> GetAllDepartmentsAsync(CancellationToken cancellationToken)
    {
        var departments = await GetAllAsync(cancellationToken).ConfigureAwait(false);
        var positions = await positionRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var users = await userRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        return departments.Select(department => new DepartmentResponseDto
        {
            Id = department.Id,
            Name = department.Name,
            TotalPositions = positions.Count(x => x.DepartmentId == department.Id),
            TotalUsers = users.Count(x => x.DepartmentId == department.Id)
        }).ToList();
    }

    public async Task<DepartmentResponseDto?> GetDepartmentByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var department = await FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);

        if (department is null)
        {
            return null;
        }

        var positions = await positionRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var users = await userRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        return new DepartmentResponseDto
        {
            Id = department.Id,
            Name = department.Name,
            TotalPositions = positions.Count(x => x.DepartmentId == department.Id),
            TotalUsers = users.Count(x => x.DepartmentId == department.Id)
        };
    }

    public async Task<bool> DepartmentExistsByIdAsync(Guid departmentId, CancellationToken cancellationToken)
        => await AnyAsync(x => x.Id == departmentId, cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyCollection<DepartmentUserResponseDto>> GetDepartmentEmployeesAsync(Guid departmentId, CancellationToken cancellationToken)
    {
        var users = await userRepository.FindAsync(x => x.DepartmentId == departmentId, cancellationToken).ConfigureAwait(false);

        var departments = await GetAllAsync(cancellationToken).ConfigureAwait(false);
        var positions = await positionRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var roles = await roleRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var branches = await branchRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var departmentDictionary = departments.ToDictionary(x => x.Id, x => x.Name);
        var positionDictionary = positions.ToDictionary(x => x.Id, x => x.Name);
        var roleDictionary = roles.ToDictionary(x => x.Id, x => x.Name);
        var branchDictionary = branches.ToDictionary(x => x.Id, x => x.Name);

        return users.Select(user => new DepartmentUserResponseDto
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            DOB = user.DOB,
            BranchName = branchDictionary.TryGetValue(user.BranchId, out var branchName) ? branchName : string.Empty,
            DepartmentName = departmentDictionary.TryGetValue(user.DepartmentId, out var departmentName) ? departmentName : string.Empty,
            PositionName = positionDictionary.TryGetValue(user.PositionId, out var positionName) ? positionName : string.Empty,
            RoleName = roleDictionary.TryGetValue(user.RoleId, out var roleName) ? roleName : string.Empty
        }).ToList();
    }
}
