using backend.Data;
using backend.Dto.DepartmentDtos;
using backend.Entities;
using backend.IRepository;
using backend.GenericRepositories;

using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

internal sealed class DepartmentRepository(AppDbContext context) : GenericRepository<Department>(context), IDepartmentRepository
{
    public async Task<bool> DepartmentExistsAsync(string? name, CancellationToken cancellationToken)
        => await DbSet.AnyAsync(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase), cancellationToken).ConfigureAwait(false);

    public async Task AddDepartmentAsync(Department department, CancellationToken cancellationToken)
        => await DbSet.AddAsync(department, cancellationToken).ConfigureAwait(false);

    public async Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await DbSet.FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);

    public async Task<bool> DuplicateDepartmentExistsAsync(Guid id, string? name, CancellationToken cancellationToken)
        => await DbSet.AnyAsync(x => x.Id != id && x.Name.Equals(name, StringComparison.OrdinalIgnoreCase), cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyCollection<DepartmentResponseDto>> GetAllDepartmentsAsync()
    {
        return await DbSet.AsNoTracking()
            .Select(d => new DepartmentResponseDto
            {
                Id = d.Id,
                Name = d.Name,
                TotalPositions = context.Positions.Count(p => p.DepartmentId == d.Id),
                TotalUsers = context.Users.Count(u => u.DepartmentId == d.Id)
            })
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<DepartmentResponseDto?> GetDepartmentByIdAsync(Guid id)
    {
        return await DbSet.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new DepartmentResponseDto
            {
                Id = x.Id,
                Name = x.Name,
                TotalPositions = context.Positions.Count(p => p.DepartmentId == x.Id),
                TotalUsers = context.Users.Count(u => u.DepartmentId == x.Id)
            })
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }

    public async Task<bool> DepartmentExistsByIdAsync(Guid departmentId)
        => await DbSet.AnyAsync(x => x.Id == departmentId).ConfigureAwait(false);

    public async Task<IReadOnlyCollection<DepartmentUserResponseDto>> GetDepartmentEmployeesAsync(Guid departmentId)
    {
        return await context.Users.AsNoTracking()
            .Include(x => x.Branch)
            .Include(x => x.Department)
            .Include(x => x.Position)
            .Include(x => x.Role)
            .Where(x => x.DepartmentId == departmentId)
            .Select(x => new DepartmentUserResponseDto
            {
                UserId = x.Id,
                Name = x.Name ?? "",
                Email = x.Email ?? "",
                DOB = x.DOB,
                BranchName = x.Branch != null ? x.Branch.Name : "",
                DepartmentName = x.Department != null ? x.Department.Name : "",
                PositionName = x.Position != null ? x.Position.Name : "",
                RoleName = x.Role != null ? x.Role.Name : ""
            })
            .ToListAsync()
            .ConfigureAwait(false);
    }
}
