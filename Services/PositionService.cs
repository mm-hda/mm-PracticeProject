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
        using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

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
                .PositionExistsAsync(dto.Name, dto.DepartmentId, cancellationToken)
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

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return new ServiceResponse<object> { IsSuccess = true, StatusCode = CustomCodes.PositionCreatedSuccessfully };
        }
        catch (OperationCanceledException)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);

            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.OperationCancelled };
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);

            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.PositionCreationFailed };
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);

            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.PositionCreationFailed };
            throw;
        }
    }

    public async Task<ServiceResponse<object>> UpdatePosition(PositionDto dto, CancellationToken cancellationToken)
    {
        using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

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

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return new ServiceResponse<object> { IsSuccess = true, StatusCode = CustomCodes.PositionUpdatedSuccessfully };
        }
        catch (OperationCanceledException)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);

            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.OperationCancelled };
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);

            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.PositionUpdateFailed };
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);

            return new ServiceResponse<object> { IsSuccess = false, StatusCode = CustomCodes.PositionUpdateFailed };
            throw;
        }
    }

    public async Task<ServiceResponse<IReadOnlyCollection<PositionResponseDto>>> GetAllPositions()
    {
        try
        {
            var positions = await positionRepository
                .GetAllPositionsAsync()
                .ConfigureAwait(false);

            return new ServiceResponse<IReadOnlyCollection<PositionResponseDto>>
            {
                IsSuccess = true,
                StatusCode = CustomCodes.DataRetrieved,
                Data = positions
            };
        }
        catch (Exception)
        {
            return new ServiceResponse<IReadOnlyCollection<PositionResponseDto>>
            {
                IsSuccess = false,
                StatusCode = CustomCodes.InternalServerError
            };
            throw;
        }
    }

    public async Task<ServiceResponse<PositionResponseDto?>> GetPositionById(Guid id)
    {
        try
        {
            var position = await positionRepository
                .GetPositionByIdAsync(id)
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
        catch (Exception)
        {
            return new ServiceResponse<PositionResponseDto?>
            {
                IsSuccess = false,
                StatusCode = CustomCodes.InternalServerError
            };
            throw;
        }
    }

    public async Task<ServiceResponse<IReadOnlyCollection<PositionResponseDto>>> GetPositionsByDepartment(Guid departmentId)
    {
        try
        {
            var departmentExists = await positionRepository
                .DepartmentExistsAsync(departmentId, CancellationToken.None)
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
                .GetPositionsByDepartmentAsync(departmentId)
                .ConfigureAwait(false);

            return new ServiceResponse<IReadOnlyCollection<PositionResponseDto>>
            {
                IsSuccess = true,
                StatusCode = CustomCodes.DataRetrieved,
                Data = positions
            };
        }
        catch (Exception)
        {
            return new ServiceResponse<IReadOnlyCollection<PositionResponseDto>>
            {
                IsSuccess = false,
                StatusCode = CustomCodes.InternalServerError
            };
            throw;
        }
    }

    public async Task<ServiceResponse<IReadOnlyCollection<PositionUserResponseDto>>> GetPositionUsers(Guid positionId)
    {
        try
        {
            var positionExists = await positionRepository
                .PositionExistsAsync(positionId)
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
                .GetPositionUsersAsync(positionId)
                .ConfigureAwait(false);

            return new ServiceResponse<IReadOnlyCollection<PositionUserResponseDto>>
            {
                IsSuccess = true,
                StatusCode = CustomCodes.DataRetrieved,
                Data = users
            };
        }
        catch (Exception)
        {
            return new ServiceResponse<IReadOnlyCollection<PositionUserResponseDto>>
            {
                IsSuccess = false,
                StatusCode = CustomCodes.InternalServerError
            };
            throw;
        }
    }
}
