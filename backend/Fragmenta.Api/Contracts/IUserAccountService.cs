using Fragmenta.Api.Dtos;

namespace Fragmenta.Api.Contracts
{
    public interface IUserAccountService
    {
        Task<bool> ChangePasswordAsync(string newPassword, string oldPassword, long userId);
        
        Task<bool> ChangeNameAsync(string newUsername, long userId);

        Task<bool> ResetPasswordAsync(string newPassword, long userId);

        Task<bool> VerifyPasswordAsync(string password, long userId);

        Task<bool> DeleteAsync(string password, long userId);
    }
}
