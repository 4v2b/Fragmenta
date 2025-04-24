namespace Fragmenta.Api.Contracts;

public interface IEmailHttpClient
{
    Task<HttpResponseMessage> SendEmailAsync(string toEmail, string subject, string content, bool isPlainText = true);
}