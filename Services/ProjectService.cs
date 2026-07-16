using backend.Data;
using backend.Dto.ProjectDto;
using backend.Entities;
using backend.IService;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class ProjectService(AppDbContext _context) : IProjectService
    {
        public async Task<Tuple<int, string>> CreateProject(ProjectDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return new Tuple<int, string>(0, "Invalid request body");
                }

                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    return new Tuple<int, string>(0, "Project name is required");
                }

                if (dto.ProjectManagerId == Guid.Empty)
                {
                    return new Tuple<int, string>(0, "Project manager id is required");
                }

                bool projectExists = await _context.Projects.AnyAsync(x => x.Name.ToLower() == dto.Name.ToLower());

                if (projectExists)
                {
                    return new Tuple<int, string>(0, "Project already exists");
                }

                bool managerExists = await _context.Users.AnyAsync(x => x.Id == dto.ProjectManagerId && x.Role != null && x.Role.Name == "Manager");

                if (!managerExists)
                {
                    return new Tuple<int, string>(0, "Project manager not found");
                }

                if (dto.EndDate != null && dto.EndDate < dto.StartDate)
                {
                    return new Tuple<int, string>(0, "End date cannot be less than start date");
                }

                Project project = new()
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Description = dto.Description,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    ProjectManagerId = dto.ProjectManagerId
                };

                await _context.Projects.AddAsync(project);

                await _context.SaveChangesAsync();

                return new Tuple<int, string>(1, "Project created successfully");
            }
            catch (Exception ex)
            {
                return new Tuple<int, string>(0, ex.Message);
            }
        }

        public async Task<Tuple<int, string>> UpdateProject(ProjectDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return new Tuple<int, string>(0, "Invalid request body");
                }

                if (dto.Id == Guid.Empty)
                {
                    return new Tuple<int, string>(0, "Project id is required");
                }

                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    return new Tuple<int, string>(0, "Project name is required");
                }

                if (dto.ProjectManagerId == Guid.Empty)
                {
                    return new Tuple<int, string>(0, "Project manager id is required");
                }

                var project = await _context.Projects.FirstOrDefaultAsync(x => x.Id == dto.Id);

                if (project == null)
                {
                    return new Tuple<int, string>(0, "Project not found");
                }

                bool managerExists = await _context.Users.AnyAsync(x => x.Id == dto.ProjectManagerId && x.Role != null && x.Role.Name == "Manager");

                if (!managerExists)
                {
                    return new Tuple<int, string>(0, "Project manager not found");
                }

                bool duplicateProject = await _context.Projects.AnyAsync(x => x.Id != dto.Id && x.Name.ToLower() == dto.Name.ToLower());

                if (duplicateProject)
                {
                    return new Tuple<int, string>(0, "Another project with this name already exists");
                }

                if (dto.EndDate != null && dto.EndDate < dto.StartDate)
                {
                    return new Tuple<int, string>(0, "End date cannot be less than start date");
                }

                project.Name = dto.Name;
                project.Description = dto.Description;
                project.StartDate = dto.StartDate;
                project.EndDate = dto.EndDate;
                project.ProjectManagerId = dto.ProjectManagerId;

                await _context.SaveChangesAsync();

                return new Tuple<int, string>(1, "Project updated successfully");
            }
            catch (Exception ex)
            {
                return new Tuple<int, string>(0, ex.Message);
            }
        }

        public async Task<Tuple<int, List<ProjectResponseDto>, string>> GetAllProjects()
        {
            try
            {
                var projects = await _context.Projects.AsNoTracking()
                    .Include(x => x.ProjectManager)
                    .Select(x => new ProjectResponseDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Description = x.Description,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        ProjectManagerId = x.ProjectManagerId,
                        ProjectManagerName = x.ProjectManager != null ? x.ProjectManager.Name ?? "" : "",
                        TotalUsers = _context.EmployeeProjects.Count(ep => ep.ProjectId == x.Id)
                    }).ToListAsync();

                return new Tuple<int, List<ProjectResponseDto>, string>(1, projects, "Projects retrieved successfully");
            }
            catch (Exception ex)
            {
                return new Tuple<int, List<ProjectResponseDto>, string>(0, new List<ProjectResponseDto>(), ex.Message);
            }
        }

        public async Task<Tuple<int, ProjectResponseDto?, string>> GetProjectById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return new Tuple<int, ProjectResponseDto?, string>(0, null, "Invalid project ID");
                }

                var project = await _context.Projects.AsNoTracking()
                    .Include(x => x.ProjectManager)
                    .Where(x => x.Id == id)
                    .Select(x => new ProjectResponseDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Description = x.Description,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        ProjectManagerId = x.ProjectManagerId,
                        ProjectManagerName = x.ProjectManager != null ? x.ProjectManager.Name ?? "" : "",
                        TotalUsers = _context.EmployeeProjects.Count(ep => ep.ProjectId == x.Id)
                    }).FirstOrDefaultAsync();

                if (project == null)
                {
                    return new Tuple<int, ProjectResponseDto?, string>(0, null, "Project not found");
                }

                return new Tuple<int, ProjectResponseDto?, string>(1, project, "Project retrieved successfully");
            }
            catch (Exception ex)
            {
                return new Tuple<int, ProjectResponseDto?, string>(0, null, ex.Message);
            }
        }

        public async Task<Tuple<int, List<ProjectUserResponseDto>, string>> GetProjectEmployees(Guid projectId)
        {
            try
            {
                if (projectId == Guid.Empty)
                {
                    return new Tuple<int, List<ProjectUserResponseDto>, string>(0, new List<ProjectUserResponseDto>(), "Invalid project ID");
                }

                bool projectExists = await _context.Projects.AnyAsync(x => x.Id == projectId);

                if (!projectExists)
                {
                    return new Tuple<int, List<ProjectUserResponseDto>, string>(0, new List<ProjectUserResponseDto>(), "Project not found");
                }

                var users = await _context.EmployeeProjects.AsNoTracking()
                    .Include(x => x.User)
                        .ThenInclude(x => x!.Branch)
                    .Include(x => x.User)
                        .ThenInclude(x => x!.Department)
                    .Include(x => x.User)
                        .ThenInclude(x => x!.Position)
                    .Include(x => x.User)
                        .ThenInclude(x => x!.Role)
                    .Where(x => x.ProjectId == projectId)
                    .Select(x => new ProjectUserResponseDto
                    {
                        UserId = x.User != null ? x.User.Id : Guid.Empty,
                        Name = x.User != null ? x.User.Name ?? "" : "",
                        Email = x.User != null ? x.User.Email ?? "" : "",
                        DOB = x.User != null ? x.User.DOB : null,
                        BranchName = x.User != null && x.User.Branch != null ? x.User.Branch.Name : "",
                        DepartmentName = x.User != null && x.User.Department != null ? x.User.Department.Name : "",
                        PositionName = x.User != null && x.User.Position != null ? x.User.Position.Name : "",
                        RoleName = x.User != null && x.User.Role != null ? x.User.Role.Name : ""
                    }).ToListAsync();

                return new Tuple<int, List<ProjectUserResponseDto>, string>(1, users, "Users retrieved successfully");
            }
            catch (Exception ex)
            {
                return new Tuple<int, List<ProjectUserResponseDto>, string>(0, new List<ProjectUserResponseDto>(), ex.Message);
            }
        }
    }
}