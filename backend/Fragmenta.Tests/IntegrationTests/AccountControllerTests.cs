using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Fragmenta.Api.Dtos;

namespace Fragmenta.Tests.IntegrationTests;

public class AccountControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private const string BaseUrl = "/api/me";

    public AccountControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Me_ReturnsOk_WhenAuthenticated()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, BaseUrl);
        
        request.Headers.Add("Test-UserId", "1");
        request.Headers.Add("Test-Email", "test1@example.com");

        // Act
        var response = await _client.SendAsync(request);
        
        

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var user = JsonSerializer.Deserialize<UserFullDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(user);
        Assert.Equal(1, user.Id);
    }

    [Fact]
    public async Task DeleteAccount_ReturnsNoContent_WhenPasswordCorrect()
    {
        // Arrange
        
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}?password=Password1234");

        request.Headers.Add("Test-UserId", "4");
        request.Headers.Add("Test-Email", "test4@example.com");
        
        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteAccount_ReturnsForbidden_WhenPasswordIncorrect()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}?password=wrongPassword123");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    
    [Fact]
    public async Task DeleteAccount_ReturnsForbidden_WhenUserHaveWorkpspace()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}?password=Password1234");

        request.Headers.Add("Test-UserId", "1");
        request.Headers.Add("Test-Email", "test1@example.com");
        
        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ChangeName_ReturnsNoContent_WhenNameValid()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/name?newName=NewUserName");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task ChangeName_ReturnsBadRequest_WhenNameInvalid()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/name?newName=");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_ReturnsNoContent_WhenPasswordsValid()
    {
        // Arrange
        var requestBody = new ChangePasswordRequest
        {
            OldPassword = "Password1234",
            NewPassword = "Password12345"
        };
        
        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/password")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json")
        };

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_ReturnsBadRequest_WhenOldPasswordIncorrect()
    {
        // Arrange
        var requestBody = new ChangePasswordRequest
        {
            OldPassword = "wrongPassword",
            NewPassword = "newValidPassword123"
        };
        
        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/password")
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
    }
}