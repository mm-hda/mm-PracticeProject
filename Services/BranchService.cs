using backend.Data;
using backend.Dto.BranchDtos;
using backend.Entities;
using backend.IService;
using backend.GenericResponse;

using Microsoft.EntityFrameworkCore;

namespace backend.Services;

internal sealed class BranchService(AppDbContext context) : IBranchService
{
    public async Task<ServiceResponse<object>> CreateBranch(BranchDto dto, CancellationToken cancellationToken)
    {
        using var transaction = await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            ArgumentNullException.ThrowIfNull(dto);

            var exists = await context.Branches.AnyAsync(x => string.Equals(x.Name, dto.Name, StringComparison.OrdinalIgnoreCase), cancellationToken).ConfigureAwait(false);

            if (exists)
            {
                return new ServiceResponse<object> { StatusCode = CustomCodes.BranchAlreadyExists, IsSuccess = false };
            }

            Branch branch = new()
            {
                Id = Guid.NewGuid(),
                Name = dto.Name ?? "",
                Location = dto.Location ?? ""
            };

            await context.Branches.AddAsync(branch, cancellationToken).ConfigureAwait(false);

            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return new ServiceResponse<object> { StatusCode = CustomCodes.BranchCreatedSuccessfully, IsSuccess = true };
        }
        catch (OperationCanceledException)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { StatusCode = CustomCodes.OperationCancelled, IsSuccess = false };
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { StatusCode = CustomCodes.BranchCreationFailed, IsSuccess = false };
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { StatusCode = CustomCodes.BranchCreationFailed, IsSuccess = false };
            throw;
        }
    }

    public async Task<ServiceResponse<object>> UpdateBranch(BranchDto dto, CancellationToken cancellationToken)
    {
        using var transaction = await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ArgumentNullException.ThrowIfNull(dto);

            var branch = await context.Branches.FirstOrDefaultAsync(x => x.Id == dto.Id, cancellationToken).ConfigureAwait(false);

            if (branch == null)
            {
                return new ServiceResponse<object> { StatusCode = CustomCodes.BranchNotFound, IsSuccess = false };
            }

            branch.Name = dto.Name ?? branch.Name;
            branch.Location = dto.Location ?? branch.Location;

            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return new ServiceResponse<object> { StatusCode = CustomCodes.BranchUpdatedSuccessfully, IsSuccess = true };
        }
        catch (OperationCanceledException)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { StatusCode = CustomCodes.OperationCancelled, IsSuccess = false };
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { StatusCode = CustomCodes.BranchUpdateFailed, IsSuccess = false };
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { StatusCode = CustomCodes.BranchUpdateFailed, IsSuccess = false };
            throw;
        }
    }

    public async Task<ServiceResponse<IReadOnlyCollection<BranchResponseDto>>> GetAllBranches()
    {
        try
        {
            var branches = await context.Branches.AsNoTracking()
                .Select(x => new BranchResponseDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Location = x.Location,
                    TotalUsers = context.Users.Count(u => u.BranchId == x.Id)
                }).ToListAsync().ConfigureAwait(false);

            return new ServiceResponse<IReadOnlyCollection<BranchResponseDto>> { StatusCode = CustomCodes.DataRetrieved, IsSuccess = true, Data = branches };
        }
        catch (Exception)
        {
            return new ServiceResponse<IReadOnlyCollection<BranchResponseDto>> { StatusCode = CustomCodes.InternalServerError, IsSuccess = false };
            throw;
        }
    }

    public async Task<ServiceResponse<BranchResponseDto?>> GetBranchById(Guid id)
    {
        try
        {
            var branch = await context.Branches.AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new BranchResponseDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Location = x.Location,
                    TotalUsers = context.Users.Count(u => u.BranchId == x.Id)
                }).FirstOrDefaultAsync().ConfigureAwait(false);
            if (branch == null)
            {
                return new ServiceResponse<BranchResponseDto?> { StatusCode = CustomCodes.BranchNotFound, IsSuccess = false };
            }

            return new ServiceResponse<BranchResponseDto?> { StatusCode = CustomCodes.DataRetrieved, IsSuccess = true, Data = branch };
        }
        catch (Exception)
        {
            return new ServiceResponse<BranchResponseDto?> { StatusCode = CustomCodes.InternalServerError, IsSuccess = false };
            throw;
        }
    }

    public async Task<ServiceResponse<IReadOnlyCollection<BranchUserResponseDto>>> GetBranchUsers(Guid branchId)
    {
        try
        {
            var users = await context.Users.AsNoTracking()
                .Include(x => x.Branch)
                .Include(x => x.Department)
                .Include(x => x.Position)
                .Include(x => x.Role)
                .Where(x => x.BranchId == branchId)
                .Select(x => new BranchUserResponseDto
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

            if (users == null || users.Count == 0)
            {
                return new ServiceResponse<IReadOnlyCollection<BranchUserResponseDto>> { StatusCode = CustomCodes.BranchNotFound, IsSuccess = false };
            }

            return new ServiceResponse<IReadOnlyCollection<BranchUserResponseDto>> { StatusCode = CustomCodes.DataRetrieved, IsSuccess = true, Data = users };
        }
        catch (Exception)
        {
            return new ServiceResponse<IReadOnlyCollection<BranchUserResponseDto>> { StatusCode = CustomCodes.InternalServerError, IsSuccess = false };
            throw;
        }
    }
}

