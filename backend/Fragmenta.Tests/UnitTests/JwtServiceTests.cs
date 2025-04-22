using Fragmenta.Api.Configuration;
using Fragmenta.Api.Dtos;
using Fragmenta.Api.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace Fragmenta.Tests.UnitTests;

public class JwtServiceTests : UnitTestsBase
{
    private readonly JwtOptions _jwtOptions;
    private readonly Mock<IOptions<JwtOptions>> _optionsMock;
    private readonly JwtService _jwtService;

    public JwtServiceTests()
    {
        _jwtOptions = new JwtOptions
        {
            Key = "very-secure-test-key-with-sufficient-length-for-hmacsha256",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpireMinutes = 60
        };

        _optionsMock = new Mock<IOptions<JwtOptions>>();
        _optionsMock.Setup(x => x.Value).Returns(_jwtOptions);
        
        _jwtService = new JwtService(_optionsMock.Object);
    }

    [Fact]
    public void GenerateToken_ValidUser_ReturnsValidToken()
    {
        // Arrange
        var user = new UserDto 
        { 
            Id = 123, 
            Email = "test@example.com"
        };

        // Act
        var token = _jwtService.GenerateToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        // Verify the token can be read back
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        // Verify token properties
        Assert.Equal(_jwtOptions.Issuer, jwtToken.Issuer);
        Assert.Equal(_jwtOptions.Audience, jwtToken.Audiences.First());

        // Verify claims
        var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "email");
        var idClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid");
        
        Assert.NotNull(emailClaim);
        Assert.NotNull(idClaim);
        Assert.Equal(user.Email, emailClaim.Value);
        Assert.Equal(user.Id.ToString(), idClaim.Value);
    }

    [Fact]
    public void GenerateToken_NullUser_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _jwtService.GenerateToken(null));
    }

    [Fact]
    public void Constructor_NullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        Mock<IOptions<JwtOptions>> nullOptions = new Mock<IOptions<JwtOptions>>();
        nullOptions.Setup(x => x.Value).Returns((JwtOptions)null);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new JwtService(nullOptions.Object));
    }
}