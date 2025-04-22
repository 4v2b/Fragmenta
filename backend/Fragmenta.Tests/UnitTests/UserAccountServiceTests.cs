using Fragmenta.Api.Contracts;
using Fragmenta.Api.Services;
using Fragmenta.Dal.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace Fragmenta.Tests.UnitTests;

public class UserAccountServiceTests: UnitTestsBase 
{
    private readonly ILogger<UserAccountService> _logger;
    private readonly Mock<IHashingService> _hasherMock;

    public UserAccountServiceTests()
    {
        _logger = new NullLogger<UserAccountService>();
        _hasherMock = new Mock<IHashingService>();
    }

    [Fact]
    public async Task ChangePasswordAsync_UserExists_ReturnsTrue()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userId = 1L;
        var user = new User { Id = userId, Email = "test@example.com", Name = "Test User", PasswordHash = [1, 2, 3, 4], PasswordSalt = [] };
        
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        
        _hasherMock.Setup(h => h.Hash(It.IsAny<string>(), null)).Returns(new byte[] { 1, 2, 3, 4 });
        _hasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<byte[]>())).Returns(true);
        
        var service = new UserAccountService(_logger, context, _hasherMock.Object);
        var password = "password";
        
        // Act
        var result = await service.ChangePasswordAsync(password, password, userId);

        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public async Task ChangePasswordAsync_NonExistingUser_ThrowsArgumentException()
    {
        // Arrange
        var context = CreateInMemoryContext();
        
        var service = new UserAccountService(_logger, context, _hasherMock.Object);
        var password = "password";

        // Act + Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await service.ChangePasswordAsync(password, password, 1));
    }
    
    [Fact]
    public async Task ChangePasswordAsync_HashDoesntMatch_ReturnsFalse()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userId = 1L;
        var user = new User { Id = userId, Email = "test@example.com", Name = "Test User", PasswordHash = [1, 2, 3, 4], PasswordSalt = [] };
        
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        
        _hasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<byte[]>())).Returns(false);
        
        var service = new UserAccountService(_logger, context, _hasherMock.Object);
        var password = "password";
        
        // Act
        var result = await service.ChangePasswordAsync(password, password, userId);

        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public async Task ChangeNameAsync_UserExists_ReturnsTrue()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userId = 1L;
        var user = new User { Id = userId, Email = "test@example.com", Name = "Test User", PasswordHash = [1, 2, 3, 4], PasswordSalt = [] };
        
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        
        var service = new UserAccountService(_logger, context, _hasherMock.Object);
        
        // Act
        var result = await service.ChangeNameAsync("name", 1);

        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public async Task ChangeNameAsync_NonExistingUser_ReturnsFalse()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new UserAccountService(_logger, context, _hasherMock.Object);
        
        // Act
        var result = await service.ChangeNameAsync("name", 1);

        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public async Task DeleteAsync_UserExists_ReturnsTrue()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userId = 1L;
        var user = new User { Id = userId, Email = "test@example.com", Name = "Test User", PasswordHash = [1, 2, 3, 4], PasswordSalt = [] };
        
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        
        _hasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<byte[]>())).Returns(true);
        
        var service = new UserAccountService(_logger, context, _hasherMock.Object);
        
        // Act
        var result = await service.DeleteAsync("name", 1);

        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public async Task DeleteAsync_WrongPassword_ReturnsFalse()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userId = 1L;
        var user = new User { Id = userId, Email = "test@example.com", Name = "Test User", PasswordHash = [1, 2, 3, 4], PasswordSalt = [] };
        
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        
        _hasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<byte[]>())).Returns(false);
        
        var service = new UserAccountService(_logger, context, _hasherMock.Object);
        var password = "password";
        
        // Act
        var result = await service.DeleteAsync(password, userId);

        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public async Task DeleteAsync_NonExistingUser_ThrowsArgumentException()
    {
        // Arrange
        var context = CreateInMemoryContext();
        
        var service = new UserAccountService(_logger, context, _hasherMock.Object);
        var password = "password";

        // Act + Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await service.DeleteAsync(password, 1));
    }
    
    [Fact]
    public async Task ResetPasswordAsync_NonExistingUser_ReturnsFalse()
    {
        // Arrange
        var context = CreateInMemoryContext();
        
        var service = new UserAccountService(_logger, context, _hasherMock.Object);
        var password = "password";
        
        // Act
        var result = await service.ResetPasswordAsync(password, 1);

        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public async Task ResetPasswordAsync_UserExists_ReturnsTrue()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userId = 1L;
        var user = new User { Id = userId, Email = "test@example.com", Name = "Test User", PasswordHash = [1, 2, 3, 4], PasswordSalt = [] };
        
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        
        var password = "password";
        
        _hasherMock.Setup(h => h.Hash(It.IsAny<string>(), null)).Returns(new byte[] { 1, 2, 3, 4 });
        var service = new UserAccountService(_logger, context, _hasherMock.Object);
        
        // Act
        var result = await service.ResetPasswordAsync(password, 1);

        // Assert
        Assert.True(result);
    }

}