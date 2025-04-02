using Fragmenta.Api.Dtos;

namespace Fragmenta.Api.Contracts
{
    public interface IUserService
    {
        AuthResult Register(RegisterRequest model);

        bool ChangePassword(string newPassword, string oldPassword, long userId);

        bool ResetPassword(string newPassword, long userId);

        bool VerifyPassword(string password, long userId);

        bool Delete(string password, long userId);

        AuthResult Authorize(LoginRequest model);

        UserFullDto? GetUserInfo(long userId);

        List<UserDto> FindManyByEmails(string[] emails);

        List<UserDto> FindByEmail(string email);

        long? FindSingleByEmail(string email);
    }
}
