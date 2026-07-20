using backend.Data;
using backend.Dto.ProjectDtos;
using backend.Entities;
using backend.IService;
using backend.GenericResponse;

using Microsoft.EntityFrameworkCore;

namespace backend.Services;

internal sealed class ProjectService(AppDbContext context) : IProjectService
{
    public async Task<Tuple<int>> CreateProject(ProjectDto dto)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (dto == null)
            {
                return new Tuple<int>(CustomCodes.InputsNotFound);
            }
            if (dto.EndDate < dto.StartDate)
            {
                return new Tuple<int>(CustomCodes.InvalidInput);
            }
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return new Tuple<int>(CustomCodes.InvalidInput);
            }

            if (dto.ProjectManagerId == Guid.Empty)
            {
                return new Tuple<int>(CustomCodes.InvalidInput);
            }

            var projectExists = await context.Projects.AnyAsync(x => x.Name.Equals(dto.Name, StringComparison.OrdinalIgnoreCase)).ConfigureAwait(false);

            if (projectExists)
            {
                return new Tuple<int>(CustomCodes.ProjectAlreadyExists);
            }

            var managerExists = await context.Users.AnyAsync(x => x.Id == dto.ProjectManagerId && x.Role != null && x.Role.Name == "Manager").ConfigureAwait(false);

            if (!managerExists)
            {
                return new Tuple<int>(CustomCodes.ProjectManagerNotFound);
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

            await context.Projects.AddAsync(project).ConfigureAwait(false);

            await context.SaveChangesAsync().ConfigureAwait(false);

            return new Tuple<int>(CustomCodes.ProjectCreatedSuccessfully);
        }
        catch (Exception)
        {
            return new Tuple<int>(CustomCodes.ProjectCreationFailed);
            throw;
        }
    }

    public async Task<Tuple<int>> UpdateProject(ProjectDto dto)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (dto == null)
            {
                return new Tuple<int>(CustomCodes.InvalidInput);
            }

            if (dto.Id == Guid.Empty)
            {
                return new Tuple<int>(CustomCodes.InvalidInput);
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return new Tuple<int>(CustomCodes.InvalidInput);
            }

            if (dto.ProjectManagerId == Guid.Empty)
            {
                return new Tuple<int>(CustomCodes.InvalidInput);
            }

            var project = await context.Projects.FirstOrDefaultAsync(x => x.Id == dto.Id).ConfigureAwait(false);

            if (project == null)
            {
                return new Tuple<int>(CustomCodes.ProjectNotFound);
            }

            var managerExists = await context.Users.AnyAsync(x => x.Id == dto.ProjectManagerId && x.Role != null && x.Role.Name == "Manager").ConfigureAwait(false);

            if (!managerExists)
            {
                return new Tuple<int>(CustomCodes.ProjectManagerNotFound);
            }

            var duplicateProject = await context.Projects.AnyAsync(x => x.Id != dto.Id && x.Name.Equals(dto.Name, StringComparison.OrdinalIgnoreCase)).ConfigureAwait(false);

            if (duplicateProject)
            {
                return new Tuple<int>(CustomCodes.ProjectAlreadyExists);
            }

            if (dto.EndDate != null && dto.EndDate < dto.StartDate)
            {
                return new Tuple<int>(CustomCodes.InvalidInput);
            }

            project.Name = dto.Name;
            project.Description = dto.Description;
            project.StartDate = dto.StartDate;
            project.EndDate = dto.EndDate;
            project.ProjectManagerId = dto.ProjectManagerId;

            await context.SaveChangesAsync().ConfigureAwait(false);

            return new Tuple<int>(CustomCodes.ProjectUpdatedSuccessfully);
        }
        catch (Exception)
        {
            return new Tuple<int>(CustomCodes.ProjectUpdateFailed);
            throw;
        }
    }

    public async Task<Tuple<int, List<ProjectResponseDto>>> GetAllProjects()
    {
        try
        {
            var projects = await context.Projects.AsNoTracking()
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
                    TotalUsers = context.EmployeeProjects.Count(ep => ep.ProjectId == x.Id)
                }).ToListAsync().ConfigureAwait(false);

            return new Tuple<int, List<ProjectResponseDto>>(CustomCodes.DataRetrieved, projects);
        }
        catch (Exception)
        {
            return new Tuple<int, List<ProjectResponseDto>>(CustomCodes.InternalServerError, []);
            throw;
        }
    }

    public async Task<Tuple<int, ProjectResponseDto?>> GetProjectById(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return new Tuple<int, ProjectResponseDto?>(CustomCodes.InvalidInput, null);
            }

            var project = await context.Projects.AsNoTracking()
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
                    TotalUsers = context.EmployeeProjects.Count(ep => ep.ProjectId == x.Id)
                }).FirstOrDefaultAsync().ConfigureAwait(false);

            if (project == null)
            {
                return new Tuple<int, ProjectResponseDto?>(CustomCodes.ProjectNotFound, null);
            }

            return new Tuple<int, ProjectResponseDto?>(CustomCodes.DataRetrieved, project);
        }
        catch (Exception)
        {
            return new Tuple<int, ProjectResponseDto?>(CustomCodes.InternalServerError, null);
            throw;
        }
    }

    public async Task<Tuple<int, List<ProjectUserResponseDto>>> GetProjectEmployees(Guid projectId)
    {
        try
        {
            if (projectId == Guid.Empty)
            {
                return new Tuple<int, List<ProjectUserResponseDto>>(CustomCodes.InvalidInput, []);
            }

            var projectExists = await context.Projects.AnyAsync(x => x.Id == projectId).ConfigureAwait(false);

            if (!projectExists)
            {
                return new Tuple<int, List<ProjectUserResponseDto>>(CustomCodes.ProjectNotFound, []);
            }

            var users = await context.EmployeeProjects.AsNoTracking()
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
                }).ToListAsync().ConfigureAwait(false);

            return new Tuple<int, List<ProjectUserResponseDto>>(CustomCodes.DataRetrieved, users);
        }
        catch (Exception)
        {
            return new Tuple<int, List<ProjectUserResponseDto>>(CustomCodes.InternalServerError, []);
            throw;
        }
    }
}
