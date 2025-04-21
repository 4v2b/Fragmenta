using Fragmenta.Api.Dtos;
using Fragmenta.Api.Enums;
using Fragmenta.Dal.Models;
using Task = System.Threading.Tasks.Task;

namespace Fragmenta.Api.Contracts
{
    public interface IRefreshTokenService
    {
        Task<string?> GenerateTokenAsync(long userId);

        Task RevokeTokensAsync(long userId);

        Task<RefreshTokenStatus> VerifyTokenAsync(string refreshToken, long userId);

        Task<string?> RefreshTokenAsync(string refreshToken, long userId);
    }
}
