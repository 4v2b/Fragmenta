using System.Net;
using System.Text;
using System.Text.Json;
using Fragmenta.Api.Dtos;

namespace Fragmenta.Tests.IntegrationTests;

public class AuthControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;
    private const string BaseUrl = "/api";

    public AuthControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_ReturnsOk_WhenCredentialsValid()
    {
        // Arrange
        var requestBody = new LoginRequest
        {
            Email = "test1@example.com",
            Password = "Password1234"
        };
        
        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/login")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json")
        };

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        Assert.NotNull(tokenResponse);
        Assert.NotNull(tokenResponse.AccessToken);
        Assert.NotNull(tokenResponse.RefreshToken);
    }

    [Fact]
    public async Task Login_ReturnsBadRequest_WhenPasswordInvalid()
    {
        // Arrange
        var requestBody = new LoginRequest
        {
            Email = "test1@example.com",
            Password = "wrongPassword1234"
        };
        
        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/login")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json")
        };

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<ErrorResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        Assert.NotNull(error);
        Assert.Equal("auth.errors.passwordInvalid", error.Message);
    }

    [Fact]
    public async Task Login_ReturnsBadRequest_WhenUserDoesNotExist()
    {
        // Arrange
        var requestBody = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "anyPassword"
        };
        
        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/login")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json")
        };

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<ErrorResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        Assert.NotNull(error);
        Assert.Equal("auth.errors.userDoesntExist", error.Message);
    }

    [Fact]
    public async Task Register_ReturnsOk_WhenRegistrationSuccessful()
    {
        // Arrange
        var requestBody = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "Password1234",
            Name = "New User"
        };
        
        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/register")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json")
        };

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        Assert.NotNull(tokenResponse);
        Assert.NotNull(tokenResponse.AccessToken);
        Assert.NotNull(tokenResponse.RefreshToken);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenUserExists()
    {
        // Arrange
        var requestBody = new RegisterRequest
        {
            Email = "test1@example.com", // Existing user
            Password = "Password1234",
            Name = "Existing User"
        };
        
        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/register")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json")
        };

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<ErrorResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        Assert.NotNull(error);
        Assert.Equal("auth.errors.userExists", error.Message);
    }

    [Fact]
    public async Task Refresh_ReturnsOk_WhenRefreshTokenValid()
    {
        // First login to get a valid refresh token
        var loginRequest = new LoginRequest
        {
            Email = "test1@example.com",
            Password = "Password1234"
        };
        
        var loginResponse = await _client.PostAsync($"{BaseUrl}/login", 
            new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json"));
        
        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var tokens = JsonSerializer.Deserialize<TokenResponse>(loginContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        // Now use the refresh token
        var refreshRequest = new RefreshRequest
        {
            RefreshToken = tokens.RefreshToken
        };
        
        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/refresh")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(refreshRequest),
                Encoding.UTF8,
                "application/json")
        };

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var tokenResponse = await response.Content.ReadAsStringAsync();
        //var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        Assert.NotNull(tokenResponse);
    }

    [Fact]
    public async Task Refresh_ReturnsUnauthorized_WhenRefreshTokenInvalid()
    {
        // Arrange
        var refreshRequest = new RefreshRequest
        {
            RefreshToken = "invalid-refresh-token"
        };
        
        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/refresh")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(refreshRequest),
                Encoding.UTF8,
                "application/json")
        };

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_ReturnsOk_WhenTokenValid()
    {
        // Need to set up a valid reset token in the database first
        var userId = 2;
        var resetToken = "valid-reset-token";
        
        // Arrange
        var requestBody = new ResetPasswordRequest
        {
            Token = resetToken,
            UserId = userId,
            NewPassword = "newValidPassword123"
        };
        
        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/reset-password")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json")
        };

        // Act
        var response = await _client.SendAsync(request);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_ReturnsBadRequest_WhenTokenInvalid()
    {
        // Arrange
        var requestBody = new ResetPasswordRequest
        {
            Token = "invalid-token",
            UserId = 1,
            NewPassword = "newValidPassword123"
        };
        
        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/reset-password")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json")
        };

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<ErrorResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        Assert.NotNull(error.Message);
    }

    [Fact]
    public async Task UserLookup_ReturnsMatchingUsers()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/lookup-users?email=test");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var users = JsonSerializer.Deserialize<List<UserDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        Assert.NotNull(users);
        Assert.Contains(users, u => u.Email == "test1@example.com");
    }

    // Helper class for deserializing error responses
    private class ErrorResponse
    {
        public string Message { get; set; }
    }
}