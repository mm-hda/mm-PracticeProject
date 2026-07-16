using backend.Data;
using backend.Dto.BranchDto;
using backend.Entities;
using backend.IService;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class BranchService(AppDbContext _context) : IBranchService
    {
        public async Task<Tuple<int, string>> CreateBranch(BranchDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                {
                    return new Tuple<int, string>(0, "Invalid request body");
                }

                bool exists = await _context.Branches.AnyAsync(x => x.Name.ToLower() == dto.Name.ToLower());

                if (exists)
                {
                    return new Tuple<int, string>(0, "Branch already exists");
                }

                Branch branch = new()
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Location = dto.Location
                };

                await _context.Branches.AddAsync(branch);

                await _context.SaveChangesAsync();

                return new Tuple<int, string>(1, "Branch created successfully");
            }
            catch (DbUpdateException ex)
            {
                return new Tuple<int, string>(0, ex.Message);
            }
            catch (Exception ex)
            {
                return new Tuple<int, string>(0, ex.Message);
            }
        }

        public async Task<Tuple<int, string>> UpdateBranch(BranchDto dto)
        {
            try
            {
                if (dto == null || dto.Id == Guid.Empty)
                {
                    return new Tuple<int, string>(0, "Invalid request body");
                }

                var branch = await _context.Branches.FirstOrDefaultAsync(x => x.Id == dto.Id);

                if (branch == null)
                {
                    return new Tuple<int, string>(0, "Branch not found");
                }

                branch.Name = dto.Name;
                branch.Location = dto.Location;

                await _context.SaveChangesAsync();

                return new Tuple<int, string>(1, "Branch updated successfully");
            }
            catch (DbUpdateException ex)
            {
                return new Tuple<int, string>(0, ex.Message);
            }
            catch (Exception ex)
            {
                return new Tuple<int, string>(0, ex.Message);
            }
        }

        public async Task<Tuple<int, List<BranchResponseDto>, string>> GetAllBranches()
        {
            try
            {
                var branches = await _context.Branches.AsNoTracking()
                    .Select(x => new BranchResponseDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Location = x.Location,
                        TotalUsers = _context.Users.Count(u => u.BranchId == x.Id)
                    }).ToListAsync();

                return new Tuple<int, List<BranchResponseDto>, string>(1, branches, "Branches retrieved successfully");
            }
            catch (Exception ex)
            {
                return new Tuple<int, List<BranchResponseDto>, string>(0, new List<BranchResponseDto>(), ex.Message);
            }
        }

        public async Task<Tuple<int, BranchResponseDto?, string>> GetBranchById(Guid id)
        {
            try
            {
                var branch = await _context.Branches.AsNoTracking()
                    .Where(x => x.Id == id)
                    .Select(x => new BranchResponseDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Location = x.Location,
                        TotalUsers = _context.Users.Count(u => u.BranchId == x.Id)
                    }).FirstOrDefaultAsync();
                if (branch == null)
                {
                    return new Tuple<int, BranchResponseDto?, string>(0, null, "Branch not found");
                }

                return new Tuple<int, BranchResponseDto?, string>(1, branch, "Branch retrieved successfully");
            }
            catch (Exception ex)
            {
                return new Tuple<int, BranchResponseDto?, string>(0, null, ex.Message);
            }
        }

        public async Task<Tuple<int, List<BranchUserResponseDto>, string>> GetBranchUsers(Guid branchId)
        {
            try
            {
                var users = await _context.Users.AsNoTracking()
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
                    }).ToListAsync();

                if (users == null || users.Count == 0)
                {
                    return new Tuple<int, List<BranchUserResponseDto>, string>(0, new List<BranchUserResponseDto>(), "No users found for the specified branch");
                }

                return new Tuple<int, List<BranchUserResponseDto>, string>(1, users, "Branch users retrieved successfully");
            }
            catch (Exception ex)
            {
                return new Tuple<int, List<BranchUserResponseDto>, string>(0, new List<BranchUserResponseDto>(), ex.Message);
            }
        }
    }
}