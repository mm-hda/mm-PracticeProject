using backend.Data;
using backend.Dto.PositionDtos;
using backend.Entities;
using backend.IService;
using backend.GenericResponse;

using Microsoft.EntityFrameworkCore;

namespace backend.Services;

internal sealed class PositionService(AppDbContext context) : IPositionService
{
    public async Task<ServiceResponse<object>> CreatePosition(PositionDto dto, CancellationToken cancellationToken)
    {
        using var transaction = await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ArgumentNullException.ThrowIfNull(dto);

            var departmentExists = await context.Departments
                .AnyAsync(x => x.Id == dto.DepartmentId, cancellationToken)
                .ConfigureAwait(false);

            if (!departmentExists)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.DepartmentNotFound };
            }

            var exists = await context.Positions.AnyAsync(x => x.Name.Equals(dto.Name, StringComparison.OrdinalIgnoreCase) && x.DepartmentId == dto.DepartmentId, cancellationToken)
            .ConfigureAwait(false);

            if (exists)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.PositionAlreadyExists };
            }

            Position position = new()
            {
                Id = Guid.NewGuid(),
                Name = dto.Name ?? "",
                DepartmentId = dto.DepartmentId
            };

            await context.Positions.AddAsync(position, cancellationToken).ConfigureAwait(false);

            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return new ServiceResponse<object> { IsSuccess = true, StatusCode = CustomCodes.PositionCreatedSuccessfully };
        }
        catch (OperationCanceledException)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.OperationCancelled };
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.PositionCreationFailed };
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.PositionCreationFailed };
            throw;
        }
    }

    public async Task<ServiceResponse<object>> UpdatePosition(PositionDto dto, CancellationToken cancellationToken)
    {
        using var transaction = await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ArgumentNullException.ThrowIfNull(dto);

            var position = await context.Positions.FirstOrDefaultAsync(x => x.Id == dto.Id, cancellationToken).ConfigureAwait(false);

            if (position == null)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.PositionNotFound };
            }

            var departmentExists = await context.Departments.AnyAsync(x => x.Id == dto.DepartmentId, cancellationToken).ConfigureAwait(false);

            if (!departmentExists)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.DepartmentNotFound };
            }

            var duplicate = await context.Positions.AnyAsync(x => x.Id != dto.Id
                    && x.Name.Equals(dto.Name, StringComparison.OrdinalIgnoreCase)
                    && x.DepartmentId == dto.DepartmentId, cancellationToken).ConfigureAwait(false);

            if (duplicate)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.PositionAlreadyExists };
            }

            position.Name = dto.Name ?? "";
            position.DepartmentId = dto.DepartmentId;

            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return new ServiceResponse<object> { IsSuccess = true, StatusCode = CustomCodes.PositionUpdatedSuccessfully };
        }
        catch (OperationCanceledException)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.OperationCancelled };
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.PositionUpdateFailed };
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.PositionUpdateFailed };
            throw;
        }
    }

    public async Task<ServiceResponse<IReadOnlyCollection<PositionResponseDto>>> GetAllPositions()
    {
        try
        {
            var positions = await context.Positions.AsNoTracking()
                .Include(x => x.Department)
                .Select(x => new PositionResponseDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    DepartmentId = x.DepartmentId,
                    DepartmentName = x.Department != null ? x.Department.Name : "",
                    TotalUsers = context.Users.Count(u => u.PositionId == x.Id)
                }).ToListAsync().ConfigureAwait(false);

            return new ServiceResponse<IReadOnlyCollection<PositionResponseDto>> { IsSuccess = true, StatusCode = CustomCodes.DataRetrieved, Data = positions };
        }
        catch (Exception)
        {
            return new ServiceResponse<IReadOnlyCollection<PositionResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.InternalServerError };
            throw;
        }
    }

    public async Task<ServiceResponse<PositionResponseDto?>> GetPositionById(Guid id)
    {
        try
        {
            var position = await context.Positions.AsNoTracking()
                .Include(x => x.Department)
                .Where(x => x.Id == id)
                .Select(x => new PositionResponseDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    DepartmentId = x.DepartmentId,
                    DepartmentName = x.Department != null ? x.Department.Name : "",
                    TotalUsers = context.Users.Count(u => u.PositionId == x.Id)
                }).FirstOrDefaultAsync().ConfigureAwait(false);

            if (position == null)
            {
                return new ServiceResponse<PositionResponseDto?> { IsSuccess = false, StatusCode = CustomCodes.PositionNotFound };
            }

            return new ServiceResponse<PositionResponseDto?> { IsSuccess = true, StatusCode = CustomCodes.DataRetrieved, Data = position };
        }
        catch (Exception)
        {
            return new ServiceResponse<PositionResponseDto?> { IsSuccess = false, StatusCode = CustomCodes.InternalServerError };
            throw;
        }
    }

    public async Task<ServiceResponse<IReadOnlyCollection<PositionResponseDto>>> GetPositionsByDepartment(Guid departmentId)
    {
        try
        {
            var departmentExists = await context.Departments.AnyAsync(x => x.Id == departmentId).ConfigureAwait(false);

            if (!departmentExists)
            {
                return new ServiceResponse<IReadOnlyCollection<PositionResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.DepartmentNotFound };
            }

            var positions = await context.Positions.AsNoTracking()
                .Include(x => x.Department)
                .Where(x => x.DepartmentId == departmentId)
                .Select(x => new PositionResponseDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    DepartmentId = x.DepartmentId,
                    DepartmentName = x.Department != null ? x.Department.Name : "",
                    TotalUsers = context.Users.Count(u => u.PositionId == x.Id)
                }).ToListAsync().ConfigureAwait(false);

            return new ServiceResponse<IReadOnlyCollection<PositionResponseDto>> { IsSuccess = true, StatusCode = CustomCodes.DataRetrieved, Data = positions };
        }
        catch (Exception)
        {
            return new ServiceResponse<IReadOnlyCollection<PositionResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.InternalServerError };
            throw;
        }
    }

    public async Task<ServiceResponse<IReadOnlyCollection<PositionUserResponseDto>>> GetPositionUsers(Guid positionId)
    {
        try
        {
            var positionExists = await context.Positions.AnyAsync(x => x.Id == positionId).ConfigureAwait(false);

            if (!positionExists)
            {
                return new ServiceResponse<IReadOnlyCollection<PositionUserResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.PositionNotFound };
            }

            var users = await context.Users.AsNoTracking()
                .Include(x => x.Branch)
                .Include(x => x.Department)
                .Include(x => x.Position)
                .Include(x => x.Role)
                .Where(x => x.PositionId == positionId)
                .Select(x => new PositionUserResponseDto
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

            return new ServiceResponse<IReadOnlyCollection<PositionUserResponseDto>> { IsSuccess = true, StatusCode = CustomCodes.DataRetrieved, Data = users };
        }
        catch (Exception)
        {
            return new ServiceResponse<IReadOnlyCollection<PositionUserResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.InternalServerError };
            throw;
        }
    }
}
