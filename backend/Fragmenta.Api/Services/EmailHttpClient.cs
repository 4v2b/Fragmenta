using System.Text;
using System.Text.Json;
using Fragmenta.Api.Configuration;
using Fragmenta.Api.Contracts;
using Microsoft.Extensions.Options;

namespace Fragmenta.Api.Services;

public class EmailHttpClient : IEmailHttpClient
{
    private readonly SmtpOptions _options;
    private readonly HttpClient _client;

    public EmailHttpClient(IOptions<SmtpOptions> options, HttpClient client)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _client = client;
    }

    public async Task<HttpResponseMessage> SendEmailAsync(string toEmail, string subject, string content, bool isPlainText = true)
    {
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiKey}");

        var payload = new
        {
            from = new { email = _options.FromEmail, name = _options.FromName},
            to = new[] { new { email = toEmail } },
            subject,
            text = content,
            category = "User triggered request"
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        return await _client.PostAsync(_options.RequestUrl, jsonContent);
    }
}