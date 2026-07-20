using backend.Data;
using backend.Dto.RoleDtos;
using backend.Entities;
using backend.IService;
using backend.GenericResponse;

using Microsoft.EntityFrameworkCore;

namespace backend.Services;

internal sealed class RoleService(AppDbContext context) : IRoleService
{
    public async Task<Tuple<int>> CreateRole(RoleDto dto)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(dto);

            var exists = await context.Roles.AnyAsync(x => x.Name.Equals(dto.Name, StringComparison.OrdinalIgnoreCase)).ConfigureAwait(false);

            if (exists)
            {
                return new Tuple<int>(CustomCodes.RoleAlreadyExists);
            }

            Role role = new()
            {
                Id = Guid.NewGuid(),
                Name = dto.Name
            };

            await context.Roles.AddAsync(role).ConfigureAwait(false);

            await context.SaveChangesAsync().ConfigureAwait(false);

            return new Tuple<int>(CustomCodes.RoleCreatedSuccessfully);
        }
        catch (Exception)
        {
            return new Tuple<int>(CustomCodes.RoleCreationFailed);
            throw;
        }
    }

    public async Task<Tuple<int, IReadOnlyCollection<RoleResponseDto>>> GetAllRoles()
    {
        try
        {
            var roles = await context.Roles.AsNoTracking()
                .Select(x => new RoleResponseDto
                {
                    Id = x.Id,
                    Name = x.Name
                }).ToListAsync().ConfigureAwait(false);
            if (roles.Count == 0)
            {
                return new Tuple<int, IReadOnlyCollection<RoleResponseDto>>(CustomCodes.RoleNotFound, []);
            }

            return new Tuple<int, IReadOnlyCollection<RoleResponseDto>>(CustomCodes.DataRetrieved, roles);
        }
        catch (Exception)
        {
            return new Tuple<int, IReadOnlyCollection<RoleResponseDto>>(CustomCodes.InternalServerError, []);
            throw;
        }
    }

    public async Task<Tuple<int, RoleResponseDto?>> GetRoleById(Guid id)
    {
        try
        {
            var role = await context.Roles.AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new RoleResponseDto
                {
                    Id = x.Id,
                    Name = x.Name
                }).FirstOrDefaultAsync().ConfigureAwait(false);

            return new Tuple<int, RoleResponseDto?>(role != null ? CustomCodes.DataRetrieved : CustomCodes.RoleNotFound, role);
        }
        catch (Exception)
        {
            return new Tuple<int, RoleResponseDto?>(CustomCodes.InternalServerError, null);
            throw;
        }
    }

    public async Task<Tuple<int, IReadOnlyCollection<RoleUserResponseDto>>> GetUsersByRole(Guid roleId)
    {
        try
        {
            var users = await context.Users
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
                }).ToListAsync().ConfigureAwait(false);

            return new Tuple<int, IReadOnlyCollection<RoleUserResponseDto>>(CustomCodes.DataRetrieved, users);
        }
        catch (Exception)
        {
            return new Tuple<int, IReadOnlyCollection<RoleUserResponseDto>>(CustomCodes.InternalServerError, []);
            throw;
        }
    }
}
