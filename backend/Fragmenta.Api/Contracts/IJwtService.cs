using Fragmenta.Api.Dtos;
using Fragmenta.Dal.Models;

namespace Fragmenta.Api.Contracts
{
    public interface IJwtService
    {
        string GenerateToken(UserDto user);
    }
}
