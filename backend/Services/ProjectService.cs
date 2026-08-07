using backend.Dto.ProjectDtos;
using backend.Entities;
using backend.GenericResponse;
using backend.IRepository;
using backend.IService;

using Microsoft.EntityFrameworkCore;

namespace backend.Services;

internal sealed class ProjectService(IProjectRepository projectRepository, IUnitOfWork unitOfWork) : IProjectService
{
    public async Task<ServiceResponse<object>> CreateProject(ProjectDto dto, CancellationToken cancellationToken)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(dto);

            var projectExists = await projectRepository.ProjectExistsAsync(dto.Name, cancellationToken).ConfigureAwait(false);

            if (projectExists)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.ProjectAlreadyExists };
            }

            var managerExists = await projectRepository.ManagerExistsAsync(dto.ProjectManagerId, cancellationToken).ConfigureAwait(false);

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

            await projectRepository.AddAsync(project, cancellationToken).ConfigureAwait(false);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new ServiceResponse<object> { IsSuccess = true, StatusCode = CustomCodes.ProjectCreatedSuccessfully };
        }
        catch (OperationCanceledException)
        {
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.OperationCancelled };
        }
        catch (DbUpdateException)
        {
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.ProjectCreationFailed };
        }
        catch (Exception)
        {
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.ProjectCreationFailed };
            throw;
        }
    }

    public async Task<ServiceResponse<object>> UpdateProject(ProjectDto dto, CancellationToken cancellationToken)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(dto);

            var project = await projectRepository.GetByIdAsync(dto.Id, cancellationToken).ConfigureAwait(false);

            if (project == null)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.ProjectNotFound };
            }

            var managerExists = await projectRepository.ManagerExistsAsync(dto.ProjectManagerId, cancellationToken).ConfigureAwait(false);

            if (!managerExists)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.ProjectManagerNotFound };
            }

            var duplicateProject = await projectRepository.DuplicateProjectExistsAsync(dto.Id, dto.Name, cancellationToken).ConfigureAwait(false);

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

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new ServiceResponse<object> { IsSuccess = true, StatusCode = CustomCodes.ProjectUpdatedSuccessfully };
        }
        catch (OperationCanceledException)
        {
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.OperationCancelled };
        }
        catch (DbUpdateException)
        {
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.ProjectUpdateFailed };
        }
        catch (Exception)
        {
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.ProjectUpdateFailed };
            throw;
        }
    }

    public async Task<ServiceResponse<IReadOnlyCollection<ProjectResponseDto>>> GetAllProjects(CancellationToken cancellationToken)
    {
        try
        {
            var projects = await projectRepository.GetAllProjectsAsync(cancellationToken).ConfigureAwait(false);

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

    public async Task<ServiceResponse<ProjectResponseDto?>> GetProjectById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var project = await projectRepository.GetProjectByIdAsync(id, cancellationToken).ConfigureAwait(false);

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

    public async Task<ServiceResponse<IReadOnlyCollection<ProjectUserResponseDto>>> GetProjectEmployees(Guid projectId, CancellationToken cancellationToken)
    {
        try
        {
            var projectExists = await projectRepository.ProjectExistsByIdAsync(projectId, cancellationToken).ConfigureAwait(false);

            if (!projectExists)
            {
                return new ServiceResponse<IReadOnlyCollection<ProjectUserResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.ProjectNotFound };
            }

            var users = await projectRepository.GetProjectEmployeesAsync(projectId, cancellationToken).ConfigureAwait(false);

            return new ServiceResponse<IReadOnlyCollection<ProjectUserResponseDto>> { IsSuccess = true, StatusCode = CustomCodes.DataRetrieved, Data = users };
        }
        catch (Exception)
        {
            return new ServiceResponse<IReadOnlyCollection<ProjectUserResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.InternalServerError };
            throw;
        }
    }
}
