using System.Text;
using System.Text.Json;
using Fragmenta.Api.Configuration;
using Fragmenta.Api.Contracts;
using Microsoft.Extensions.Options;

namespace Fragmenta.Api.Services;

public class EmailHttpClient : IEmailHttpClient
{
    private readonly SmtpOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;

    public EmailHttpClient(IOptions<SmtpOptions> options, IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<HttpResponseMessage> SendEmailAsync(string toEmail, string subject, string content, bool isPlainText = true)
    {
        var client = _httpClientFactory.CreateClient();
        
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiKey}");

        var payload = new
        {
            from = new { email = _options.FromEmail, name = _options.FromName},
            to = new[] { new { email = toEmail } },
            subject,
            text = content,
            category = "User triggered request"
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        return await client.PostAsync(_options.RequestUrl, jsonContent);
    }
}