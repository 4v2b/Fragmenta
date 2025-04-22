using Fragmenta.Api.Contracts;
using Fragmenta.Api.Enums;
using Fragmenta.Api.Services;
using Fragmenta.Dal.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Fragmenta.Tests.UnitTests;

public class RefreshTokenServiceTests : UnitTestsBase
{
    private readonly ILogger<RefreshTokenService> _logger;
    private readonly Mock<IHashingService> _hasherMock;

    public RefreshTokenServiceTests()
    {
        _logger = new NullLogger<RefreshTokenService>();
        _hasherMock = new Mock<IHashingService>();
    }

    [Fact]
    public async Task GenerateTokenAsync_NoExistingToken_CreatesNewToken()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userId = 1L;
        var user = new User { Id = userId, Email = "test@example.com", Name = "Test User", PasswordHash = [], PasswordSalt = [] };
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        
        _hasherMock.Setup(h => h.Hash(It.IsAny<string>(), null)).Returns(new byte[] { 1, 2, 3, 4 });
        
        var service = new RefreshTokenService(_logger, _hasherMock.Object, context);

        // Act
        var token = await service.GenerateTokenAsync(userId);

        // Assert
        Assert.NotNull(token);
        Assert.Single(await context.RefreshTokens.ToListAsync());
        var savedToken = await context.RefreshTokens.FirstAsync();
        Assert.Equal(userId, savedToken.UserId);
        Assert.Null(savedToken.RevokedAt);
        Assert.True(savedToken.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task GenerateTokenAsync_ExistingValidToken_ReturnsNull()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userId = 1L;
        var user = new User { Id = userId, Email = "test@example.com", Name = "Test User", PasswordHash = [], PasswordSalt = []};
        var existingToken = new RefreshToken
        {
            UserId = userId,
            TokenHash = new byte[] { 1, 2, 3, 4 },
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow,
            RevokedAt = null
        };
        
        await context.Users.AddAsync(user);
        await context.RefreshTokens.AddAsync(existingToken);
        await context.SaveChangesAsync();
        
        var service = new RefreshTokenService(_logger, _hasherMock.Object, context);

        // Act
        var token = await service.GenerateTokenAsync(userId);

        // Assert
        Assert.Null(token);
        Assert.Single(await context.RefreshTokens.ToListAsync());
    }

    [Fact]
    public async Task GenerateTokenAsync_UserNotFound_ThrowsArgumentException()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new RefreshTokenService(_logger, _hasherMock.Object, context);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.GenerateTokenAsync(999));
    }

    [Fact]
    public async Task RefreshTokenAsync_ValidToken_GeneratesNewToken()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userId = 1L;
        var refreshToken = "valid-token";
        var hashedToken = new byte[] { 1, 2, 3, 4 };
        
        var user = new User { Id = userId, Email = "test@example.com", Name = "Test User", PasswordHash = [], PasswordSalt = [] };
        var existingToken = new RefreshToken
        {
            UserId = userId,
            TokenHash = hashedToken,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow,
            RevokedAt = null
        };
        
        _hasherMock.Setup(h => h.Hash(refreshToken, null)).Returns(hashedToken);
        
        await context.Users.AddAsync(user);
        await context.RefreshTokens.AddAsync(existingToken);
        await context.SaveChangesAsync();
        
        var service = new RefreshTokenService(_logger, _hasherMock.Object, context);

        // Act
        var newToken = await service.RefreshTokenAsync(refreshToken, userId);

        // Assert
        Assert.NotNull(newToken);
        Assert.Equal(2, await context.RefreshTokens.CountAsync());
        Assert.NotNull(await context.RefreshTokens.FirstAsync(t => t.RevokedAt != null));
    }

    [Fact]
    public async Task RefreshTokenAsync_ExpiredToken_GeneratesNewToken()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userId = 1L;
        var refreshToken = "expired-token";
        var hashedToken = new byte[] { 1, 2, 3, 4 };
        
        var user = new User { Id = userId, Email = "test@example.com", Name = "Test User", PasswordHash = [], PasswordSalt = [] };
        var expiredToken = new RefreshToken
        {
            UserId = userId,
            TokenHash = hashedToken,
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expired
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            RevokedAt = null
        };
        
        _hasherMock.Setup(h => h.Hash(refreshToken, null)).Returns(hashedToken);
        
        await context.Users.AddAsync(user);
        await context.RefreshTokens.AddAsync(expiredToken);
        await context.SaveChangesAsync();
        
        var service = new RefreshTokenService(_logger, _hasherMock.Object, context);

        // Act
        var newToken = await service.RefreshTokenAsync(refreshToken, userId);

        // Assert
        Assert.NotNull(newToken);
        Assert.Equal(2, await context.RefreshTokens.CountAsync());
        Assert.NotNull(await context.RefreshTokens.FirstAsync(t => t.RevokedAt != null));
    }

    [Fact]
    public async Task RefreshTokenAsync_InvalidToken_ReturnsNull()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userId = 1L;
        var refreshToken = "invalid-token";
        var storedToken = new byte[] { 1, 2, 3, 4 };
        var userToken = new byte[] { 5, 6, 7, 8 }; // Different hash
        
        var user = new User { Id = userId, Email = "test@example.com", Name = "Test User", PasswordHash = [], PasswordSalt = [] };
        var existingToken = new RefreshToken
        {
            UserId = userId,
            TokenHash = storedToken,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow,
            RevokedAt = null
        };
        
        _hasherMock.Setup(h => h.Hash(refreshToken, null)).Returns(userToken);
        
        await context.Users.AddAsync(user);
        await context.RefreshTokens.AddAsync(existingToken);
        await context.SaveChangesAsync();
        
        var service = new RefreshTokenService(_logger, _hasherMock.Object, context);

        // Act
        var newToken = await service.RefreshTokenAsync(refreshToken, userId);

        // Assert
        Assert.Null(newToken);
        Assert.Single(await context.RefreshTokens.ToListAsync());
        Assert.Null((await context.RefreshTokens.FirstAsync()).RevokedAt);
    }

    [Fact]
    public async Task RevokeTokensAsync_RevokesAllUserTokens()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userId = 1L;
        
        var tokens = new[]
        {
            new RefreshToken
            {
                UserId = userId,
                TokenHash = new byte[] { 1, 2, 3, 4 },
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                CreatedAt = DateTime.UtcNow,
                RevokedAt = null
            },
            new RefreshToken
            {
                UserId = userId,
                TokenHash = new byte[] { 5, 6, 7, 8 },
                ExpiresAt = DateTime.UtcNow.AddDays(2),
                CreatedAt = DateTime.UtcNow,
                RevokedAt = null
            }
        };
        
        await context.RefreshTokens.AddRangeAsync(tokens);
        await context.SaveChangesAsync();
        
        var service = new RefreshTokenService(_logger, _hasherMock.Object, context);

        // Act
        await service.RevokeTokensAsync(userId);

        // Assert
        Assert.Equal(2, await context.RefreshTokens.CountAsync());
        Assert.Equal(2, await context.RefreshTokens.CountAsync(t => t.RevokedAt != null));
    }

    [Fact]
    public async Task VerifyTokenAsync_ValidToken_ReturnsValid()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userId = 1L;
        var refreshToken = "valid-token";
        var hashedToken = new byte[] { 1, 2, 3, 4 };
        
        var token = new RefreshToken
        {
            UserId = userId,
            TokenHash = hashedToken,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow,
            RevokedAt = null
        };
        
        _hasherMock.Setup(h => h.Hash(refreshToken, null)).Returns(hashedToken);
        
        await context.RefreshTokens.AddAsync(token);
        await context.SaveChangesAsync();
        
        var service = new RefreshTokenService(_logger, _hasherMock.Object, context);

        // Act
        var result = await service.VerifyTokenAsync(refreshToken, userId);

        // Assert
        Assert.Equal(RefreshTokenStatus.Valid, result);
    }

    [Fact]
    public async Task VerifyTokenAsync_ExpiredToken_ReturnsExpired()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userId = 1L;
        var refreshToken = "expired-token";
        var hashedToken = new byte[] { 1, 2, 3, 4 };
        
        var token = new RefreshToken
        {
            UserId = userId,
            TokenHash = hashedToken,
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expired
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            RevokedAt = null
        };
        
        _hasherMock.Setup(h => h.Hash(refreshToken, null)).Returns(hashedToken);
        
        await context.RefreshTokens.AddAsync(token);
        await context.SaveChangesAsync();
        
        var service = new RefreshTokenService(_logger, _hasherMock.Object, context);

        // Act
        var result = await service.VerifyTokenAsync(refreshToken, userId);

        // Assert
        Assert.Equal(RefreshTokenStatus.Expired, result);
    }

    [Fact]
    public async Task VerifyTokenAsync_InvalidToken_ReturnsInvalidOrRevoked()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userId = 1L;
        var refreshToken = "invalid-token";
        var storedToken = new byte[] { 1, 2, 3, 4 };
        var userToken = new byte[] { 5, 6, 7, 8 }; // Different hash
        
        var token = new RefreshToken
        {
            UserId = userId,
            TokenHash = storedToken,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow,
            RevokedAt = null
        };
        
        _hasherMock.Setup(h => h.Hash(refreshToken, null)).Returns(userToken);
        
        await context.RefreshTokens.AddAsync(token);
        await context.SaveChangesAsync();
        
        var service = new RefreshTokenService(_logger, _hasherMock.Object, context);

        // Act
        var result = await service.VerifyTokenAsync(refreshToken, userId);

        // Assert
        Assert.Equal(RefreshTokenStatus.InvalidOrRevoked, result);
    }

    [Fact]
    public async Task VerifyTokenAsync_NoToken_ReturnsInvalidOrRevoked()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new RefreshTokenService(_logger, _hasherMock.Object, context);

        // Act
        var result = await service.VerifyTokenAsync("some-token", 999);

        // Assert
        Assert.Equal(RefreshTokenStatus.InvalidOrRevoked, result);
    }
}