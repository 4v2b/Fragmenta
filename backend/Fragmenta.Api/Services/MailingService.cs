using Fragmenta.Api.Contracts;
using System.Text.Json;
using System.Text;
using Fragmenta.Api.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using Fragmenta.Api.Dtos;
using Fragmenta.Api.Enums;

namespace Fragmenta.Api.Services
{
    public class MailingService : IMailingService
    {
        private readonly SmtpOptions _options;
        private readonly ILogger<MailingService> _logger;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan lockoutTime = TimeSpan.FromMinutes(10);
        private const int MaxAttempts = 3;

        public MailingService(ILogger<MailingService> logger, IOptions<SmtpOptions> options, IMemoryCache cache)
        {
            _logger = logger;
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _cache = cache;
        }

        // TODO Include domain name in header prop in request body

        public async Task<EmailSendResult> SendEmailAsync(string receiver, string content)
        {
            var key = $"email_attempts_{receiver}";

            // Check if the user is locked out
            if (_cache.TryGetValue<(int Attempts, DateTime? LockedUntil)>(key, out var entry))
            {
                if (entry.LockedUntil.HasValue && entry.LockedUntil > DateTime.UtcNow)
                {
                    return EmailSendResult.RateLimited(entry.LockedUntil.Value);
                }
            }
            else
            {
                entry = (0, null);
            }

            using HttpClient client = new();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiKey}");

            using StringContent jsonContent = new(
                JsonSerializer.Serialize(new
                {
                    from = new { email = _options.FromEmail, name = _options.FromName },
                    to = new[] { new { email = receiver } },
                    subject = "Password Recovery",
                    text = content,
                    category = "Testing"
                }),
                Encoding.UTF8,
                "application/json"
            );

            _logger.LogInformation("Sending password reset email to {Email}", receiver);

            var response = await client.PostAsync("https://send.api.mailtrap.io/api/send", jsonContent);

            if (response.IsSuccessStatusCode)
            {

                entry = (entry.Attempts + 1, entry.Attempts + 1 >= MaxAttempts ? DateTime.UtcNow.Add(lockoutTime) : null);
                _cache.Set(key, entry, new MemoryCacheEntryOptions
                {
                    //TODO remove reset lockout time literals 
                    AbsoluteExpirationRelativeToNow = entry.LockedUntil.HasValue ? lockoutTime : TimeSpan.FromMinutes(15)
                });

                return EmailSendResult.SuccessResult();
            }

            // Reset attempts on failure
            _cache.Remove(key);
            _logger.LogError("Failed to send email to {Email}. StatusCode: {StatusCode}", receiver, response.StatusCode);
            return EmailSendResult.Failure(EmailSendErrorType.SendingError);  
        }
    }
}
