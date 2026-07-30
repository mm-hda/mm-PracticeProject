using backend.Data;
using backend.Dto.RoleDtos;
using backend.Entities;
using backend.IRepository;
using backend.GenericRepositories;

using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

internal sealed class RoleRepository(AppDbContext context) : GenericRepository<Role>(context), IRoleRepository
{
    public async Task<bool> RoleExistsAsync(string? name, CancellationToken cancellationToken) => await DbSet.AnyAsync(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase), cancellationToken).ConfigureAwait(false);

    public async Task AddRoleAsync(Role role, CancellationToken cancellationToken) => await DbSet.AddAsync(role, cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyCollection<RoleResponseDto>> GetAllRolesAsync()
    {
        return await DbSet.AsNoTracking()
            .Select(x => new RoleResponseDto
            {
                Id = x.Id,
                Name = x.Name
            })
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<RoleResponseDto?> GetRoleByIdAsync(Guid id)
    {
        return await DbSet.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new RoleResponseDto
            {
                Id = x.Id,
                Name = x.Name
            })
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<RoleUserResponseDto>> GetUsersByRoleAsync(Guid roleId)
    {
        return await context.Users
            .Include(x => x.Role)
            .Include(x => x.Department)
            .Include(x => x.Position)
            .Include(x => x.Branch)
            .Where(x => x.RoleId == roleId)
            .Select(x => new RoleUserResponseDto
            {
                UserId = x.Id,
                Name = x.Name ?? "",
                Email = x.Email ?? "",
                RoleName = x.Role != null ? x.Role.Name : "",
                DepartmentName = x.Department != null ? x.Department.Name : "",
                PositionName = x.Position != null ? x.Position.Name : "",
                BranchName = x.Branch != null ? x.Branch.Name : ""
            })
            .ToListAsync()
            .ConfigureAwait(false);
    }
}
