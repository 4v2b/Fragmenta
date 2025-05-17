using Fragmenta.Api.Contracts;
using System.Text.Json;
using System.Text;
using Fragmenta.Api.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using Fragmenta.Api.Dtos;
using Fragmenta.Api.Enums;
using Fragmenta.Api.Utils;

namespace Fragmenta.Api.Services
{
    public class MailingService : IMailingService
    {
        private readonly ILogger<MailingService> _logger;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan lockoutTime = TimeSpan.FromMinutes(10);
        private const int MaxAttempts = 3;
        private readonly IEmailHttpClient _emailHttpClient;
        private readonly FrontendOptions _frontendOptions;

        public MailingService(ILogger<MailingService> logger,  IMemoryCache cache,  IEmailHttpClient emailHttpClient, IOptions<FrontendOptions> frontendOptions)
        {
            _logger = logger;
            _emailHttpClient = emailHttpClient;
            _cache = cache;
            _frontendOptions = frontendOptions.Value ?? throw new ArgumentNullException(nameof(frontendOptions));
        }

        public async Task<EmailSendResult> SendResetTokenAsync(string receiver, string token, long userId, string culture = "en-US")
        {
            var key = $"email_attempts_{receiver}";
            
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
            
            var content = MailBodyFormer.CreateResetPasswordTextBody(_frontendOptions.BaseUrl, token, userId, culture);

            var response = await _emailHttpClient.SendEmailAsync(receiver, "Password Reset", content);

            _logger.LogInformation("Sending password reset email to {Email}", receiver);

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
            _cache.Remove(key);
            _logger.LogError("Failed to send email to {Email}. StatusCode: {StatusCode}", receiver, response.StatusCode);
            return EmailSendResult.Failure(EmailSendErrorType.SendingError);  
        }
    }
}
