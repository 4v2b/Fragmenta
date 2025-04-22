using System.Text;
using Fragmenta.Api.Services;

namespace Fragmenta.Tests.UnitTests;

public class Sha256HashingServiceTests : UnitTestsBase
{
    private readonly Sha256HashingService _hashingService;

    public Sha256HashingServiceTests()
    {
        _hashingService = new Sha256HashingService();
    }

    [Fact]
    public void Hash_WithoutSalt_ReturnsCorrectHash()
    {
        // Arrange
        string input = "test-password";
        
        // Act
        var hash1 = _hashingService.Hash(input);
        var hash2 = _hashingService.Hash(input);
        
        // Assert
        Assert.NotNull(hash1);
        Assert.Equal(32, hash1.Length); // SHA-256 produces 32 byte hash
        Assert.Equal(hash1, hash2); // Same input should produce same hash
    }

    [Fact]
    public void Hash_WithSalt_ReturnsCorrectHash()
    {
        // Arrange
        string input = "test-password";
        byte[] salt = Encoding.UTF8.GetBytes("somesalt");
        
        // Act
        var hash1 = _hashingService.Hash(input, salt);
        var hash2 = _hashingService.Hash(input, salt);
        
        // Assert
        Assert.NotNull(hash1);
        Assert.Equal(32, hash1.Length);
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void Hash_SameInputDifferentSalt_ReturnsDifferentHash()
    {
        // Arrange
        string input = "test-password";
        byte[] salt1 = "salt1"u8.ToArray();
        byte[] salt2 = "salt2"u8.ToArray();
        
        // Act
        var hash1 = _hashingService.Hash(input, salt1);
        var hash2 = _hashingService.Hash(input, salt2);
        
        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void Verify_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        string password = "correct-password";
        var hash = _hashingService.Hash(password);
        
        // Act
        var result = _hashingService.Verify(password, hash);
        
        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Verify_IncorrectPassword_ReturnsFalse()
    {
        // Arrange
        string correctPassword = "correct-password";
        string wrongPassword = "wrong-password";
        var hash = _hashingService.Hash(correctPassword);
        
        // Act
        var result = _hashingService.Verify(wrongPassword, hash);
        
        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Verify_WithSalt_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        string password = "test-password";
        byte[] salt = "somesalt"u8.ToArray();
        var hash = _hashingService.Hash(password, salt);
        
        // Act
        var result = _hashingService.Verify(password, hash, salt);
        
        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Verify_WithSalt_IncorrectPassword_ReturnsFalse()
    {
        // Arrange
        string correctPassword = "correct-password";
        string wrongPassword = "wrong-password";
        byte[] salt = "somesalt"u8.ToArray();
        var hash = _hashingService.Hash(correctPassword, salt);
        
        // Act
        var result = _hashingService.Verify(wrongPassword, hash, salt);
        
        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Verify_WrongSalt_ReturnsFalse()
    {
        // Arrange
        string password = "test-password";
        byte[] salt1 = "salt1"u8.ToArray();
        byte[] salt2 = "salt2"u8.ToArray();
        var hash = _hashingService.Hash(password, salt1);
        
        // Act
        var result = _hashingService.Verify(password, hash, salt2);
        
        // Assert
        Assert.False(result);
    }
}