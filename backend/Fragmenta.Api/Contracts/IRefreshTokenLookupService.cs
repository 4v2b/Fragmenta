using Fragmenta.Api.Dtos;

namespace Fragmenta.Api.Contracts;

public interface IRefreshTokenLookupService
{
    Task<UserDto?> GetUserByTokenAsync(string token);

    Task<bool> HasValidTokenAsync(long userId);
}