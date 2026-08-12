using backend.Dto.DepartmentDtos;
using backend.Entities;
using backend.GenericResponse;
using backend.IRepository;
using backend.IService;

using Microsoft.EntityFrameworkCore;

namespace backend.Services;

internal sealed class DepartmentService(IDepartmentRepository departmentRepository, IUnitOfWork unitOfWork) : IDepartmentService
{
    public async Task<ServiceResponse<object>> CreateDepartment(DepartmentDto dto, CancellationToken cancellationToken)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(dto);

            var exists = await departmentRepository.DepartmentExistsAsync(dto.Name, cancellationToken).ConfigureAwait(false);

            if (exists)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.DepartmentAlreadyExists };
            }

            Department department = new()
            {
                Id = Guid.NewGuid(),
                Name = dto.Name ?? ""
            };

            await departmentRepository.AddAsync(department, cancellationToken).ConfigureAwait(false);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new ServiceResponse<object> { IsSuccess = true, StatusCode = CustomCodes.DepartmentCreatedSuccessfully };
        }
        catch (OperationCanceledException)
        {
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.OperationCancelled };
        }
        catch (NullReferenceException)
        {
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.DepartmentCreationFailed };
        }
    }

    public async Task<ServiceResponse<object>> UpdateDepartment(DepartmentDto dto, CancellationToken cancellationToken)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(dto);

            var existing = await departmentRepository.GetByIdAsync(dto.Id, cancellationToken).ConfigureAwait(false);

            if (existing == null)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.DepartmentNotFound };
            }

            var duplicate = await departmentRepository.DuplicateDepartmentExistsAsync(dto.Id, dto.Name, cancellationToken).ConfigureAwait(false);

            if (duplicate)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.DepartmentAlreadyExists };
            }

            existing.Name = dto.Name ?? "";

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new ServiceResponse<object> { IsSuccess = true, StatusCode = CustomCodes.DepartmentUpdatedSuccessfully };
        }
        catch (OperationCanceledException)
        {
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.OperationCancelled };
        }
    }

    public async Task<ServiceResponse<IReadOnlyCollection<DepartmentResponseDto>>> GetAllDepartments(CancellationToken cancellationToken)
    {
        var departments = await departmentRepository.GetAllDepartmentsAsync(cancellationToken).ConfigureAwait(false);

        return new ServiceResponse<IReadOnlyCollection<DepartmentResponseDto>> { IsSuccess = true, StatusCode = CustomCodes.DataRetrieved, Data = departments };
    }

    public async Task<ServiceResponse<DepartmentResponseDto?>> GetDepartmentById(Guid id, CancellationToken cancellationToken)
    {
        var department = await departmentRepository.GetDepartmentByIdAsync(id, cancellationToken).ConfigureAwait(false);

        if (department == null)
        {
            return new ServiceResponse<DepartmentResponseDto?> { IsSuccess = false, StatusCode = CustomCodes.DepartmentNotFound, Data = null };
        }

        return new ServiceResponse<DepartmentResponseDto?> { IsSuccess = true, StatusCode = CustomCodes.DataRetrieved, Data = department };
    }

    public async Task<ServiceResponse<IReadOnlyCollection<DepartmentUserResponseDto>>> GetDepartmentEmployees(Guid departmentId, CancellationToken cancellationToken)
    {
        var departmentExists = await departmentRepository.DepartmentExistsByIdAsync(departmentId, cancellationToken).ConfigureAwait(false);

        if (!departmentExists)
        {
            return new ServiceResponse<IReadOnlyCollection<DepartmentUserResponseDto>> { IsSuccess = false, StatusCode = CustomCodes.DepartmentNotFound };
        }

        var users = await departmentRepository.GetDepartmentEmployeesAsync(departmentId, cancellationToken).ConfigureAwait(false);

        return new ServiceResponse<IReadOnlyCollection<DepartmentUserResponseDto>> { IsSuccess = true, StatusCode = CustomCodes.DataRetrieved, Data = users };
    }
}
