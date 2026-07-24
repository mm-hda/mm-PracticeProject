using backend.Data;
using backend.Dto.UserDtos;
using backend.Dto.CommonDtos;
using backend.IService;
using backend.GenericResponse;

using Microsoft.EntityFrameworkCore;

namespace backend.Services;

internal sealed class UserService(AppDbContext context) : IUserService
{
    public async Task<ServiceResponse<IReadOnlyCollection<UserResponseDto>>> GetAllUsers(PaginationDto dto, CancellationToken cancellationToken)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(dto);

            dto.PageNumber = dto.PageNumber <= 0 ? 1 : dto.PageNumber;
            dto.PageSize = dto.PageSize <= 0 ? 10 : dto.PageSize;

            var query = context.Users
                .AsNoTracking()
                .Include(x => x.Role)
                .Include(x => x.Branch)
                .Include(x => x.Department)
                .Include(x => x.Position)
                .Where(x => x.Role != null && x.Role.Name != "Admin");

            var totalRecords = await query.CountAsync(cancellationToken).ConfigureAwait(false);

            if (totalRecords == 0)
            {
                return new ServiceResponse<IReadOnlyCollection<UserResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.UserNotFound };
            }

            if ((int)Math.Ceiling(totalRecords / (double)dto.PageSize) < dto.PageNumber)
            {
                return new ServiceResponse<IReadOnlyCollection<UserResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.UserNotFound };
            }

            var users = await query
                .OrderBy(x => x.Name)
                .Skip((dto.PageNumber - 1) * dto.PageSize)
                .Take(dto.PageSize)
                .Select(x => new UserResponseDto
                {
                    UserId = x.Id,
                    Name = x.Name,
                    Email = x.Email,
                    DOB = x.DOB,
                    RoleName = x.Role != null ? x.Role.Name : "",
                    BranchName = x.Branch != null ? x.Branch.Name : "",
                    DepartmentName = x.Department != null ? x.Department.Name : "",
                    PositionName = x.Position != null ? x.Position.Name : ""
                }).ToListAsync(cancellationToken).ConfigureAwait(false);

            var meta = new PaginationMetaDto
            {
                PageNumber = dto.PageNumber,
                PageSize = dto.PageSize,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)dto.PageSize)
            };

            return new ServiceResponse<IReadOnlyCollection<UserResponseDto>> { IsSuccess = true, StatusCode = CustomCodes.DataRetrieved, Data = users, Meta = meta };
        }
        catch (OperationCanceledException)
        {
            return new ServiceResponse<IReadOnlyCollection<UserResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.InternalServerError };
        }
        catch (Exception)
        {
            return new ServiceResponse<IReadOnlyCollection<UserResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.InternalServerError };
            throw;
        }
    }

    public async Task<ServiceResponse<IReadOnlyCollection<UserResponseDto>>> GetUserBySearch(string searchTerm)
    {
        try
        {

            var users = await context.Users.AsNoTracking()
                .Include(x => x.Role)
                .Include(x => x.Branch)
                .Include(x => x.Department)
                .Include(x => x.Position)
                .Where(x => EF.Functions.Like(x.Name, $"%{searchTerm}%") || EF.Functions.Like(x.Email, $"%{searchTerm}%"))
                .Select(x => new UserResponseDto
                {
                    UserId = x.Id,
                    Name = x.Name,
                    Email = x.Email,
                    DOB = x.DOB,
                    RoleName = x.Role != null ? x.Role.Name : "",
                    BranchName = x.Branch != null ? x.Branch.Name : "",
                    DepartmentName = x.Department != null ? x.Department.Name : "",
                    PositionName = x.Position != null ? x.Position.Name : ""
                }).ToListAsync().ConfigureAwait(false);

            if (users == null)
            {
                return new ServiceResponse<IReadOnlyCollection<UserResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.UserNotFound };
            }

            return new ServiceResponse<IReadOnlyCollection<UserResponseDto>> { IsSuccess = true, StatusCode = CustomCodes.DataRetrieved, Data = users };
        }
        catch (Exception)
        {
            return new ServiceResponse<IReadOnlyCollection<UserResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.InternalServerError };
            throw;
        }
    }

    public async Task<ServiceResponse<UserResponseDto?>> GetUserById(Guid id)
    {
        try
        {
            var user = await context.Users.AsNoTracking()
                .Include(x => x.Role)
                .Include(x => x.Branch)
                .Include(x => x.Department)
                .Include(x => x.Position)
                .Where(x => x.Id == id)
                .Select(x => new UserResponseDto
                {
                    UserId = x.Id,
                    Name = x.Name,
                    Email = x.Email,
                    DOB = x.DOB,
                    RoleName = x.Role != null ? x.Role.Name : "",
                    BranchName = x.Branch != null ? x.Branch.Name : "",
                    DepartmentName = x.Department != null ? x.Department.Name : "",
                    PositionName = x.Position != null ? x.Position.Name : ""
                }).FirstOrDefaultAsync().ConfigureAwait(false);

            if (user == null)
            {
                return new ServiceResponse<UserResponseDto?> { IsSuccess = false, StatusCode = CustomCodes.UserNotFound };
            }

            return new ServiceResponse<UserResponseDto?> { IsSuccess = true, StatusCode = CustomCodes.DataRetrieved, Data = user };
        }
        catch (Exception)
        {
            return new ServiceResponse<UserResponseDto?> { IsSuccess = false, StatusCode = CustomCodes.InternalServerError, Data = null };
            throw;
        }
    }

    public async Task<ServiceResponse<IReadOnlyCollection<UserResponseDto>>> GetUsersByFilter(UserFilterDto dto)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(dto);

            var query = context.Users.AsNoTracking()
                .Include(x => x.Role)
                .Include(x => x.Branch)
                .Include(x => x.Department)
                .Include(x => x.Position)
                .Where(x => x.Role != null && x.Role.Name != "Admin")
                .AsQueryable();

            if (dto.RoleId.HasValue)
            {
                query = query.Where(x => x.RoleId == dto.RoleId.Value);
            }

            if (dto.BranchId.HasValue)
            {
                query = query.Where(x => x.BranchId == dto.BranchId.Value);
            }

            if (dto.DepartmentId.HasValue)
            {
                query = query.Where(x => x.DepartmentId == dto.DepartmentId.Value);
            }

            if (dto.PositionId.HasValue)
            {
                query = query.Where(x => x.PositionId == dto.PositionId.Value);
            }

            var users = await query.Select(x => new UserResponseDto
            {
                UserId = x.Id,
                Name = x.Name,
                Email = x.Email,
                DOB = x.DOB,
                RoleName = x.Role != null ? x.Role.Name : "",
                BranchName = x.Branch != null ? x.Branch.Name : "",
                DepartmentName = x.Department != null ? x.Department.Name : "",
                PositionName = x.Position != null ? x.Position.Name : ""
            }).ToListAsync().ConfigureAwait(false);

            if (users == null || users.Count == 0)
            {
                return new ServiceResponse<IReadOnlyCollection<UserResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.UserNotFound };
            }

            return new ServiceResponse<IReadOnlyCollection<UserResponseDto>> { IsSuccess = true, StatusCode = CustomCodes.DataRetrieved, Data = users };
        }
        catch (Exception)
        {
            return new ServiceResponse<IReadOnlyCollection<UserResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.InternalServerError };
            throw;
        }
    }
}

