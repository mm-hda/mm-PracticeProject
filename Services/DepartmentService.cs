using backend.Data;
using backend.Dto.DepartmentDtos;
using backend.Entities;
using backend.IService;
using backend.GenericResponse;

using Microsoft.EntityFrameworkCore;

namespace backend.Services;

internal sealed class DepartmentService(AppDbContext context) : IDepartmentService
{
    public async Task<Tuple<int>> CreateDepartment(DepartmentDto dto)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
            {
                return new Tuple<int>(CustomCodes.InputsNotFound);
            }

            var exists = await context.Departments.AnyAsync(x => x.Name.Equals(dto.Name, StringComparison.OrdinalIgnoreCase)).ConfigureAwait(false);

            if (exists)
            {
                return new Tuple<int>(CustomCodes.DepartmentAlreadyExists);
            }

            Department department = new()
            {
                Id = Guid.NewGuid(),
                Name = dto.Name
            };

            await context.Departments.AddAsync(department).ConfigureAwait(false);

            try
            {
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (DbUpdateException)
            {
                return new Tuple<int>(CustomCodes.DepartmentCreationFailed);
            }

            await context.SaveChangesAsync().ConfigureAwait(false);

            return new Tuple<int>(CustomCodes.DepartmentCreatedSuccessfully);
        }
        catch (NullReferenceException)
        {
            return new Tuple<int>(CustomCodes.DepartmentCreationFailed);
        }
    }

    public async Task<Tuple<int>> UpdateDepartment(DepartmentDto dto)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (dto == null || dto.Id == Guid.Empty || string.IsNullOrWhiteSpace(dto.Name))
            {
                return new Tuple<int>(CustomCodes.InvalidInput);
            }

            var existing = await context.Departments.FirstOrDefaultAsync(x => x.Id == dto.Id).ConfigureAwait(false);

            if (existing == null)
            {
                return new Tuple<int>(CustomCodes.DepartmentNotFound);
            }

            var duplicate = await context.Departments.AnyAsync(x => x.Id != dto.Id && x.Name.Equals(dto.Name, StringComparison.OrdinalIgnoreCase)).ConfigureAwait(false);

            if (duplicate)
            {
                return new Tuple<int>(CustomCodes.DepartmentAlreadyExists);
            }

            existing.Name = dto.Name;

            await context.SaveChangesAsync().ConfigureAwait(false);

            return new Tuple<int>(CustomCodes.DepartmentUpdatedSuccessfully);
        }
        catch (Exception)
        {
            return new Tuple<int>(CustomCodes.DepartmentUpdateFailed);
            throw;
        }
    }

    public async Task<Tuple<int, List<DepartmentResponseDto>>> GetAllDepartments()
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

            return new Tuple<int, List<DepartmentResponseDto>>(CustomCodes.DataRetrieved, departments);
        }
        catch (Exception)
        {
            return new Tuple<int, List<DepartmentResponseDto>>(CustomCodes.InternalServerError, []);
            throw;
        }
    }

    public async Task<Tuple<int, DepartmentResponseDto?>> GetDepartmentById(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return new Tuple<int, DepartmentResponseDto?>(CustomCodes.InvalidInput, null);
            }

            var department = await context.Departments.AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new DepartmentResponseDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    TotalPositions = context.Positions.Count(p => p.DepartmentId == x.Id),
                    TotalUsers = context.Users.Count(u => u.DepartmentId == x.Id)
                }).FirstOrDefaultAsync().ConfigureAwait(false);

            return new Tuple<int, DepartmentResponseDto?>
            (department != null ? CustomCodes.DataRetrieved : CustomCodes.DepartmentNotFound, department);
        }
        catch (Exception)
        {
            return new Tuple<int, DepartmentResponseDto?>(CustomCodes.InternalServerError, null);
            throw;
        }
    }

    public async Task<Tuple<int, List<DepartmentUserResponseDto>>> GetDepartmentEmployees(Guid departmentId)
    {
        try
        {
            if (departmentId == Guid.Empty)
            {
                return new Tuple<int, List<DepartmentUserResponseDto>>(CustomCodes.InvalidInput, []);
            }

            var departmentExists = await context.Departments.AnyAsync(x => x.Id == departmentId).ConfigureAwait(false);

            if (!departmentExists)
            {
                return new Tuple<int, List<DepartmentUserResponseDto>>(CustomCodes.DepartmentNotFound, []);
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

            return new Tuple<int, List<DepartmentUserResponseDto>>(CustomCodes.DataRetrieved, users);
        }
        catch (Exception)
        {
            return new Tuple<int, List<DepartmentUserResponseDto>>(CustomCodes.InternalServerError, []);
            throw;
        }
    }
}
