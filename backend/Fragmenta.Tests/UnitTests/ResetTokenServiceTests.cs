using Fragmenta.Api.Contracts;
using Fragmenta.Api.Services;
using Fragmenta.Dal.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Fragmenta.Tests.UnitTests;

public class ResetTokenServiceTests : UnitTestsBase
{
    private readonly ILogger<ResetTokenService> _logger;
    private readonly Mock<IHashingService> _hasherMock;

    public ResetTokenServiceTests()
    {
        _logger = new NullLogger<ResetTokenService>();
        _hasherMock = new Mock<IHashingService>();
    }

    [Fact]
    public async Task GenerateTokenAsync_UserExists_CreatesToken()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userId = 1L;
        var user = new User { Id = userId, Email = "test@example.com", Name = "Test User", PasswordHash = [], PasswordSalt = [] };
        
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        
        _hasherMock.Setup(h => h.Hash(It.IsAny<string>(), null)).Returns(new byte[] { 1, 2, 3, 4 });
        
        var service = new ResetTokenService(_logger, context, _hasherMock.Object);

        // Act
        var token = await service.GenerateTokenAsync(userId);

        // Assert
        Assert.NotNull(token);
        Assert.Single(await context.ResetTokens.ToListAsync());
        var savedToken = await context.ResetTokens.FirstAsync();
        Assert.Equal(userId, savedToken.UserId);
        Assert.True(savedToken.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task GenerateTokenAsync_ExistingToken_ReplacesToken()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userId = 1L;
        var user = new User { Id = userId, Email = "test@example.com", Name = "Test User", PasswordHash = [], PasswordSalt = [] };
        var existingToken = new ResetToken
        {
            UserId = userId,
            TokenHash = new byte[] { 1, 2, 3, 4 },
            ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            CreatedAt = DateTime.UtcNow
        };
        
        await context.Users.AddAsync(user);
        await context.ResetTokens.AddAsync(existingToken);
        await context.SaveChangesAsync();
        
        _hasherMock.Setup(h => h.Hash(It.IsAny<string>(), null)).Returns(new byte[] { 5, 6, 7, 8 });
        
        var service = new ResetTokenService(_logger, context, _hasherMock.Object);

        // Act
        var token = await service.GenerateTokenAsync(userId);

        // Assert
        Assert.NotNull(token);
        Assert.Single(await context.ResetTokens.ToListAsync());
        var savedToken = await context.ResetTokens.FirstAsync();
        Assert.Equal(userId, savedToken.UserId);
        Assert.True(savedToken.ExpiresAt > DateTime.UtcNow);
        Assert.Equal(new byte[] { 5, 6, 7, 8 }, savedToken.TokenHash);
    }

    [Fact]
    public async Task GenerateTokenAsync_UserNotFound_ThrowsArgumentException()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new ResetTokenService(_logger, context, _hasherMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.GenerateTokenAsync(999));
    }

    [Fact]
    public async Task VerifyAndDestroyTokenAsync_ValidToken_ReturnsTrue()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userId = 1L;
        var resetToken = "valid-token";
        var hashedToken = new byte[] { 1, 2, 3, 4 };
        
        var token = new ResetToken
        {
            UserId = userId,
            TokenHash = hashedToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            CreatedAt = DateTime.UtcNow
        };
        
        _hasherMock.Setup(h => h.Hash(resetToken, null)).Returns(hashedToken);
        
        await context.ResetTokens.AddAsync(token);
        await context.SaveChangesAsync();
        
        var service = new ResetTokenService(_logger, context, _hasherMock.Object);

        // Act
        var result = await service.VerifyAndDestroyTokenAsync(resetToken, userId);

        // Assert
        Assert.True(result);
        Assert.Empty(await context.ResetTokens.ToListAsync());
    }

    [Fact]
    public async Task VerifyAndDestroyTokenAsync_ExpiredToken_ReturnsFalse()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userId = 1L;
        var resetToken = "expired-token";
        var hashedToken = new byte[] { 1, 2, 3, 4 };
        
        var token = new ResetToken
        {
            UserId = userId,
            TokenHash = hashedToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(-5), // Expired
            CreatedAt = DateTime.UtcNow.AddMinutes(-35)
        };
        
        _hasherMock.Setup(h => h.Hash(resetToken, null)).Returns(hashedToken);
        
        await context.ResetTokens.AddAsync(token);
        await context.SaveChangesAsync();
        
        var service = new ResetTokenService(_logger, context, _hasherMock.Object);

        // Act
        var result = await service.VerifyAndDestroyTokenAsync(resetToken, userId);

        // Assert
        Assert.False(result);
        Assert.Empty(await context.ResetTokens.ToListAsync());
    }

    [Fact]
    public async Task VerifyAndDestroyTokenAsync_InvalidToken_ReturnsFalse()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userId = 1L;
        var resetToken = "invalid-token";
        var storedToken = new byte[] { 1, 2, 3, 4 };
        var userToken = new byte[] { 5, 6, 7, 8 }; // Different hash
        
        var token = new ResetToken
        {
            UserId = userId,
            TokenHash = storedToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            CreatedAt = DateTime.UtcNow
        };
        
        _hasherMock.Setup(h => h.Hash(resetToken, null)).Returns(userToken);
        
        await context.ResetTokens.AddAsync(token);
        await context.SaveChangesAsync();
        
        var service = new ResetTokenService(_logger, context, _hasherMock.Object);

        // Act
        var result = await service.VerifyAndDestroyTokenAsync(resetToken, userId);

        // Assert
        Assert.False(result);
        Assert.Single(await context.ResetTokens.ToListAsync());
    }

    [Fact]
    public async Task VerifyAndDestroyTokenAsync_NoToken_ReturnsFalse()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new ResetTokenService(_logger, context, _hasherMock.Object);

        // Act
        var result = await service.VerifyAndDestroyTokenAsync("some-token", 999);

        // Assert
        Assert.False(result);
    }
}