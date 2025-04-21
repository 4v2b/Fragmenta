using Fragmenta.Api.Dtos;

namespace Fragmenta.Api.Contracts;

public interface IUserLookupService
{
    Task<long?> FindSingleByEmailAsync(string email);
    
    Task<UserFullDto?> GetUserInfoAsync(long userId);

    Task<List<UserDto>> FindManyByEmailsAsync(string[] emails);

    Task<List<UserDto>> FindByEmailAsync(string email);
}