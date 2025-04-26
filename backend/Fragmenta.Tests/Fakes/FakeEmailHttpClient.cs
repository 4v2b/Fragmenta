using System.Net;
using Fragmenta.Api.Contracts;

namespace Fragmenta.Tests.Fakes;

public class FakeEmailHttpClient : IEmailHttpClient
{
    public Task<HttpResponseMessage> SendEmailAsync(string email, string subject, string body, bool isPlainText = true) 
        => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
}