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
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        var departments = await GetAllAsync(cancellationToken).ConfigureAwait(false);

        return departments.Any(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task AddDepartmentAsync(Department department, CancellationToken cancellationToken)
        => await AddAsync(department, cancellationToken).ConfigureAwait(false);

    public async Task<Department?> DepartmentByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var departments = await GetAllAsync(cancellationToken).ConfigureAwait(false);

        return departments.FirstOrDefault(x => x.Id == id);
    }

    public async Task<bool> DuplicateDepartmentExistsAsync(Guid id, string? name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        var departments = await GetAllAsync(cancellationToken).ConfigureAwait(false);

        return departments.Any(x => x.Id != id && string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
    }

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
        var departments = await GetAllAsync(cancellationToken).ConfigureAwait(false);
        var department = departments.FirstOrDefault(x => x.Id == id);

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
    {
        var departments = await GetAllAsync(cancellationToken).ConfigureAwait(false);

        return departments.Any(x => x.Id == departmentId);
    }

    public async Task<IReadOnlyCollection<DepartmentUserResponseDto>> GetDepartmentEmployeesAsync(Guid departmentId, CancellationToken cancellationToken)
    {
        var departments = await GetAllAsync(cancellationToken).ConfigureAwait(false);
        var users = await userRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var positions = await positionRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var roles = await roleRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var branches = await branchRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var departmentUsers = users.Where(x => x.DepartmentId == departmentId).ToList();

        var departmentDictionary = departments.ToDictionary(x => x.Id, x => x.Name);
        var positionDictionary = positions.ToDictionary(x => x.Id, x => x.Name);
        var roleDictionary = roles.ToDictionary(x => x.Id, x => x.Name);
        var branchDictionary = branches.ToDictionary(x => x.Id, x => x.Name);

        return departmentUsers.Select(user => new DepartmentUserResponseDto
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
