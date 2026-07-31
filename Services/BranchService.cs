using backend.Dto.BranchDtos;
using backend.Entities;
using backend.GenericResponse;
using backend.IRepository;
using backend.IService;

using Microsoft.EntityFrameworkCore;

namespace backend.Services;

internal sealed class BranchService(IBranchRepository branchRepository, IUnitOfWork unitOfWork) : IBranchService
{
    public async Task<ServiceResponse<object>> CreateBranch(BranchDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);

        // using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var exists = await branchRepository.BranchExistsAsync(dto.Name, cancellationToken).ConfigureAwait(false);

            if (exists)
            {
                return new()
                {
                    StatusCode = CustomCodes.BranchAlreadyExists,
                    IsSuccess = false
                };
            }

            Branch branch = new()
            {
                Id = Guid.NewGuid(),
                Name = dto.Name ?? string.Empty,
                Location = dto.Location ?? string.Empty
            };

            await branchRepository.AddAsync(branch, cancellationToken).ConfigureAwait(false);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            // await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return new()
            {
                StatusCode = CustomCodes.BranchCreatedSuccessfully,
                IsSuccess = true
            };
        }
        catch (OperationCanceledException)
        {
            // await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);

            return new()
            {
                StatusCode = CustomCodes.OperationCancelled,
                IsSuccess = false
            };
        }
        catch (DbUpdateException)
        {
            // await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);

            return new()
            {
                StatusCode = CustomCodes.BranchCreationFailed,
                IsSuccess = false
            };
        }
        catch (Exception)
        {
            // await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);

            return new()
            {
                StatusCode = CustomCodes.BranchCreationFailed,
                IsSuccess = false
            };
            throw;
        }
    }

    public async Task<ServiceResponse<object>> UpdateBranch(BranchDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);

        using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var branch = await branchRepository.GetByIdAsync(dto.Id, cancellationToken).ConfigureAwait(false);

            if (branch == null)
            {
                return new()
                {
                    StatusCode = CustomCodes.BranchNotFound,
                    IsSuccess = false
                };
            }

            branch.Name = dto.Name ?? branch.Name;
            branch.Location = dto.Location ?? branch.Location;

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return new()
            {
                StatusCode = CustomCodes.BranchUpdatedSuccessfully,
                IsSuccess = true
            };
        }
        catch (OperationCanceledException)
        {
            await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);

            return new()
            {
                StatusCode = CustomCodes.OperationCancelled,
                IsSuccess = false
            };
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);

            return new()
            {
                StatusCode = CustomCodes.BranchUpdateFailed,
                IsSuccess = false
            };
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);

            return new()
            {
                StatusCode = CustomCodes.BranchUpdateFailed,
                IsSuccess = false
            };
            throw;
        }
    }

    public async Task<ServiceResponse<IReadOnlyCollection<BranchResponseDto>>> GetAllBranches(CancellationToken cancellationToken)
    {
        try
        {
            var branches = await branchRepository.GetAllBranchesAsync(cancellationToken).ConfigureAwait(false);

            return new()
            {
                StatusCode = CustomCodes.DataRetrieved,
                IsSuccess = true,
                Data = branches
            };
        }
        catch (Exception)
        {
            return new()
            {
                StatusCode = CustomCodes.InternalServerError,
                IsSuccess = false
            };
            throw;
        }
    }

    public async Task<ServiceResponse<BranchResponseDto?>> GetBranchById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var branch = await branchRepository.GetBranchByIdAsync(id, cancellationToken).ConfigureAwait(false);

            if (branch == null)
            {
                return new()
                {
                    StatusCode = CustomCodes.BranchNotFound,
                    IsSuccess = false
                };
            }

            return new()
            {
                StatusCode = CustomCodes.DataRetrieved,
                IsSuccess = true,
                Data = branch
            };
        }
        catch (Exception)
        {
            return new()
            {
                StatusCode = CustomCodes.InternalServerError,
                IsSuccess = false
            };
            throw;
        }
    }

    public async Task<ServiceResponse<IReadOnlyCollection<BranchUserResponseDto>>> GetBranchUsers(Guid branchId, CancellationToken cancellationToken)
    {
        try
        {
            var users = await branchRepository.GetBranchUsersAsync(branchId, cancellationToken).ConfigureAwait(false);

            if (users.Count == 0)
            {
                return new()
                {
                    StatusCode = CustomCodes.BranchNotFound,
                    IsSuccess = false
                };
            }

            return new()
            {
                StatusCode = CustomCodes.DataRetrieved,
                IsSuccess = true,
                Data = users
            };
        }
        catch (Exception)
        {
            return new()
            {
                StatusCode = CustomCodes.InternalServerError,
                IsSuccess = false
            };
            throw;
        }
    }
}
