using backend.Data;
using backend.Dto.PositionDto;
using backend.Entities;
using backend.IService;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class PositionService(AppDbContext _context) : IPositionService
    {
        public async Task<Tuple<int, string>> CreatePosition(PositionDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                {
                    return new Tuple<int, string>(0, "Invalid request body");
                }

                if (dto.DepartmentId == Guid.Empty)
                {
                    return new Tuple<int, string>(0, "Invalid department id");
                }

                var departmentExists = await _context.Departments
                    .AnyAsync(x => x.Id == dto.DepartmentId);

                if (!departmentExists)
                {
                    return new Tuple<int, string>(0, "Department not found");
                }

                var exists = await _context.Positions.AnyAsync(x => x.Name.ToLower() == dto.Name.ToLower() && x.DepartmentId == dto.DepartmentId);

                if (exists)
                {
                    return new Tuple<int, string>(0, "Position already exists in this department");
                }

                Position position = new()
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    DepartmentId = dto.DepartmentId
                };

                await _context.Positions.AddAsync(position);

                await _context.SaveChangesAsync();

                return new Tuple<int, string>(1, "Position created successfully");
            }
            catch (Exception ex)
            {
                return new Tuple<int, string>(0, ex.Message);
            }
        }

        public async Task<Tuple<int, string>> UpdatePosition(PositionDto dto)
        {
            try
            {
                if (dto == null || dto.Id == Guid.Empty || string.IsNullOrWhiteSpace(dto.Name))
                {
                    return new Tuple<int, string>(0, "Invalid request body");
                }

                if (dto.DepartmentId == Guid.Empty)
                {
                    return new Tuple<int, string>(0, "Invalid department id");
                }

                var position = await _context.Positions.FirstOrDefaultAsync(x => x.Id == dto.Id);

                if (position == null)
                {
                    return new Tuple<int, string>(0, "Position not found");
                }

                var departmentExists = await _context.Departments.AnyAsync(x => x.Id == dto.DepartmentId);

                if (!departmentExists)
                {
                    return new Tuple<int, string>(0, "Department not found");
                }

                var duplicate = await _context.Positions.AnyAsync(x => x.Id != dto.Id
                        && x.Name.ToLower() == dto.Name.ToLower()
                        && x.DepartmentId == dto.DepartmentId);

                if (duplicate)
                {
                    return new Tuple<int, string>(0, "Another position with this name already exists in this department");
                }

                position.Name = dto.Name;
                position.DepartmentId = dto.DepartmentId;

                await _context.SaveChangesAsync();

                return new Tuple<int, string>(1, "Position updated successfully");
            }
            catch (Exception ex)
            {
                return new Tuple<int, string>(0, ex.Message);
            }
        }

        public async Task<Tuple<int, List<PositionResponseDto>, string>> GetAllPositions()
        {
            try
            {
                var positions = await _context.Positions.AsNoTracking()
                    .Include(x => x.Department)
                    .Select(x => new PositionResponseDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        DepartmentId = x.DepartmentId,
                        DepartmentName = x.Department != null ? x.Department.Name : "",
                        TotalUsers = _context.Users.Count(u => u.PositionId == x.Id)
                    }).ToListAsync();

                return new Tuple<int, List<PositionResponseDto>, string>(1, positions, "Positions retrieved successfully");
            }
            catch (Exception ex)
            {
                return new Tuple<int, List<PositionResponseDto>, string>(0, new List<PositionResponseDto>(), ex.Message);
            }
        }

        public async Task<Tuple<int, PositionResponseDto?, string>> GetPositionById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return new Tuple<int, PositionResponseDto?, string>(0, null, "Invalid position ID");
                }

                var position = await _context.Positions.AsNoTracking()
                    .Include(x => x.Department)
                    .Where(x => x.Id == id)
                    .Select(x => new PositionResponseDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        DepartmentId = x.DepartmentId,
                        DepartmentName = x.Department != null ? x.Department.Name : "",
                        TotalUsers = _context.Users.Count(u => u.PositionId == x.Id)
                    }).FirstOrDefaultAsync();

                if (position == null)
                {
                    return new Tuple<int, PositionResponseDto?, string>(0, null, "Position not found");
                }

                return new Tuple<int, PositionResponseDto?, string>(1, position, "Position retrieved successfully");
            }
            catch (Exception ex)
            {
                return new Tuple<int, PositionResponseDto?, string>(0, null, ex.Message);
            }
        }

        public async Task<Tuple<int, List<PositionResponseDto>, string>> GetPositionsByDepartment(Guid departmentId)
        {
            try
            {
                if (departmentId == Guid.Empty)
                {
                    return new Tuple<int, List<PositionResponseDto>, string>(0, new List<PositionResponseDto>(), "Invalid department ID");
                }


                var departmentExists = await _context.Departments.AnyAsync(x => x.Id == departmentId);

                if (!departmentExists)
                {
                    return new Tuple<int, List<PositionResponseDto>, string>(0, new List<PositionResponseDto>(), "Department not found");
                }

                var positions = await _context.Positions.AsNoTracking()
                    .Include(x => x.Department)
                    .Where(x => x.DepartmentId == departmentId)
                    .Select(x => new PositionResponseDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        DepartmentId = x.DepartmentId,
                        DepartmentName = x.Department != null ? x.Department.Name : "",
                        TotalUsers = _context.Users.Count(u => u.PositionId == x.Id)
                    }).ToListAsync();

                return new Tuple<int, List<PositionResponseDto>, string>(1, positions, "Positions retrieved successfully");
            }
            catch (Exception ex)
            {
                return new Tuple<int, List<PositionResponseDto>, string>(0, new List<PositionResponseDto>(), ex.Message);
            }

        }

        public async Task<Tuple<int, List<PositionUserResponseDto>, string>> GetPositionUsers(Guid positionId)
        {
            try
            {
                if (positionId == Guid.Empty)
                {
                    return new Tuple<int, List<PositionUserResponseDto>, string>(0, new List<PositionUserResponseDto>(), "Invalid position ID");
                }

                var positionExists = await _context.Positions.AnyAsync(x => x.Id == positionId);

                if (!positionExists)
                {
                    return new Tuple<int, List<PositionUserResponseDto>, string>(0, new List<PositionUserResponseDto>(), "Position not found");
                }

                var users = await _context.Users.AsNoTracking()
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
                    }).ToListAsync();

                return new Tuple<int, List<PositionUserResponseDto>, string>(1, users, "Users retrieved successfully");
            }
            catch (Exception ex)
            {
                return new Tuple<int, List<PositionUserResponseDto>, string>(0, new List<PositionUserResponseDto>(), ex.Message);
            }
        }
    }
}