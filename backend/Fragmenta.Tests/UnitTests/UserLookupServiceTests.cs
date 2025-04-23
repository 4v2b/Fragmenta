using Fragmenta.Api.Contracts;
using Fragmenta.Api.Services;
using Fragmenta.Dal;
using Fragmenta.Dal.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace Fragmenta.Tests.UnitTests;

public class UserLookupServiceTests : UnitTestsBase
{
    private readonly ILogger<UserLookupService> _logger;
    private readonly Mock<IHashingService> _hasherMock;

    public UserLookupServiceTests()
    {
        _logger = new NullLogger<UserLookupService>();
        _hasherMock = new Mock<IHashingService>();
    }
    private async Task<ApplicationContext> CreateInMemoryContextWithUsers()
    {
        var context = CreateInMemoryContext();
        
        context.Users.AddRange(new User
        {
            Id = 1,
            Email = "test1@example.com",
            Name = "Test 1",
            PasswordSalt = [],
            PasswordHash = [1, 2, 3, 4]
        }, new User
        {
            Id = 2,
            Email = "test2@example.com",
            Name = "Test 2",
            PasswordSalt = [],
            PasswordHash = [1, 2, 3, 4]
        }, new User
        {
            Id = 3,
            Email = "test3@example.com",
            Name = "Test 3",
            PasswordSalt = [],
            PasswordHash = [1, 2, 3, 4]
        });

        await context.SaveChangesAsync();
        return context;
    } 
    
        [Fact]
        public async Task FindManyByEmailsAsync_ReturnsMatchingUsers()
        {
            // Arrange
            var context = await CreateInMemoryContextWithUsers();
            
            var service = new UserLookupService(_logger, context, _hasherMock.Object);
            var emails = new[] { "test1@example.com", "test3@example.com" };

            // Act
            var result = await service.FindManyByEmailsAsync(emails);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, u => u.Email == "test1@example.com" && u.Id == 1);
            Assert.Contains(result, u => u.Email == "test3@example.com" && u.Id == 3);
        }

        [Fact]
        public async Task FindByEmailAsync_ReturnsUsersWithMatchingEmailPart()
        {
            // Arrange
            var context = await CreateInMemoryContextWithUsers();
            
            var service = new UserLookupService(_logger, context, _hasherMock.Object);

            // Act
            var result = await service.FindByEmailAsync("test");

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Contains(result, u => u.Email == "test1@example.com");
            Assert.Contains(result, u => u.Email == "test2@example.com");
            Assert.Contains(result, u => u.Email == "test3@example.com");
        }

        [Fact]
        public async Task GetUserInfoAsync_ReturnsFullUserInfo_WhenUserExists()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var testUser = new User { Id = 1, Email = "test@example.com", Name = "Test User", PasswordSalt = [],
                PasswordHash = [1, 2, 3, 4]};
            
            context.Users.Add(testUser);
            await context.SaveChangesAsync();
            
            var service = new UserLookupService(_logger, context, _hasherMock.Object);

            // Act
            var result = await service.GetUserInfoAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test@example.com", result.Email);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test User", result.Name);
        }

        [Fact]
        public async Task GetUserInfoAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var service = new UserLookupService(_logger, context, _hasherMock.Object);

            // Act
            var result = await service.GetUserInfoAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task FindSingleByEmailAsync_ReturnsUserId_WhenEmailExists()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var testUser = new User { Id = 42, Email = "unique@example.com", Name = "Unique User", PasswordSalt = [],
                PasswordHash = [1, 2, 3, 4] };
            
            context.Users.Add(testUser);
            await context.SaveChangesAsync();
            
            var service = new UserLookupService(_logger, context, _hasherMock.Object);

            // Act
            var result = await service.FindSingleByEmailAsync("unique@example.com");

            // Assert
            Assert.Equal(42, result);
        }

        [Fact]
        public async Task FindSingleByEmailAsync_ReturnsNull_WhenEmailDoesNotExist()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var service = new UserLookupService(_logger, context, _hasherMock.Object);

            // Act
            var result = await service.FindSingleByEmailAsync("nonexistent@example.com");

            // Assert
            Assert.Null(result);
        }
}