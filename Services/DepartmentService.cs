using backend.Data;
using backend.Dto.DepartmentDtos;
using backend.Entities;
using backend.IService;
using backend.GenericResponse;

using Microsoft.EntityFrameworkCore;

namespace backend.Services;

internal sealed class DepartmentService(AppDbContext context) : IDepartmentService
{
    public async Task<ServiceResponse<object>> CreateDepartment(DepartmentDto dto, CancellationToken cancellationToken)
    {
        using var transaction = await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ArgumentNullException.ThrowIfNull(dto);

            var exists = await context.Departments.AnyAsync(x => x.Name.Equals(dto.Name, StringComparison.OrdinalIgnoreCase), cancellationToken).ConfigureAwait(false);

            if (exists)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.DepartmentAlreadyExists };
            }

            Department department = new()
            {
                Id = Guid.NewGuid(),
                Name = dto.Name ?? ""
            };

            await context.Departments.AddAsync(department, cancellationToken).ConfigureAwait(false);

            try
            {
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (DbUpdateException)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.DepartmentCreationFailed };
            }

            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { IsSuccess = true, StatusCode = CustomCodes.DepartmentCreatedSuccessfully };
        }
        catch (OperationCanceledException)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.OperationCancelled };
        }
        catch (NullReferenceException)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.DepartmentCreationFailed };
        }
    }

    public async Task<ServiceResponse<object>> UpdateDepartment(DepartmentDto dto, CancellationToken cancellationToken)
    {
        using var transaction = await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ArgumentNullException.ThrowIfNull(dto);

            var existing = await context.Departments.FirstOrDefaultAsync(x => x.Id == dto.Id, cancellationToken).ConfigureAwait(false);

            if (existing == null)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.DepartmentNotFound };
            }

            var duplicate = await context.Departments.AnyAsync(x => x.Id != dto.Id && x.Name.Equals(dto.Name, StringComparison.OrdinalIgnoreCase), cancellationToken).ConfigureAwait(false);

            if (duplicate)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.DepartmentAlreadyExists };
            }

            existing.Name = dto.Name ?? "";

            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return new ServiceResponse<object> { IsSuccess = true, StatusCode = CustomCodes.DepartmentUpdatedSuccessfully };
        }
        catch (OperationCanceledException)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.OperationCancelled };
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.DepartmentUpdateFailed };
            throw;
        }
    }

    public async Task<ServiceResponse<IReadOnlyCollection<DepartmentResponseDto>>> GetAllDepartments()
    {
        try
        {
            var departments = await context.Departments.AsNoTracking()
                .Select(d => new DepartmentResponseDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    TotalPositions = context.Positions.Count(p => p.DepartmentId == d.Id),
                    TotalUsers = context.Users.Count(u => u.DepartmentId == d.Id)
                }).ToListAsync().ConfigureAwait(false);

            return new ServiceResponse<IReadOnlyCollection<DepartmentResponseDto>> { IsSuccess = true, StatusCode = CustomCodes.DataRetrieved, Data = departments };
        }
        catch (Exception)
        {
            return new ServiceResponse<IReadOnlyCollection<DepartmentResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.InternalServerError };
            throw;
        }
    }

    public async Task<ServiceResponse<DepartmentResponseDto?>> GetDepartmentById(Guid id)
    {
        try
        {
            var department = await context.Departments.AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new DepartmentResponseDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    TotalPositions = context.Positions.Count(p => p.DepartmentId == x.Id),
                    TotalUsers = context.Users.Count(u => u.DepartmentId == x.Id)
                }).FirstOrDefaultAsync().ConfigureAwait(false);

            if (department == null)
            {
                return new ServiceResponse<DepartmentResponseDto?> { IsSuccess = false, StatusCode = CustomCodes.DepartmentNotFound, Data = null };
            }

            return new ServiceResponse<DepartmentResponseDto?> { IsSuccess = true, StatusCode = CustomCodes.DataRetrieved, Data = department };
        }
        catch (Exception)
        {
            return new ServiceResponse<DepartmentResponseDto?> { IsSuccess = false, StatusCode = CustomCodes.InternalServerError };
            throw;
        }
    }

    public async Task<ServiceResponse<IReadOnlyCollection<DepartmentUserResponseDto>>> GetDepartmentEmployees(Guid departmentId)
    {
        try
        {
            var departmentExists = await context.Departments.AnyAsync(x => x.Id == departmentId).ConfigureAwait(false);

            if (!departmentExists)
            {
                return new ServiceResponse<IReadOnlyCollection<DepartmentUserResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.DepartmentNotFound };
            }

            var users = await context.Users.AsNoTracking()
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
                }).ToListAsync().ConfigureAwait(false);

            return new ServiceResponse<IReadOnlyCollection<DepartmentUserResponseDto>> { IsSuccess = true, StatusCode = CustomCodes.DataRetrieved, Data = users };
        }
        catch (Exception)
        {
            return new ServiceResponse<IReadOnlyCollection<DepartmentUserResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.InternalServerError };
            throw;
        }
    }
}
