using Fragmenta.Api.Dtos;

namespace Fragmenta.Api.Contracts;

public interface IRefreshTokenLookupService
{
    UserDto? GetUserByToken(string token);

    bool HasValidToken(long userId);
}