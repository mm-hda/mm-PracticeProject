using backend.Data;
using backend.Dto.EmployeeProjectDtos;
using backend.Dto.ProjectDtos;
using backend.Entities;
using backend.IService;
using Microsoft.EntityFrameworkCore;
using backend.Dto.CommonDtos;
using backend.GenericResponse;

namespace backend.Services;

internal sealed class EmployeeProjectService(AppDbContext _context) : IEmployeeProjectService
{
    public async Task<Tuple<int>> CreateEmployeeProject(EmployeeProjectDto dto)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (dto == null)
            {
                return new Tuple<int>(CustomCodes.InputsNotFound);
            }

            if (dto.UserId == Guid.Empty)
            {
                return new Tuple<int>(CustomCodes.InputsNotFound);
            }

            if (dto.ProjectId == Guid.Empty)
            {
                return new Tuple<int>(CustomCodes.InputsNotFound);
            }

            var userExists = await _context.Users.AnyAsync(x => x.Id == dto.UserId).ConfigureAwait(false);

            if (!userExists)
            {
                return new Tuple<int>(CustomCodes.UserNotFound);
            }

            var project = await _context.Projects.FirstOrDefaultAsync(x => x.Id == dto.ProjectId).ConfigureAwait(false);

            if (project == null)
            {
                return new Tuple<int>(CustomCodes.ProjectNotFound);
            }

            if (project.EndDate != null && project.EndDate < DateTime.UtcNow)
            {
                return new Tuple<int>(CustomCodes.ProjectEnded);
            }

            var alreadyAssigned = await _context.EmployeeProjects.AnyAsync(x => x.UserId == dto.UserId && x.ProjectId == dto.ProjectId).ConfigureAwait(false);

            if (alreadyAssigned)
            {
                return new Tuple<int>(CustomCodes.UserAlreadyAssignedToProject);
            }

            EmployeeProject employeeProject = new()
            {
                Id = Guid.NewGuid(),
                UserId = dto.UserId,
                ProjectId = dto.ProjectId,
                AssignedDate = DateTime.UtcNow
            };

            await _context.EmployeeProjects.AddAsync(employeeProject).ConfigureAwait(false);

            await _context.SaveChangesAsync().ConfigureAwait(false);

            return new Tuple<int>(CustomCodes.EmployeeProjectCreatedSuccessfully);
        }
        catch (Exception)
        {
            return new Tuple<int>(CustomCodes.EmployeeProjectCreationFailed);
            throw;
        }
    }

    public async Task<Tuple<int>> RemoveEmployeeProject(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return new Tuple<int>(CustomCodes.InvalidInput);
            }

            var employeeProject = await _context.EmployeeProjects.FirstOrDefaultAsync(x => x.Id == id).ConfigureAwait(false);

            if (employeeProject == null)
            {
                return new Tuple<int>(CustomCodes.EmployeeProjectNotFound);
            }

            var project = await _context.Projects.FirstOrDefaultAsync(x => x.Id == employeeProject.ProjectId).ConfigureAwait(false);

            if (project == null)
            {
                return new Tuple<int>(CustomCodes.ProjectNotFound);
            }

            if (project.EndDate != null && project.EndDate < DateTime.UtcNow)
            {
                return new Tuple<int>(CustomCodes.ProjectEnded);
            }

            if (employeeProject.AssignedDate < DateTime.UtcNow.AddDays(-30))
            {
                return new Tuple<int>(CustomCodes.InvalidInput);
            }

            _context.EmployeeProjects.Remove(employeeProject);

            await _context.SaveChangesAsync().ConfigureAwait(false);

            return new Tuple<int>(CustomCodes.EmployeeProjectRemovedSuccessfully);
        }
        catch (Exception)
        {
            return new Tuple<int>(CustomCodes.EmployeeProjectRemovalFailed);
            throw;
        }
    }

    public async Task<Tuple<int, IReadOnlyCollection<EmployeeProjectResponseDto>>> GetAllEmployeeProjects()
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
                }).ToListAsync().ConfigureAwait(false);

            return new Tuple<int, IReadOnlyCollection<EmployeeProjectResponseDto>>(CustomCodes.DataRetrieved, employeeProjects);
        }
        catch (Exception)
        {
            return new Tuple<int, IReadOnlyCollection<EmployeeProjectResponseDto>>(CustomCodes.InternalServerError, []);
            throw;
        }
    }

    public async Task<Tuple<int, IReadOnlyCollection<ProjectResponseDto>, PaginationMetaDto?>> GetUserProjectsByUserId(Guid userId, PaginationDto dto)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(dto);

            dto.PageNumber = dto.PageNumber <= 0 ? 1 : dto.PageNumber;
            dto.PageSize = dto.PageSize <= 0 ? 10 : dto.PageSize;

            if (userId == Guid.Empty)
            {
                return new Tuple<int, IReadOnlyCollection<ProjectResponseDto>, PaginationMetaDto?>(CustomCodes.InvalidInput, [], null);
            }

            var userExists = await _context.Users.AnyAsync(x => x.Id == userId).ConfigureAwait(false);

            if (!userExists)
            {
                return new Tuple<int, IReadOnlyCollection<ProjectResponseDto>, PaginationMetaDto?>(CustomCodes.UserNotFound, [], null);
            }

            var query = _context.EmployeeProjects.AsNoTracking().Where(x => x.UserId == userId);

            if (query == null)
            {
                return new Tuple<int, IReadOnlyCollection<ProjectResponseDto>, PaginationMetaDto?>(CustomCodes.EmployeeProjectNotFound, [], null);
            }

            var totalRecords = await query.CountAsync().ConfigureAwait(false);

            if ((int)Math.Ceiling(totalRecords / (double)dto.PageSize) < dto.PageNumber)
            {
                return new Tuple<int, IReadOnlyCollection<ProjectResponseDto>, PaginationMetaDto?>(CustomCodes.PageNumberExceeds, [], null);
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
                }).ToListAsync().ConfigureAwait(false);

            var meta = new PaginationMetaDto
            {
                PageNumber = dto.PageNumber,
                PageSize = dto.PageSize,
                TotalRecords = projects.Count,
                TotalPages = (int)Math.Ceiling(projects.Count / (double)dto.PageSize)
            };

            return new Tuple<int, IReadOnlyCollection<ProjectResponseDto>, PaginationMetaDto?>(CustomCodes.DataRetrieved, projects, meta);
        }
        catch (Exception)
        {
            return new Tuple<int, IReadOnlyCollection<ProjectResponseDto>, PaginationMetaDto?>(CustomCodes.InternalServerError, [], null);
            throw;
        }
    }
}
