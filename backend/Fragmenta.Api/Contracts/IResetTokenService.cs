namespace Fragmenta.Api.Contracts
{
    public interface IResetTokenService
    {
        Task<bool> VerifyAndDestroyTokenAsync(string token, long userId);

        Task<string> GenerateTokenAsync(long userId);
    }
}
