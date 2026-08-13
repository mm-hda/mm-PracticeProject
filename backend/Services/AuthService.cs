using backend.Dto;
using backend.Dto.RefreshTokenDtos;
using backend.Entities;
using backend.GenericResponse;
using backend.IRepository;
using backend.IService;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

internal sealed class AuthService(IAuthRepository authRepository, IRefreshTokenRepository refreshTokenRepository, IUnitOfWork unitOfWork, ITokenService tokenService, ICookieService cookieService) : IAuthService
{
    public async Task<ServiceResponse<TokenDto>> LoginUser(LoginDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        TokenDto tokenDto = new();

        try
        {
            var existingUser = await authRepository.GetUserByEmailWithDetailsAsync(dto.Email, cancellationToken).ConfigureAwait(false);

            if (existingUser is null)
            {
                return new ServiceResponse<TokenDto>
                {
                    StatusCode = CustomCodes.UserNotFound,
                    IsSuccess = false,
                    Data = tokenDto
                };
            }

            var passwordHasher = new PasswordHasher<string>();
            var verificationResult = passwordHasher.VerifyHashedPassword(
                dto.Email ?? string.Empty,
                existingUser.Password ?? string.Empty,
                dto.Password ?? string.Empty);

            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return new ServiceResponse<TokenDto>
                {
                    StatusCode = CustomCodes.InvalidCredentials,
                    IsSuccess = false,
                    Data = tokenDto
                };
            }

            if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
            {
                existingUser.Password = PasswordHashing(existingUser.Email ?? string.Empty, dto.Password ?? string.Empty);
            }

            var accessToken = tokenService.GenerateAccessToken(existingUser);
            var refreshToken = tokenService.GenerateRefreshToken();
            var refreshTokenHash = tokenService.HashRefreshToken(refreshToken);
            var refreshTokenExpiresAtUtc = DateTime.UtcNow.AddDays(tokenService.GetRefreshTokenExpiryDays());

            await refreshTokenRepository.AddRefreshTokenAsync(new CreateRefreshTokenDto
            {
                UserId = existingUser.Id,
                TokenHash = refreshTokenHash,
                ExpiresAtUtc = refreshTokenExpiresAtUtc,
                CreatedAtUtc = DateTime.UtcNow,
                LastUsedAtUtc = DateTime.UtcNow
            }, cancellationToken).ConfigureAwait(false);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            cookieService.AppendAccessTokenCookie(accessToken);
            cookieService.AppendRefreshTokenCookie(refreshToken, refreshTokenExpiresAtUtc);

            tokenDto.UserId = existingUser.Id;
            tokenDto.Name = existingUser.Name ?? string.Empty;
            tokenDto.Email = existingUser.Email ?? string.Empty;
            tokenDto.Role = existingUser.Role?.Name ?? string.Empty;
            tokenDto.Branch = existingUser.Branch?.Name ?? string.Empty;

            return new ServiceResponse<TokenDto>
            {
                StatusCode = CustomCodes.LoginSuccessfully,
                IsSuccess = true,
                Data = tokenDto
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (DbUpdateException)
        {
            return new ServiceResponse<TokenDto>
            {
                StatusCode = CustomCodes.InternalServerError,
                IsSuccess = false,
                Data = tokenDto
            };
        }
    }

    public async Task<ServiceResponse<TokenDto>> RefreshTokenAsync(CancellationToken cancellationToken)
    {
        TokenDto tokenDto = new();

        var refreshToken = cookieService.GetRefreshTokenCookie();

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            cookieService.ClearAuthCookies();

            return new ServiceResponse<TokenDto>
            {
                StatusCode = CustomCodes.InvalidCredentials,
                IsSuccess = false,
                Data = tokenDto
            };
        }

        var refreshTokenHash = tokenService.HashRefreshToken(refreshToken);

        var storedToken = await refreshTokenRepository
            .GetByTokenHashAsync(refreshTokenHash, cancellationToken)
            .ConfigureAwait(false);

        if (storedToken is null || !storedToken.IsActive || storedToken.ExpiresAtUtc <= DateTime.UtcNow)
        {
            cookieService.ClearAuthCookies();

            return new ServiceResponse<TokenDto>
            {
                StatusCode = CustomCodes.InvalidCredentials,
                IsSuccess = false,
                Data = tokenDto
            };
        }

        var user = await authRepository.GetUserByEmailWithDetailsAsync(storedToken.UserEmail, cancellationToken).ConfigureAwait(false);

        if (user is null)
        {
            cookieService.ClearAuthCookies();

            return new ServiceResponse<TokenDto>
            {
                StatusCode = CustomCodes.UserNotFound,
                IsSuccess = false,
                Data = tokenDto
            };
        }

        await refreshTokenRepository.UpdateLastUsedAtUtcAsync(storedToken.Id, cancellationToken).ConfigureAwait(false);

        var newAccessToken = tokenService.GenerateAccessToken(user);

        var remainingRefreshTokenTime = storedToken.ExpiresAtUtc - DateTime.UtcNow;

        if (remainingRefreshTokenTime <= TimeSpan.FromDays(3))
        {
            await refreshTokenRepository
                .DeactivateRefreshTokenAsync(storedToken.Id, cancellationToken)
                .ConfigureAwait(false);

            var newRefreshToken = tokenService.GenerateRefreshToken();
            var newRefreshTokenHash = tokenService.HashRefreshToken(newRefreshToken);
            var newRefreshTokenExpiresAtUtc = DateTime.UtcNow.AddDays(tokenService.GetRefreshTokenExpiryDays());

            await refreshTokenRepository.AddRefreshTokenAsync(new CreateRefreshTokenDto
            {
                UserId = user.Id,
                TokenHash = newRefreshTokenHash,
                ExpiresAtUtc = newRefreshTokenExpiresAtUtc,
                CreatedAtUtc = DateTime.UtcNow,
                LastUsedAtUtc = DateTime.UtcNow
            }, cancellationToken).ConfigureAwait(false);

            cookieService.AppendRefreshTokenCookie(newRefreshToken, newRefreshTokenExpiresAtUtc);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        cookieService.AppendAccessTokenCookie(newAccessToken);

        tokenDto.UserId = user.Id;
        tokenDto.Name = user.Name ?? string.Empty;
        tokenDto.Email = user.Email ?? string.Empty;
        tokenDto.Role = user.Role?.Name ?? string.Empty;
        tokenDto.Branch = user.Branch?.Name ?? string.Empty;

        return new ServiceResponse<TokenDto>
        {
            StatusCode = CustomCodes.LoginSuccessfully,
            IsSuccess = true,
            Data = tokenDto
        };
    }

    public async Task<ServiceResponse<object>> LogoutAsync(CancellationToken cancellationToken)
    {
        var refreshToken = cookieService.GetRefreshTokenCookie();

        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            var refreshTokenHash = tokenService.HashRefreshToken(refreshToken);

            var storedToken = await refreshTokenRepository
                .GetByTokenHashAsync(refreshTokenHash, cancellationToken)
                .ConfigureAwait(false);

            if (storedToken is not null && storedToken.IsActive)
            {
                await refreshTokenRepository
                    .DeactivateRefreshTokenAsync(storedToken.Id, cancellationToken)
                    .ConfigureAwait(false);

                await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        cookieService.ClearAuthCookies();

        return new ServiceResponse<object>
        {
            StatusCode = CustomCodes.LoginSuccessfully,
            IsSuccess = true
        };
    }

    public async Task<ServiceResponse<object>> RegisterUser(RegisterUserDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);

        try
        {
            var emailExists = await authRepository.EmailExistsAsync(dto.Email, cancellationToken).ConfigureAwait(false);

            if (emailExists)
            {
                return new ServiceResponse<object>
                {
                    StatusCode = CustomCodes.UserAlreadyExists,
                    IsSuccess = false
                };
            }

            var branchExists = await authRepository.BranchExistsAsync(dto.BranchId, cancellationToken).ConfigureAwait(false);

            if (!branchExists)
            {
                return new ServiceResponse<object>
                {
                    StatusCode = CustomCodes.BranchNotFound,
                    IsSuccess = false
                };
            }

            var departmentExists = await authRepository.DepartmentExistsAsync(dto.DepartmentId, cancellationToken).ConfigureAwait(false);

            if (!departmentExists)
            {
                return new ServiceResponse<object>
                {
                    StatusCode = CustomCodes.DepartmentNotFound,
                    IsSuccess = false
                };
            }

            var positionExists = await authRepository.PositionExistsAsync(dto.PositionId, dto.DepartmentId, cancellationToken).ConfigureAwait(false);

            if (!positionExists)
            {
                return new ServiceResponse<object>
                {
                    StatusCode = CustomCodes.PositionNotFound,
                    IsSuccess = false
                };
            }

            var roleExists = await authRepository.RoleExistsAsync(dto.RoleId, cancellationToken).ConfigureAwait(false);

            if (!roleExists)
            {
                return new ServiceResponse<object>
                {
                    StatusCode = CustomCodes.RoleNotFound,
                    IsSuccess = false
                };
            }

            cancellationToken.ThrowIfCancellationRequested();

            User newUser = new()
            {
                Id = Guid.NewGuid(),
                Name = dto.Name ?? string.Empty,
                Email = dto.Email ?? string.Empty,
                Password = PasswordHashing(dto.Email ?? string.Empty, dto.Password ?? string.Empty),
                DOB = dto.DOB,
                BranchId = dto.BranchId,
                DepartmentId = dto.DepartmentId,
                PositionId = dto.PositionId,
                RoleId = dto.RoleId
            };

            await authRepository.AddUserAsync(newUser, cancellationToken).ConfigureAwait(false);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new ServiceResponse<object>
            {
                StatusCode = CustomCodes.UserCreatedSuccessfully,
                IsSuccess = true
            };
        }
        catch (OperationCanceledException)
        {
            return new ServiceResponse<object>
            {
                StatusCode = CustomCodes.OperationCancelled,
                IsSuccess = false
            };
        }
        catch (DbUpdateException)
        {
            return new ServiceResponse<object>
            {
                StatusCode = CustomCodes.UserCreationFailed,
                IsSuccess = false
            };
        }
    }

    private static string PasswordHashing(string email, string password)
    {
        var passwordHasher = new PasswordHasher<string>();
        return passwordHasher.HashPassword(email, password);
    }
}
