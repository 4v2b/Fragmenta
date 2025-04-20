using Fragmenta.Api.Dtos;

namespace Fragmenta.Api.Contracts
{
    public interface IUserAccountService
    {
        bool ChangePassword(string newPassword, string oldPassword, long userId);
        
        Task<bool> ChangeNameAsync(string newUsername, long userId);

        bool ResetPassword(string newPassword, long userId);

        bool VerifyPassword(string password, long userId);

        bool Delete(string password, long userId);
    }
}
