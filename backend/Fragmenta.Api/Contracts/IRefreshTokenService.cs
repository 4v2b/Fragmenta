using Fragmenta.Api.Dtos;
using Fragmenta.Api.Enums;
using Fragmenta.Dal.Models;

namespace Fragmenta.Api.Contracts
{
    public interface IRefreshTokenService
    {
        string? GenerateToken(long userId);

        void RevokeTokens(long userId);

        RefreshTokenStatus VerifyToken(string refreshToken, long userId);

        string? RefreshToken(string refreshToken, long userId);
    }
}
