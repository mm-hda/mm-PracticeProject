using backend.Dto;

namespace backend.IService
{
    public interface IAuthService
    {
        Task<Tuple<int, TokenDto>> LoginUser(LoginDto dto);

        Task<Tuple<int, string>> RegisterUser(RegisterUserDto dto);
    }
}