using Fragmenta.Api.Dtos;

namespace Fragmenta.Api.Contracts;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequest model);
    
    Task<AuthResult> AuthorizeAsync(LoginRequest model);
}