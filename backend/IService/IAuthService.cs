using backend.Dto;
using backend.GenericResponse;

namespace backend.IService;

public interface IAuthService
{
    Task<ServiceResponse<TokenDto>> LoginUser(LoginDto dto, CancellationToken cancellationToken);
    Task<ServiceResponse<object>> RegisterUser(RegisterUserDto dto, CancellationToken cancellationToken);
    Task<ServiceResponse<TokenDto>> RefreshTokenAsync(CancellationToken cancellationToken);
    Task<ServiceResponse<object>> LogoutAsync(CancellationToken cancellationToken);

}
