using backend.Data;
using backend.Dto.RoleDtos;
using backend.Entities;
using backend.IService;
using backend.GenericResponse;

using Microsoft.EntityFrameworkCore;

namespace backend.Services;

internal sealed class RoleService(AppDbContext context) : IRoleService
{
    public async Task<ServiceResponse<object>> CreateRole(RoleDto dto, CancellationToken cancellationToken)
    {
        using var transaction = await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ArgumentNullException.ThrowIfNull(dto);

            var exists = await context.Roles.AnyAsync(x => x.Name.Equals(dto.Name, StringComparison.OrdinalIgnoreCase), cancellationToken).ConfigureAwait(false);

            if (exists)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.RoleAlreadyExists };
            }

            Role role = new()
            {
                Id = Guid.NewGuid(),
                Name = dto.Name ?? ""
            };

            await context.Roles.AddAsync(role, cancellationToken).ConfigureAwait(false);

            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return new ServiceResponse<object> { IsSuccess = true, StatusCode = CustomCodes.RoleCreatedSuccessfully };
        }
        catch (OperationCanceledException)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.OperationCancelled };
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.RoleCreationFailed };
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.RoleCreationFailed };
            throw;
        }
    }

    public async Task<ServiceResponse<IReadOnlyCollection<RoleResponseDto>>> GetAllRoles()
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
                return new ServiceResponse<IReadOnlyCollection<RoleResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.RoleNotFound };
            }

            return new ServiceResponse<IReadOnlyCollection<RoleResponseDto>> { IsSuccess = true, StatusCode = CustomCodes.DataRetrieved, Data = roles };
        }
        catch (Exception)
        {
            return new ServiceResponse<IReadOnlyCollection<RoleResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.InternalServerError };
            throw;
        }
    }

    public async Task<ServiceResponse<RoleResponseDto?>> GetRoleById(Guid id)
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

            if (role == null)
            {
                return new ServiceResponse<RoleResponseDto?> { IsSuccess = false, StatusCode = CustomCodes.RoleNotFound, Data = null };
            }

            return new ServiceResponse<RoleResponseDto?> { IsSuccess = true, StatusCode = CustomCodes.DataRetrieved, Data = role };
        }
        catch (Exception)
        {
            return new ServiceResponse<RoleResponseDto?> { IsSuccess = false, StatusCode = CustomCodes.InternalServerError, Data = null };
            throw;
        }
    }

    public async Task<ServiceResponse<IReadOnlyCollection<RoleUserResponseDto>>> GetUsersByRole(Guid roleId)
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

            if (users.Count == 0)
            {
                return new ServiceResponse<IReadOnlyCollection<RoleUserResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.UserNotFound };
            }
            return new ServiceResponse<IReadOnlyCollection<RoleUserResponseDto>> { IsSuccess = true, StatusCode = CustomCodes.DataRetrieved, Data = users };
        }
        catch (Exception)
        {
            return new ServiceResponse<IReadOnlyCollection<RoleUserResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.InternalServerError };
            throw;
        }
    }
}
