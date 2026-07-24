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
    public async Task<ServiceResponse<object>> CreateEmployeeProject(EmployeeProjectDto dto, CancellationToken cancellationToken)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ArgumentNullException.ThrowIfNull(dto);

            var userExists = await _context.Users.AnyAsync(x => x.Id == dto.UserId, cancellationToken).ConfigureAwait(false);

            if (!userExists)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.UserNotFound };
            }

            var project = await _context.Projects.FirstOrDefaultAsync(x => x.Id == dto.ProjectId, cancellationToken).ConfigureAwait(false);

            if (project == null)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.ProjectNotFound };
            }

            if (project.EndDate != null && project.EndDate < DateTime.UtcNow)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.ProjectEnded };
            }

            var alreadyAssigned = await _context.EmployeeProjects.AnyAsync(x => x.UserId == dto.UserId && x.ProjectId == dto.ProjectId, cancellationToken).ConfigureAwait(false);

            if (alreadyAssigned)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.UserAlreadyAssignedToProject };
            }

            EmployeeProject employeeProject = new()
            {
                Id = Guid.NewGuid(),
                UserId = dto.UserId,
                ProjectId = dto.ProjectId,
                AssignedDate = DateTime.UtcNow
            };

            await _context.EmployeeProjects.AddAsync(employeeProject, cancellationToken).ConfigureAwait(false);

            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return new ServiceResponse<object> { IsSuccess = true, StatusCode = CustomCodes.EmployeeProjectCreatedSuccessfully };
        }
        catch (OperationCanceledException)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.OperationCancelled };
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.EmployeeProjectCreationFailed };
            throw;
        }
    }

    public async Task<ServiceResponse<object>> RemoveEmployeeProject(Guid id, CancellationToken cancellationToken)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var employeeProject = await _context.EmployeeProjects.FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);

            if (employeeProject == null)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.EmployeeProjectNotFound };
            }

            var project = await _context.Projects.FirstOrDefaultAsync(x => x.Id == employeeProject.ProjectId, cancellationToken).ConfigureAwait(false);

            if (project == null)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.ProjectNotFound };
            }

            if (project.EndDate != null && project.EndDate < DateTime.UtcNow)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.ProjectEnded };
            }

            if (employeeProject.AssignedDate < DateTime.UtcNow.AddDays(-30))
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.InvalidInput };
            }

            _context.EmployeeProjects.Remove(employeeProject);

            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { IsSuccess = true, StatusCode = CustomCodes.EmployeeProjectRemovedSuccessfully };
        }
        catch (OperationCanceledException)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.OperationCancelled };
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.EmployeeProjectRemovalFailed };
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.EmployeeProjectRemovalFailed };
            throw;
        }
    }

    public async Task<ServiceResponse<IReadOnlyCollection<EmployeeProjectResponseDto>>> GetAllEmployeeProjects()
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

            return new ServiceResponse<IReadOnlyCollection<EmployeeProjectResponseDto>> { IsSuccess = true, StatusCode = CustomCodes.DataRetrieved, Data = employeeProjects };
        }
        catch (Exception)
        {
            return new ServiceResponse<IReadOnlyCollection<EmployeeProjectResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.InternalServerError };
            throw;
        }
    }

    public async Task<ServiceResponse<IReadOnlyCollection<ProjectResponseDto>>> GetUserProjectsByUserId(Guid userId, PaginationDto dto)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(dto);

            dto.PageNumber = dto.PageNumber <= 0 ? 1 : dto.PageNumber;
            dto.PageSize = dto.PageSize <= 0 ? 10 : dto.PageSize;

            var userExists = await _context.Users.AnyAsync(x => x.Id == userId).ConfigureAwait(false);

            if (!userExists)
            {
                return new ServiceResponse<IReadOnlyCollection<ProjectResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.UserNotFound };
            }

            var query = _context.EmployeeProjects.AsNoTracking().Where(x => x.UserId == userId);

            if (query == null)
            {
                return new ServiceResponse<IReadOnlyCollection<ProjectResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.EmployeeProjectNotFound };
            }

            var totalRecords = await query.CountAsync().ConfigureAwait(false);

            if ((int)Math.Ceiling(totalRecords / (double)dto.PageSize) < dto.PageNumber)
            {
                return new ServiceResponse<IReadOnlyCollection<ProjectResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.PageNumberExceeds };
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

            return new ServiceResponse<IReadOnlyCollection<ProjectResponseDto>> { IsSuccess = true, StatusCode = CustomCodes.DataRetrieved, Data = projects, Meta = meta };
        }
        catch (Exception)
        {
            return new ServiceResponse<IReadOnlyCollection<ProjectResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.InternalServerError };
            throw;
        }
    }
}
