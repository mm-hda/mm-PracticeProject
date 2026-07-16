using backend.Data;
using backend.Dto.DepartmentDto;
using backend.Entities;
using backend.IService;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class DepartmentService(AppDbContext _context) : IDepartmentService
    {
        public async Task<Tuple<int, string>> CreateDepartment(DepartmentDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                {
                    return new Tuple<int, string>(0, "Invalid request body");
                }

                var exists = await _context.Departments.AnyAsync(x => x.Name.ToLower() == dto.Name.ToLower());

                if (exists)
                {
                    return new Tuple<int, string>(0, "Department already exists");
                }

                Department department = new()
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name
                };

                await _context.Departments.AddAsync(department);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex) 
                {
                    return new Tuple<int, string>(0, "Department already exists : " + ex.Message);
                }

                await _context.SaveChangesAsync();

                return new Tuple<int, string>(1, "Department created successfully");
            }
            catch (NullReferenceException ex)
            {
                return new Tuple<int, string>(0, ex.Message);
            }
            
        }

        public async Task<Tuple<int, string>> UpdateDepartment(DepartmentDto dto)
        {
            try
            {
                if (dto == null || dto.Id == Guid.Empty || string.IsNullOrWhiteSpace(dto.Name))
                {
                    return new Tuple<int, string>(0, "Invalid request body");
                }

                var existing = await _context.Departments.FirstOrDefaultAsync(x => x.Id == dto.Id);

                if (existing == null)
                {
                    return new Tuple<int, string>(0, "Department not found");
                }

                var duplicate = await _context.Departments.AnyAsync(x => x.Id != dto.Id && x.Name.ToLower() == dto.Name.ToLower());

                if (duplicate)
                {
                    return new Tuple<int, string>(0, "Another department with this name already exists");
                }

                existing.Name = dto.Name;

                await _context.SaveChangesAsync();

                return new Tuple<int, string>(1, "Department updated successfully");
            }
            catch (Exception ex)
            {
                return new Tuple<int, string>(0, ex.Message);
            }
        }

        public async Task<Tuple<int, List<DepartmentResponseDto>, string>> GetAllDepartments()
        {
            try
            {
                var departments = await _context.Departments.AsNoTracking()
                    .Select(d => new DepartmentResponseDto
                    {
                        Id = d.Id,
                        Name = d.Name,
                        TotalPositions = _context.Positions.Count(p => p.DepartmentId == d.Id),
                        TotalUsers = _context.Users.Count(u => u.DepartmentId == d.Id)
                    }).ToListAsync();

                return new Tuple<int, List<DepartmentResponseDto>, string>(1, departments, "Departments retrieved successfully");
            }
            catch (Exception ex)
            {
                return new Tuple<int, List<DepartmentResponseDto>, string>(0, new List<DepartmentResponseDto>(), ex.Message);
            }
        }

        public async Task<Tuple<int, DepartmentResponseDto?, string>> GetDepartmentById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return new Tuple<int, DepartmentResponseDto?, string>(0, null, "Invalid department ID");
                }

                var department = await _context.Departments.AsNoTracking()
                    .Where(x => x.Id == id)
                    .Select(x => new DepartmentResponseDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        TotalPositions = _context.Positions.Count(p => p.DepartmentId == x.Id),
                        TotalUsers = _context.Users.Count(u => u.DepartmentId == x.Id)
                    }).FirstOrDefaultAsync();

                return new Tuple<int, DepartmentResponseDto?, string>
                (department != null ? 1 : 0, department, department != null ? "Department retrieved successfully" : "Department not found");
            }
            catch (Exception ex)
            {
                return new Tuple<int, DepartmentResponseDto?, string>(0, null, ex.Message);
            }
        }

        public async Task<Tuple<int, List<DepartmentUserResponseDto>, string>> GetDepartmentEmployees(Guid departmentId)
        {
            try
            {
                if (departmentId == Guid.Empty)
                {
                    return new Tuple<int, List<DepartmentUserResponseDto>, string>(0, new List<DepartmentUserResponseDto>(), "Invalid department ID");
                }

                var departmentExists = await _context.Departments.AnyAsync(x => x.Id == departmentId);

                if (!departmentExists)
                {
                    return new Tuple<int, List<DepartmentUserResponseDto>, string>(0, new List<DepartmentUserResponseDto>(), "Department not found");
                }

                var users = await _context.Users.AsNoTracking()
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
                    }).ToListAsync();

                return new Tuple<int, List<DepartmentUserResponseDto>, string>(1, users, "Users retrieved successfully");
            }
            catch (Exception ex)
            {
                return new Tuple<int, List<DepartmentUserResponseDto>, string>(0, new List<DepartmentUserResponseDto>(), ex.Message);
            }
        }
    }
}