using backend.Data;
using backend.Dto.BranchDtos;
using backend.Entities;
using backend.IService;
using backend.GenericResponse;

using Microsoft.EntityFrameworkCore;

namespace backend.Services;

internal sealed class BranchService(AppDbContext context) : IBranchService
{
    public async Task<Tuple<int>> CreateBranch(BranchDto dto)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
            {
                return new Tuple<int>(CustomCodes.InputsNotFound);
            }

            var exists = await context.Branches.AnyAsync(x => string.Equals(x.Name, dto.Name, StringComparison.OrdinalIgnoreCase)).ConfigureAwait(false);

            if (exists)
            {
                return new Tuple<int>(CustomCodes.BranchAlreadyExists);
            }

            Branch branch = new()
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Location = dto.Location
            };

            await context.Branches.AddAsync(branch).ConfigureAwait(false);

            await context.SaveChangesAsync().ConfigureAwait(false);

            return new Tuple<int>(CustomCodes.BranchCreatedSuccessfully);
        }
        catch (DbUpdateException)
        {
            return new Tuple<int>(CustomCodes.BranchCreationFailed);
        }
        catch (Exception)
        {
            return new Tuple<int>(CustomCodes.BranchCreationFailed);
            throw;
        }
    }

    public async Task<Tuple<int>> UpdateBranch(BranchDto dto)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (dto == null || dto.Id == Guid.Empty)
            {
                return new Tuple<int>(CustomCodes.InputsNotFound);
            }

            var branch = await context.Branches.FirstOrDefaultAsync(x => x.Id == dto.Id).ConfigureAwait(false);

            if (branch == null)
            {
                return new Tuple<int>(CustomCodes.BranchNotFound);
            }

            branch.Name = dto.Name;
            branch.Location = dto.Location;

            await context.SaveChangesAsync().ConfigureAwait(false);

            return new Tuple<int>(CustomCodes.BranchUpdatedSuccessfully);
        }
        catch (DbUpdateException)
        {
            return new Tuple<int>(CustomCodes.BranchUpdateFailed);
        }
        catch (Exception)
        {
            return new Tuple<int>(CustomCodes.BranchUpdateFailed);
            throw;
        }
    }

    public async Task<Tuple<int, IReadOnlyCollection<BranchResponseDto>>> GetAllBranches()
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

            return new Tuple<int, IReadOnlyCollection<BranchResponseDto>>(CustomCodes.DataRetrieved, branches);
        }
        catch (Exception)
        {
            return new Tuple<int, IReadOnlyCollection<BranchResponseDto>>(CustomCodes.InternalServerError, []);
            throw;
        }
    }

    public async Task<Tuple<int, BranchResponseDto?>> GetBranchById(Guid id)
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
                return new Tuple<int, BranchResponseDto?>(CustomCodes.BranchNotFound, null);
            }

            return new Tuple<int, BranchResponseDto?>(CustomCodes.DataRetrieved, branch);
        }
        catch (Exception)
        {
            return new Tuple<int, BranchResponseDto?>(CustomCodes.InternalServerError, null);
            throw;
        }
    }

    public async Task<Tuple<int, IReadOnlyCollection<BranchUserResponseDto>>> GetBranchUsers(Guid branchId)
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
                return new Tuple<int, IReadOnlyCollection<BranchUserResponseDto>>(CustomCodes.BranchNotFound, []);
            }

            return new Tuple<int, IReadOnlyCollection<BranchUserResponseDto>>(CustomCodes.DataRetrieved, users);
        }
        catch (Exception)
        {
            return new Tuple<int, IReadOnlyCollection<BranchUserResponseDto>>(CustomCodes.InternalServerError, []);
            throw;
        }
    }
}

