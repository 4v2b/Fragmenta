namespace Fragmenta.Api.Contracts
{
    public interface IResetTokenService
    {
        bool VerifyAndDestroyToken(string token, long userId);

        string GenerateToken(long userId);
    }
}
