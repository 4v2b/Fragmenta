using Fragmenta.Api.Dtos;

namespace Fragmenta.Api.Contracts
{
    public interface IUserService
    {
        UserDto? Register(RegisterRequest model);

        bool ChangePassword(string newPassword, string oldPassword, long userId);

        bool VerifyPassword(string password, long userId);

        bool Delete(string password, long userId);

        UserDto? Authorize(LoginRequest model);
    }
}
