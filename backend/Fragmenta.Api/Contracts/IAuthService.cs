using Fragmenta.Api.Dtos;

namespace Fragmenta.Api.Contracts;

public interface IAuthService
{
    AuthResult Register(RegisterRequest model);
    
    AuthResult Authorize(LoginRequest model);
}