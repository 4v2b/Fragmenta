using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
using Fragmenta.Api.Services;
using Fragmenta.Dal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Fragmenta.Tests.UnitTests;

public class RefreshTokenLookupServiceTests : UnitTestsBase
{
    private readonly ILogger<RefreshTokenLookupService> _logger;
    private readonly Mock<IHashingService> _hasherMock;

    public RefreshTokenLookupServiceTests()
    {
        _logger = new NullLogger<RefreshTokenLookupService>();
        _hasherMock = new Mock<IHashingService>();
    }

    [Fact]
    public async Task GetUserByTokenAsync_ValidToken_ReturnsUser()
    {
        // Arrange
        var context = CreateInMemoryContext();
        
        var token = "valid-token";
        var hashedToken = new byte[] { 1, 2, 3, 4 };
        
        _hasherMock.Setup(h => h.Hash(token, null)).Returns(hashedToken);
        
        var user = new User { Id = 1, Email = "test@example.com", Name = "Test User", PasswordHash = [], PasswordSalt = []};
        var refreshToken = new RefreshToken 
        { 
            Id = 1,
            TokenHash = hashedToken,
            User = user,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow,
            RevokedAt = null
        };
        
        await context.Users.AddAsync(user);
        await context.RefreshTokens.AddAsync(refreshToken);
        await context.SaveChangesAsync();
        
        var service = new RefreshTokenLookupService(_logger, _hasherMock.Object, context);

        // Act
        var result = await service.GetUserByTokenAsync(token);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Email, result.Email);
    }

    [Fact]
    public async Task GetUserByTokenAsync_InvalidToken_ReturnsNull()
    {
        // Arrange
        var context = CreateInMemoryContext();
        
        var token = "invalid-token";
        var hashedToken = new byte[] { 1, 2, 3, 4 };
        
        _hasherMock.Setup(h => h.Hash(token, null)).Returns(hashedToken);
        
        var service = new RefreshTokenLookupService(_logger, _hasherMock.Object, context);

        // Act
        var result = await service.GetUserByTokenAsync(token);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task HasValidTokenAsync_UserHasValidToken_ReturnsTrue()
    {
        // Arrange
        var context = CreateInMemoryContext();
        
        var userId = 1L;
        var refreshToken = new RefreshToken 
        { 
            Id = 1,
            TokenHash = new byte[] { 1, 2, 3, 4 },
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow,
            RevokedAt = null
        };
        
        await context.RefreshTokens.AddAsync(refreshToken);
        await context.SaveChangesAsync();
        
        var service = new RefreshTokenLookupService(_logger, _hasherMock.Object, context);

        // Act
        var result = await service.HasValidTokenAsync(userId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasValidTokenAsync_ExpiredToken_ReturnsFalse()
    {
        // Arrange
        var context = CreateInMemoryContext();
        
        var userId = 1L;
        var refreshToken = new RefreshToken 
        { 
            Id = 1,
            TokenHash = new byte[] { 1, 2, 3, 4 },
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expired
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            RevokedAt = null
        };
        
        await context.RefreshTokens.AddAsync(refreshToken);
        await context.SaveChangesAsync();
        
        var service = new RefreshTokenLookupService(_logger, _hasherMock.Object, context);

        // Act
        var result = await service.HasValidTokenAsync(userId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HasValidTokenAsync_RevokedToken_ReturnsFalse()
    {
        // Arrange
        var context = CreateInMemoryContext();
        
        var userId = 1L;
        var refreshToken = new RefreshToken 
        { 
            Id = 1,
            TokenHash = new byte[] { 1, 2, 3, 4 },
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            RevokedAt = DateTime.UtcNow // Revoked
        };
        
        await context.RefreshTokens.AddAsync(refreshToken);
        await context.SaveChangesAsync();
        
        var service = new RefreshTokenLookupService(_logger, _hasherMock.Object, context);

        // Act
        var result = await service.HasValidTokenAsync(userId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HasValidTokenAsync_NoToken_ReturnsFalse()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new RefreshTokenLookupService(_logger, _hasherMock.Object, context);

        // Act
        var result = await service.HasValidTokenAsync(999);

        // Assert
        Assert.False(result);
    }
}