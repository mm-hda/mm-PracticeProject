using backend.Dto.CommonDtos;
using backend.Dto.EmployeeProjectDtos;
using backend.Dto.ProjectDtos;
using backend.Entities;
using backend.GenericResponse;
using backend.IRepository;
using backend.IService;

using Microsoft.EntityFrameworkCore;

namespace backend.Services;

internal sealed class EmployeeProjectService(IEmployeeProjectRepository employeeProjectRepository, IUnitOfWork unitOfWork) : IEmployeeProjectService
{
    public async Task<ServiceResponse<object>> CreateEmployeeProject(EmployeeProjectDto dto, CancellationToken cancellationToken)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(dto);

            var userExists = await employeeProjectRepository.UserExistsAsync(dto.UserId, cancellationToken).ConfigureAwait(false);

            if (!userExists)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.UserNotFound };
            }

            var project = await employeeProjectRepository.GetProjectByIdAsync(dto.ProjectId, cancellationToken).ConfigureAwait(false);

            if (project == null)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.ProjectNotFound };
            }

            if (project.EndDate != null && project.EndDate < DateTime.UtcNow)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.ProjectEnded };
            }

            var alreadyAssigned = await employeeProjectRepository.EmployeeProjectExistsAsync(dto.UserId, dto.ProjectId, cancellationToken).ConfigureAwait(false);

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

            await employeeProjectRepository.AddEmployeeProjectAsync(employeeProject, cancellationToken).ConfigureAwait(false);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new ServiceResponse<object> { IsSuccess = true, StatusCode = CustomCodes.EmployeeProjectCreatedSuccessfully };
        }
        catch (OperationCanceledException)
        {
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.OperationCancelled };
        }
        catch (Exception)
        {
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.EmployeeProjectCreationFailed };
            throw;
        }
    }

    public async Task<ServiceResponse<object>> RemoveEmployeeProject(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var employeeProject = await employeeProjectRepository.GetEmployeeProjectByIdAsync(id, cancellationToken).ConfigureAwait(false);

            if (employeeProject == null)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.EmployeeProjectNotFound };
            }

            var project = await employeeProjectRepository.GetProjectByIdAsync(employeeProject.ProjectId, cancellationToken).ConfigureAwait(false);

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

            employeeProjectRepository.Remove(employeeProject, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new ServiceResponse<object> { IsSuccess = true, StatusCode = CustomCodes.EmployeeProjectRemovedSuccessfully };
        }
        catch (OperationCanceledException)
        {
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.OperationCancelled };
        }
        catch (DbUpdateException)
        {
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.EmployeeProjectRemovalFailed };
        }
        catch (Exception)
        {
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.EmployeeProjectRemovalFailed };
            throw;
        }
    }

    public async Task<ServiceResponse<IReadOnlyCollection<EmployeeProjectResponseDto>>> GetAllEmployeeProjects(CancellationToken cancellationToken)
    {
        try
        {
            var employeeProjects = await employeeProjectRepository.GetAllEmployeeProjectsAsync(cancellationToken).ConfigureAwait(false);

            return new ServiceResponse<IReadOnlyCollection<EmployeeProjectResponseDto>> { IsSuccess = true, StatusCode = CustomCodes.DataRetrieved, Data = employeeProjects };
        }
        catch (Exception)
        {
            return new ServiceResponse<IReadOnlyCollection<EmployeeProjectResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.InternalServerError };
            throw;
        }
    }

    public async Task<ServiceResponse<IReadOnlyCollection<ProjectResponseDto>>> GetUserProjectsByUserId(Guid userId, PaginationDto dto, CancellationToken cancellationToken)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(dto);

            dto.PageNumber = dto.PageNumber <= 0 ? 1 : dto.PageNumber;
            dto.PageSize = dto.PageSize <= 0 ? 10 : dto.PageSize;

            var userExists = await employeeProjectRepository.UserExistsAsync(userId, cancellationToken).ConfigureAwait(false);

            if (!userExists)
            {
                return new ServiceResponse<IReadOnlyCollection<ProjectResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.UserNotFound };
            }

            var totalRecords = await employeeProjectRepository.GetUserProjectsCountAsync(userId, cancellationToken).ConfigureAwait(false);

            if ((int)Math.Ceiling(totalRecords / (double)dto.PageSize) < dto.PageNumber)
            {
                return new ServiceResponse<IReadOnlyCollection<ProjectResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.PageNumberExceeds };
            }

            var projects = await employeeProjectRepository.GetUserProjectsByUserIdAsync(userId, dto.PageNumber, dto.PageSize, cancellationToken).ConfigureAwait(false);

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
