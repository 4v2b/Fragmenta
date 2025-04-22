using Fragmenta.Api.Dtos;
using Fragmenta.Api.Enums;
using Fragmenta.Api.Utils;
using Fragmenta.Api.Contracts;
using Fragmenta.Api.Services;
using Fragmenta.Dal;
using Fragmenta.Dal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using Task = System.Threading.Tasks.Task;
using Xunit;

namespace Fragmenta.Tests.UnitTests;

public class AuthServiceTests : UnitTestsBase
{
    private async Task<ApplicationContext> GetContextWithUserAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationContext(options);
        
        var hashingMock = new Mock<IHashingService>();
        hashingMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<byte[]>()))
            .Returns<string, string, string>((password, hash, salt) => password == "correct");


        var salt = SaltGenerator.GenerateSalt();
        context.Users.Add(new User
        {
            Id = 1,
            Email = "test@example.com",
            Name = "Test",
            PasswordSalt = salt,
            PasswordHash = [1, 2, 3, 4],
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
        return context;
    }

    [Fact]
    public async Task AuthorizeAsync_SuccessfulLogin_ReturnsSuccess()
    {
        var context = await GetContextWithUserAsync();
        var hashingMock = new Mock<IHashingService>();
        hashingMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<byte[]>()))
            .Returns<string, byte[], byte[]>((password, hash, salt) => password == "password123");

        var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new AuthService(context, new NullLogger<UserAccountService>(), hashingMock.Object, cache);

        var result = await service.AuthorizeAsync(new LoginRequest
        {
            Email = "test@example.com",
            Password = "password123"
        });

        Assert.True(result.IsSuccess);
        Assert.Equal("test@example.com", result.User.Email);
    }

    [Fact]
    public async Task AuthorizeAsync_WrongPassword_IncrementsAttempts()
    {
        var context = await GetContextWithUserAsync();
        
        var hashingMock = new Mock<IHashingService>();
        hashingMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<byte[]>()))
            .Returns<string, byte[], byte[]>((password, hash, salt) => password == "correct");
        
        var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new AuthService(context, new NullLogger<UserAccountService>(), hashingMock.Object, cache);

        var result = await service.AuthorizeAsync(new LoginRequest
        {
            Email = "test@example.com",
            Password = "wrong"
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(AuthErrorType.PasswordInvalid, result.Error);
    }

    [Fact]
    public async Task AuthorizeAsync_LockedUser_ReturnsLocked()
    {
        var context = await GetContextWithUserAsync();
        var hashingMock = new Mock<IHashingService>();
        hashingMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<byte[]>()))
            .Returns<string, byte[], byte[]>((password, hash, salt) => password == "password");

        var cache = new MemoryCache(new MemoryCacheOptions());

        cache.Set("failed_attempts_test@example.com", (2, DateTime.UtcNow.AddMinutes(5)));
        var service = new AuthService(context, new NullLogger<UserAccountService>(), hashingMock.Object, cache);

        var result = await service.AuthorizeAsync(new LoginRequest
        {
            Email = "test@example.com",
            Password = "password123"
        });

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.IsLocked);
    }

    [Fact]
    public async Task RegisterAsync_NewUser_ReturnsSuccess()
    {
        var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationContext(options);
        var hashingMock = new Mock<IHashingService>();
        hashingMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<byte[]>()))
            .Returns<string, string, string>((password, hash, salt) => password == "correct");

        var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new AuthService(context, new NullLogger<UserAccountService>(), hashingMock.Object, cache);

        var result = await service.RegisterAsync(new RegisterRequest
        {
            Email = "new@example.com",
            Name = "New User",
            Password = "securePass!"
        });

        Assert.True(result.IsSuccess);
        Assert.Equal("new@example.com", result.User.Email);
    }

    [Fact]
    public async Task RegisterAsync_ExistingUser_ReturnsUserExists()
    {
        var context = await GetContextWithUserAsync();
        var hashingMock = new Mock<IHashingService>();
        hashingMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<byte[]>()))
            .Returns<string, string, string>((password, hash, salt) => password == "correct");
        var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new AuthService(context, new NullLogger<UserAccountService>(), hashingMock.Object, cache);

        var result = await service.RegisterAsync(new RegisterRequest
        {
            Email = "test@example.com",
            Name = "Any",
            Password = "123"
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(AuthErrorType.UserExists, result.Error);
    }

    // Додаткові можливі тести:
    // - AuthorizeAsync: неіснуючий email → повертає UserNonExistent
    // - AuthorizeAsync: досягнення ліміту 3 спроб → заблокований на 10 хв
    // - RegisterAsync: пароль з null або дуже короткий → очікуємо обробку
    // - AuthorizeAsync: edge case - спроба в момент завершення lockout
}