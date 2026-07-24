using backend.Data;
using backend.Dto.ProjectDtos;
using backend.Entities;
using backend.IService;
using backend.GenericResponse;

using Microsoft.EntityFrameworkCore;

namespace backend.Services;

internal sealed class ProjectService(AppDbContext context) : IProjectService
{
    public async Task<ServiceResponse<object>> CreateProject(ProjectDto dto, CancellationToken cancellationToken)
    {
        using var transaction = await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ArgumentNullException.ThrowIfNull(dto);

            var projectExists = await context.Projects.AnyAsync(x => x.Name.Equals(dto.Name, StringComparison.OrdinalIgnoreCase), cancellationToken).ConfigureAwait(false);

            if (projectExists)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.ProjectAlreadyExists };
            }

            var managerExists = await context.Users.AnyAsync(x => x.Id == dto.ProjectManagerId && x.Role != null && x.Role.Name == "Manager", cancellationToken).ConfigureAwait(false);

            if (!managerExists)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.ProjectManagerNotFound };
            }

            Project project = new()
            {
                Id = Guid.NewGuid(),
                Name = dto.Name ?? "",
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                ProjectManagerId = dto.ProjectManagerId
            };

            await context.Projects.AddAsync(project, cancellationToken).ConfigureAwait(false);

            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return new ServiceResponse<object> { IsSuccess = true, StatusCode = CustomCodes.ProjectCreatedSuccessfully };
        }
        catch (OperationCanceledException)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.OperationCancelled };
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.ProjectCreationFailed };
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.ProjectCreationFailed };
            throw;
        }
    }

    public async Task<ServiceResponse<object>> UpdateProject(ProjectDto dto, CancellationToken cancellationToken)
    {
        using var transaction = await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ArgumentNullException.ThrowIfNull(dto);

            var project = await context.Projects.FirstOrDefaultAsync(x => x.Id == dto.Id, cancellationToken).ConfigureAwait(false);

            if (project == null)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.ProjectNotFound };
            }

            var managerExists = await context.Users.AnyAsync(x => x.Id == dto.ProjectManagerId && x.Role != null && x.Role.Name == "Manager", cancellationToken).ConfigureAwait(false);

            if (!managerExists)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.ProjectManagerNotFound };
            }

            var duplicateProject = await context.Projects.AnyAsync(x => x.Id != dto.Id && x.Name.Equals(dto.Name, StringComparison.OrdinalIgnoreCase), cancellationToken).ConfigureAwait(false);

            if (duplicateProject)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.ProjectAlreadyExists };
            }

            if (dto.EndDate != null && dto.EndDate < dto.StartDate)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.InvalidInput };
            }

            project.Name = dto.Name ?? "";
            project.Description = dto.Description;
            project.StartDate = dto.StartDate;
            project.EndDate = dto.EndDate;
            project.ProjectManagerId = dto.ProjectManagerId;

            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return new ServiceResponse<object> { IsSuccess = true, StatusCode = CustomCodes.ProjectUpdatedSuccessfully };
        }
        catch (OperationCanceledException)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.OperationCancelled };
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.ProjectUpdateFailed };
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.ProjectUpdateFailed };
            throw;
        }
    }

    public async Task<ServiceResponse<IReadOnlyCollection<ProjectResponseDto>>> GetAllProjects()
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

            if (projects == null || projects.Count == 0)
            {
                return new ServiceResponse<IReadOnlyCollection<ProjectResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.ProjectNotFound };
            }

            return new ServiceResponse<IReadOnlyCollection<ProjectResponseDto>> { IsSuccess = true, StatusCode = CustomCodes.DataRetrieved, Data = projects };
        }
        catch (Exception)
        {
            return new ServiceResponse<IReadOnlyCollection<ProjectResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.InternalServerError };
            throw;
        }
    }

    public async Task<ServiceResponse<ProjectResponseDto?>> GetProjectById(Guid id)
    {
        try
        {
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
                return new ServiceResponse<ProjectResponseDto?> { IsSuccess = false, StatusCode = CustomCodes.ProjectNotFound };
            }

            return new ServiceResponse<ProjectResponseDto?> { IsSuccess = true, StatusCode = CustomCodes.DataRetrieved, Data = project };
        }
        catch (Exception)
        {
            return new ServiceResponse<ProjectResponseDto?> { IsSuccess = false, StatusCode = CustomCodes.InternalServerError };
            throw;
        }
    }

    public async Task<ServiceResponse<IReadOnlyCollection<ProjectUserResponseDto>>> GetProjectEmployees(Guid projectId)
    {
        try
        {

            var projectExists = await context.Projects.AnyAsync(x => x.Id == projectId).ConfigureAwait(false);

            if (!projectExists)
            {
                return new ServiceResponse<IReadOnlyCollection<ProjectUserResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.ProjectNotFound };
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

            return new ServiceResponse<IReadOnlyCollection<ProjectUserResponseDto>> { IsSuccess = true, StatusCode = CustomCodes.DataRetrieved, Data = users };
        }
        catch (Exception)
        {
            return new ServiceResponse<IReadOnlyCollection<ProjectUserResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.InternalServerError };
            throw;
        }
    }
}
