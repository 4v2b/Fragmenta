using Fragmenta.Api.Dtos;

namespace Fragmenta.Api.Contracts
{
    public interface IMailingService
    {
        Task<EmailSendResult> SendResetTokenAsync(string receiver, string token, long userId);
    }
}
