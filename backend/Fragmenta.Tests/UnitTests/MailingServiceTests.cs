using System.Net;
using Fragmenta.Api.Contracts;
using Fragmenta.Api.Enums;
using Fragmenta.Api.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Fragmenta.Tests.UnitTests;

public class MailingServiceTests
{
    private readonly IMemoryCache _memoryCache;
    private readonly Mock<IEmailHttpClient> _httpClientMock = new();

    public MailingServiceTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
    }

    [Fact]
    public async Task SendResetTokenAsync_FirstAttempt_Success()
    {
        // Arrange
        var receiver = "test@example.com";
        var token = "token";
        var userId = 1L;

        _httpClientMock.Setup(c => c.SendEmailAsync(receiver, It.IsAny<string>(), It.IsAny<string>(), true))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var service = new MailingService(new NullLogger<MailingService>(), _memoryCache, _httpClientMock.Object);

        // Act
        var result = await service.SendResetTokenAsync(receiver, token, userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorType);
    }

    [Fact]
    public async Task SendResetTokenAsync_TooManyAttempts_ReturnsRateLimited()
    {
        // Arrange
        var receiver = "test@example.com";
        var key = $"email_attempts_{receiver}";
        var token = "token";
        var userId = 1L;

        var value = (Attempts: 3, LockedUntil: (DateTime?)DateTime.UtcNow.AddMinutes(5));
        _memoryCache.Set(key, value);
        
        var service = new MailingService(new NullLogger<MailingService>(), _memoryCache, _httpClientMock.Object);

        // Act
        var result = await service.SendResetTokenAsync(receiver, token, userId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(EmailSendErrorType.RateLimited, result.ErrorType);
    }

    [Fact]
    public async Task SendResetTokenAsync_HttpError_ReturnsFailure()
    {
        // Arrange
        var receiver = "fail@example.com";
        var token = "token";
        var userId = 1L;

        _httpClientMock.Setup(c => c.SendEmailAsync(receiver, It.IsAny<string>(), It.IsAny<string>(), true))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var service = new MailingService(new NullLogger<MailingService>(), _memoryCache, _httpClientMock.Object);

        // Act
        var result = await service.SendResetTokenAsync(receiver, token, userId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(EmailSendErrorType.SendingError, result.ErrorType);
    }
}