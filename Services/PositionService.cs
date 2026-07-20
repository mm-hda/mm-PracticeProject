using backend.Data;
using backend.Dto.PositionDtos;
using backend.Entities;
using backend.IService;
using backend.GenericResponse;

using Microsoft.EntityFrameworkCore;

namespace backend.Services;

internal sealed class PositionService(AppDbContext context) : IPositionService
{
    public async Task<Tuple<int>> CreatePosition(PositionDto dto)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
            {
                return new Tuple<int>(CustomCodes.InputsNotFound);
            }

            if (dto.DepartmentId == Guid.Empty)
            {
                return new Tuple<int>(CustomCodes.InputsNotFound);
            }

            var departmentExists = await context.Departments
                .AnyAsync(x => x.Id == dto.DepartmentId)
                .ConfigureAwait(false);

            if (!departmentExists)
            {
                return new Tuple<int>(CustomCodes.DepartmentNotFound);
            }

            var exists = await context.Positions.AnyAsync(x => x.Name.Equals(dto.Name, StringComparison.OrdinalIgnoreCase) && x.DepartmentId == dto.DepartmentId)
            .ConfigureAwait(false);

            if (exists)
            {
                return new Tuple<int>(CustomCodes.PositionAlreadyExists);
            }

            Position position = new()
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                DepartmentId = dto.DepartmentId
            };

            await context.Positions.AddAsync(position).ConfigureAwait(false);

            await context.SaveChangesAsync().ConfigureAwait(false);

            return new Tuple<int>(CustomCodes.PositionCreatedSuccessfully);
        }
        catch (Exception)
        {
            return new Tuple<int>(CustomCodes.PositionCreationFailed);
            throw;
        }
    }

    public async Task<Tuple<int>> UpdatePosition(PositionDto dto)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (dto == null || dto.Id == Guid.Empty || string.IsNullOrWhiteSpace(dto.Name))
            {
                return new Tuple<int>(CustomCodes.InputsNotFound);
            }

            if (dto.DepartmentId == Guid.Empty)
            {
                return new Tuple<int>(CustomCodes.InputsNotFound);
            }

            var position = await context.Positions.FirstOrDefaultAsync(x => x.Id == dto.Id).ConfigureAwait(false);

            if (position == null)
            {
                return new Tuple<int>(CustomCodes.PositionNotFound);
            }

            var departmentExists = await context.Departments.AnyAsync(x => x.Id == dto.DepartmentId).ConfigureAwait(false);

            if (!departmentExists)
            {
                return new Tuple<int>(CustomCodes.DepartmentNotFound);
            }

            var duplicate = await context.Positions.AnyAsync(x => x.Id != dto.Id
                    && x.Name.Equals(dto.Name, StringComparison.OrdinalIgnoreCase)
                    && x.DepartmentId == dto.DepartmentId).ConfigureAwait(false);

            if (duplicate)
            {
                return new Tuple<int>(CustomCodes.PositionAlreadyExists);
            }

            position.Name = dto.Name;
            position.DepartmentId = dto.DepartmentId;

            await context.SaveChangesAsync().ConfigureAwait(false);

            return new Tuple<int>(CustomCodes.PositionUpdatedSuccessfully);
        }
        catch (Exception)
        {
            return new Tuple<int>(CustomCodes.PositionUpdateFailed);
            throw;
        }
    }

    public async Task<Tuple<int, IReadOnlyCollection<PositionResponseDto>>> GetAllPositions()
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

            return new Tuple<int, IReadOnlyCollection<PositionResponseDto>>(CustomCodes.DataRetrieved, positions);
        }
        catch (Exception)
        {
            return new Tuple<int, IReadOnlyCollection<PositionResponseDto>>(CustomCodes.InternalServerError, []);
            throw;
        }
    }

    public async Task<Tuple<int, PositionResponseDto?>> GetPositionById(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return new Tuple<int, PositionResponseDto?>(CustomCodes.InputsNotFound, null);
            }

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
                return new Tuple<int, PositionResponseDto?>(CustomCodes.PositionNotFound, null);
            }

            return new Tuple<int, PositionResponseDto?>(CustomCodes.DataRetrieved, position);
        }
        catch (Exception)
        {
            return new Tuple<int, PositionResponseDto?>(CustomCodes.InternalServerError, null);
            throw;
        }
    }

    public async Task<Tuple<int, IReadOnlyCollection<PositionResponseDto>>> GetPositionsByDepartment(Guid departmentId)
    {
        try
        {
            if (departmentId == Guid.Empty)
            {
                return new Tuple<int, IReadOnlyCollection<PositionResponseDto>>(CustomCodes.InputsNotFound, []);
            }

            var departmentExists = await context.Departments.AnyAsync(x => x.Id == departmentId).ConfigureAwait(false);

            if (!departmentExists)
            {
                return new Tuple<int, IReadOnlyCollection<PositionResponseDto>>(CustomCodes.DepartmentNotFound, []);
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

            return new Tuple<int, IReadOnlyCollection<PositionResponseDto>>(CustomCodes.DataRetrieved, positions);
        }
        catch (Exception)
        {
            return new Tuple<int, IReadOnlyCollection<PositionResponseDto>>(CustomCodes.InternalServerError, []);
            throw;
        }
    }

    public async Task<Tuple<int, IReadOnlyCollection<PositionUserResponseDto>>> GetPositionUsers(Guid positionId)
    {
        try
        {
            if (positionId == Guid.Empty)
            {
                return new Tuple<int, IReadOnlyCollection<PositionUserResponseDto>>(CustomCodes.InputsNotFound, []);
            }

            var positionExists = await context.Positions.AnyAsync(x => x.Id == positionId).ConfigureAwait(false);

            if (!positionExists)
            {
                return new Tuple<int, IReadOnlyCollection<PositionUserResponseDto>>(CustomCodes.PositionNotFound, []);
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

            return new Tuple<int, IReadOnlyCollection<PositionUserResponseDto>>(CustomCodes.DataRetrieved, users);
        }
        catch (Exception)
        {
            return new Tuple<int, IReadOnlyCollection<PositionUserResponseDto>>(CustomCodes.InternalServerError, []);
            throw;
        }
    }
}
