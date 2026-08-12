using backend.Dto.PositionDtos;
using backend.Entities;
using backend.GenericResponse;
using backend.IRepository;
using backend.IService;

using Microsoft.EntityFrameworkCore;

namespace backend.Services;

internal sealed class PositionService(IPositionRepository positionRepository, IUnitOfWork unitOfWork) : IPositionService
{
    public async Task<ServiceResponse<object>> CreatePosition(PositionDto dto, CancellationToken cancellationToken)
    {

        try
        {
            ArgumentNullException.ThrowIfNull(dto);

            var departmentExists = await positionRepository
                .DepartmentExistsAsync(dto.DepartmentId, cancellationToken)
                .ConfigureAwait(false);

            if (!departmentExists)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.DepartmentNotFound };
            }

            var exists = await positionRepository
                .PositionExistsAsync(dto.Name, cancellationToken)
                .ConfigureAwait(false);

            if (exists)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.PositionAlreadyExists };
            }

            Position position = new()
            {
                Id = Guid.NewGuid(),
                Name = dto.Name ?? "",
                DepartmentId = dto.DepartmentId
            };

            await positionRepository.AddAsync(position, cancellationToken).ConfigureAwait(false);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new ServiceResponse<object> { IsSuccess = true, StatusCode = CustomCodes.PositionCreatedSuccessfully };
        }
        catch (OperationCanceledException)
        {
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.OperationCancelled };
        }
        catch (DbUpdateException)
        {
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.PositionCreationFailed };
        }
    }

    public async Task<ServiceResponse<object>> UpdatePosition(PositionDto dto, CancellationToken cancellationToken)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(dto);

            var position = await positionRepository
                .GetByIdAsync(dto.Id, cancellationToken)
                .ConfigureAwait(false);

            if (position == null)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.PositionNotFound };
            }

            var departmentExists = await positionRepository
                .DepartmentExistsAsync(dto.DepartmentId, cancellationToken)
                .ConfigureAwait(false);

            if (!departmentExists)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.DepartmentNotFound };
            }

            var duplicate = await positionRepository
                .DuplicatePositionExistsAsync(dto.Id, dto.Name, dto.DepartmentId, cancellationToken)
                .ConfigureAwait(false);

            if (duplicate)
            {
                return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.PositionAlreadyExists };
            }

            position.Name = dto.Name ?? "";
            position.DepartmentId = dto.DepartmentId;

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new ServiceResponse<object> { IsSuccess = true, StatusCode = CustomCodes.PositionUpdatedSuccessfully };
        }
        catch (OperationCanceledException)
        {
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.OperationCancelled };
        }
        catch (DbUpdateException)
        {
            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.PositionUpdateFailed };
        }
    }

    public async Task<ServiceResponse<IReadOnlyCollection<PositionResponseDto>>> GetAllPositions(CancellationToken cancellationToken)
    {
        var positions = await positionRepository
            .GetAllPositionsAsync(cancellationToken)
            .ConfigureAwait(false);

        return new ServiceResponse<IReadOnlyCollection<PositionResponseDto>>
        {
            IsSuccess = true,
            StatusCode = CustomCodes.DataRetrieved,
            Data = positions
        };
    }

    public async Task<ServiceResponse<PositionResponseDto?>> GetPositionById(Guid id, CancellationToken cancellationToken)
    {
        var position = await positionRepository
            .GetPositionByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        if (position == null)
        {
            return new ServiceResponse<PositionResponseDto?>
            {
                IsSuccess = false,
                StatusCode = CustomCodes.PositionNotFound
            };
        }

        return new ServiceResponse<PositionResponseDto?>
        {
            IsSuccess = true,
            StatusCode = CustomCodes.DataRetrieved,
            Data = position
        };
    }

    public async Task<ServiceResponse<IReadOnlyCollection<PositionResponseDto>>> GetPositionsByDepartment(Guid departmentId, CancellationToken cancellationToken)
    {
        var departmentExists = await positionRepository
            .DepartmentExistsAsync(departmentId, cancellationToken)
            .ConfigureAwait(false);

        if (!departmentExists)
        {
            return new ServiceResponse<IReadOnlyCollection<PositionResponseDto>>
            {
                IsSuccess = false,
                StatusCode = CustomCodes.DepartmentNotFound
            };
        }

        var positions = await positionRepository
            .GetPositionsByDepartmentAsync(departmentId, cancellationToken)
            .ConfigureAwait(false);

        return new ServiceResponse<IReadOnlyCollection<PositionResponseDto>>
        {
            IsSuccess = true,
            StatusCode = CustomCodes.DataRetrieved,
            Data = positions
        };
    }

    public async Task<ServiceResponse<IReadOnlyCollection<PositionUserResponseDto>>> GetPositionUsers(Guid positionId, CancellationToken cancellationToken)
    {
        var positionExists = await positionRepository
            .PositionExistsAsync(positionId, cancellationToken)
            .ConfigureAwait(false);

        if (!positionExists)
        {
            return new ServiceResponse<IReadOnlyCollection<PositionUserResponseDto>>
            {
                IsSuccess = false,
                StatusCode = CustomCodes.PositionNotFound
            };
        }

        var users = await positionRepository
            .GetPositionUsersAsync(positionId, cancellationToken)
            .ConfigureAwait(false);

        return new ServiceResponse<IReadOnlyCollection<PositionUserResponseDto>>
        {
            IsSuccess = true,
            StatusCode = CustomCodes.DataRetrieved,
            Data = users
        };
    }
}
