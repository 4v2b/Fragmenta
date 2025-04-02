using Fragmenta.Api.Dtos;

namespace Fragmenta.Api.Contracts
{
    public interface IMailingService
    {
        Task<EmailSendResult> SendEmailAsync(string receiver, string content);
    }
}
