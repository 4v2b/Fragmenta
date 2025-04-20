using Fragmenta.Api.Dtos;

namespace Fragmenta.Api.Contracts;

public interface IUserLookupService
{
    long? FindSingleByEmail(string email);
    
    UserFullDto? GetUserInfo(long userId);

    List<UserDto> FindManyByEmails(string[] emails);

    List<UserDto> FindByEmail(string email);
}