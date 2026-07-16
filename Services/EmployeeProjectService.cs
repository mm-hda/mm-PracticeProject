using backend.Data;
using backend.Dto.EmployeeProjectDto;
using backend.Dto.ProjectDto;
using backend.Entities;
using backend.IService;
using Microsoft.EntityFrameworkCore;
using backend.Dto.Common;

namespace backend.Services
{
    public class EmployeeProjectService(AppDbContext _context) : IEmployeeProjectService
    {
        public async Task<Tuple<int, string>> CreateEmployeeProject(EmployeeProjectDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return new Tuple<int, string>(0, "Invalid request body");
                }

                if (dto.UserId == Guid.Empty)
                {
                    return new Tuple<int, string>(0, "Invalid user id");
                }

                if (dto.ProjectId == Guid.Empty)
                {
                    return new Tuple<int, string>(0, "Invalid project id");
                }

                bool userExists = await _context.Users.AnyAsync(x => x.Id == dto.UserId);

                if (!userExists)
                {
                    return new Tuple<int, string>(0, "User not found");
                }

                var project = await _context.Projects.FirstOrDefaultAsync(x => x.Id == dto.ProjectId);

                if (project == null)
                {
                    return new Tuple<int, string>(0, "Project not found");
                }

                if (project.EndDate != null && project.EndDate < DateTime.UtcNow)
                {
                    return new Tuple<int, string>(0, "Cannot assign user to a project that has already ended");
                }

                bool alreadyAssigned = await _context.EmployeeProjects.AnyAsync(x => x.UserId == dto.UserId && x.ProjectId == dto.ProjectId);

                if (alreadyAssigned)
                {
                    return new Tuple<int, string>(0, "User already assigned to this project");
                }

                EmployeeProject employeeProject = new()
                {
                    Id = Guid.NewGuid(),
                    UserId = dto.UserId,
                    ProjectId = dto.ProjectId,
                    AssignedDate = DateTime.UtcNow
                };

                await _context.EmployeeProjects.AddAsync(employeeProject);

                await _context.SaveChangesAsync();

                return new Tuple<int, string>(1, "User assigned to project successfully");
            }
            catch (Exception ex)
            {
                return new Tuple<int, string>(0, ex.Message);
            }
        }

        public async Task<Tuple<int, string>> RemoveEmployeeProject(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return new Tuple<int, string>(0, "Invalid assignment id");
                }

                var employeeProject = await _context.EmployeeProjects.FirstOrDefaultAsync(x => x.Id == id);

                if (employeeProject == null)
                {
                    return new Tuple<int, string>(0, "Assignment not found");
                }

                var project = await _context.Projects.FirstOrDefaultAsync(x => x.Id == employeeProject.ProjectId);

                if (project == null)
                {
                    return new Tuple<int, string>(0, "Project not found");
                }

                if (project.EndDate != null && project.EndDate < DateTime.UtcNow)
                {
                    return new Tuple<int, string>(0, "Cannot remove assignment from a project that has already ended");
                }

                if (employeeProject.AssignedDate < DateTime.UtcNow.AddDays(-30))
                {
                    return new Tuple<int, string>(0, "Cannot remove assignment that has been in place for more than 30 days");
                }

                _context.EmployeeProjects.Remove(employeeProject);

                await _context.SaveChangesAsync();

                return new Tuple<int, string>(1, "Assignment removed successfully");
            }
            catch (Exception ex)
            {
                return new Tuple<int, string>(0, ex.Message);
            }
        }

        public async Task<Tuple<int, List<EmployeeProjectResponseDto>, string>> GetAllEmployeeProjects()
        {
            try
            {
                var employeeProjects = await _context.EmployeeProjects.AsNoTracking()
                    .Include(x => x.User)
                        .ThenInclude(x => x!.Role)
                    .Include(x => x.Project)
                    .Select(x => new EmployeeProjectResponseDto
                    {
                        Id = x.Id,
                        UserId = x.UserId,
                        UserName = x.User != null ? x.User.Name ?? "" : "",
                        UserEmail = x.User != null ? x.User.Email ?? "" : "",
                        RoleName = x.User != null && x.User.Role != null ? x.User.Role.Name : "",
                        ProjectId = x.ProjectId,
                        ProjectName = x.Project != null ? x.Project.Name : "",
                        AssignedDate = x.AssignedDate
                    }).ToListAsync();

                return new Tuple<int, List<EmployeeProjectResponseDto>, string>(1, employeeProjects, "Employee projects retrieved successfully");
            }
            catch (Exception ex)
            {
                return new Tuple<int, List<EmployeeProjectResponseDto>, string>(0, new List<EmployeeProjectResponseDto>(), ex.Message);
            }
        }

        public async Task<Tuple<int, List<ProjectResponseDto>, PaginationMetaDto?, string>> GetUserProjectsByUserId(Guid userId, PaginationDto dto)
        {
            try
            {

                dto.PageNumber = dto.PageNumber <= 0 ? 1 : dto.PageNumber;
                dto.PageSize = dto.PageSize <= 0 ? 10 : dto.PageSize;

                if (userId == Guid.Empty)
                {
                    return new Tuple<int, List<ProjectResponseDto>, PaginationMetaDto?, string>(0, new List<ProjectResponseDto>(), null, "Invalid user ID");
                }

                bool userExists = await _context.Users.AnyAsync(x => x.Id == userId);

                if (!userExists)
                {
                    return new Tuple<int, List<ProjectResponseDto>, PaginationMetaDto?, string>(0, new List<ProjectResponseDto>(), null, "User not found");
                }

                var query = _context.EmployeeProjects.AsNoTracking().Where(x => x.UserId == userId);

                if (query == null)
                {
                    return new Tuple<int, List<ProjectResponseDto>, PaginationMetaDto?, string>(0, new List<ProjectResponseDto>(), null, "No projects found for the user");
                }

                var totalRecords = await query.CountAsync();

                if ((int)Math.Ceiling(totalRecords / (double)dto.PageSize) < dto.PageNumber)
                {
                    return new Tuple<int, List<ProjectResponseDto>, PaginationMetaDto?, string>(0, new List<ProjectResponseDto>(), null, "Page number exceeds total pages");
                }

                var projects = await query
                    .OrderByDescending(x => x.Project!.StartDate)
                    .Skip((dto.PageNumber - 1) * dto.PageSize)
                    .Take(dto.PageSize)
                    .Select(x => new ProjectResponseDto
                    {
                        Id = x.Project != null ? x.Project.Id : Guid.Empty,
                        Name = x.Project != null ? x.Project.Name : "",
                        Description = x.Project != null ? x.Project.Description : "",
                        StartDate = x.Project != null ? x.Project.StartDate : DateTime.MinValue,
                        EndDate = x.Project != null ? x.Project.EndDate : null,
                        ProjectManagerId = x.Project != null ? x.Project.ProjectManagerId : Guid.Empty,
                        ProjectManagerName = x.Project != null && x.Project.ProjectManager != null ? x.Project.ProjectManager.Name ?? "" : "",
                        TotalUsers = _context.EmployeeProjects.Count(ep => ep.ProjectId == x.ProjectId)
                    }).ToListAsync();

                var meta = new PaginationMetaDto
                {
                    PageNumber = dto.PageNumber,
                    PageSize = dto.PageSize,
                    TotalRecords = projects.Count,
                    TotalPages = (int)Math.Ceiling(projects.Count / (double)dto.PageSize)
                };

                return new Tuple<int, List<ProjectResponseDto>, PaginationMetaDto?, string>(1, projects, meta, "User projects retrieved successfully");
            }
            catch (Exception ex)
            {
                return new Tuple<int, List<ProjectResponseDto>, PaginationMetaDto?, string>(0, new List<ProjectResponseDto>(), null, ex.Message);
            }
        }
    }
}

