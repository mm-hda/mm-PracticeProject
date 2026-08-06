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

        var departments = await CountAsync(x => x.Name == name, cancellationToken).ConfigureAwait(false);

        return departments > 0;
    }

    public async Task AddDepartmentAsync(Department department, CancellationToken cancellationToken) => await AddAsync(department, cancellationToken).ConfigureAwait(false);

    public async Task<Department?> DepartmentByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var department = await FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);

        return department;
    }

    public async Task<bool> DuplicateDepartmentExistsAsync(Guid id, string? name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        var departments = await CountAsync(x => x.Id != id && string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase), cancellationToken).ConfigureAwait(false);

        return departments > 0;
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
        var department = await FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);

        if (department is null)
        {
            return null;
        }

        var positions = await positionRepository.CountAsync(x => x.DepartmentId == department.Id, cancellationToken).ConfigureAwait(false);
        var users = await userRepository.CountAsync(x => x.DepartmentId == department.Id, cancellationToken).ConfigureAwait(false);

        return new DepartmentResponseDto
        {
            Id = department.Id,
            Name = department.Name,
            TotalPositions = positions,
            TotalUsers = users
        };
    }

    public async Task<bool> DepartmentExistsByIdAsync(Guid departmentId, CancellationToken cancellationToken)
    {
        var departments = await CountAsync(x => x.Id == departmentId, cancellationToken).ConfigureAwait(false);

        return departments > 0;
    }

    public async Task<IReadOnlyCollection<DepartmentUserResponseDto>> GetDepartmentEmployeesAsync(Guid departmentId, CancellationToken cancellationToken)
    {
        var departmentExistsTask = AnyAsync(x => x.Id == departmentId, cancellationToken);

        var usersTask = userRepository.FindAsync(x => x.DepartmentId == departmentId, cancellationToken);

        var positionsTask = positionRepository.GetAllAsync(cancellationToken);
        var rolesTask = roleRepository.GetAllAsync(cancellationToken);
        var branchesTask = branchRepository.GetAllAsync(cancellationToken);

        await Task.WhenAll(
            departmentExistsTask,
            usersTask,
            positionsTask,
            rolesTask,
            branchesTask).ConfigureAwait(false);

        if (!await departmentExistsTask.ConfigureAwait(false))
        {
            return Array.Empty<DepartmentUserResponseDto>();
        }

        var users = await usersTask.ConfigureAwait(false);

        var positionDictionary = (await positionsTask.ConfigureAwait(false)).ToDictionary(x => x.Id, x => x.Name);

        var roleDictionary = (await rolesTask.ConfigureAwait(false)).ToDictionary(x => x.Id, x => x.Name);

        var branchDictionary = (await branchesTask.ConfigureAwait(false)).ToDictionary(x => x.Id, x => x.Name);

        return users.Select(user => new DepartmentUserResponseDto
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            DOB = user.DOB,
            BranchName = branchDictionary.GetValueOrDefault(user.BranchId, string.Empty),
            PositionName = positionDictionary.GetValueOrDefault(user.PositionId, string.Empty),
            RoleName = roleDictionary.GetValueOrDefault(user.RoleId, string.Empty)
        }).ToList();
    }
}
