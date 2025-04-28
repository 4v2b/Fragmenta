using System.Net;
using System.Text;
using System.Text.Json;
using Fragmenta.Api.Dtos;

namespace Fragmenta.Tests.IntegrationTests;

public class StatusControllerTests: IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private const string BaseUrl = "api/statuses";

    public StatusControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateStatus_ReturnsCreated_WhenAuthorizedInWorkspace()
    {
        // Arrange
        
        var requestBody = new CreateOrUpdateStatusRequest
        {
            ColorHex = "#FFFFFF",
            MaxTasks = null,
            Weight = 0,
            Name = "Status"
        };
        
        var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl + "?boardId=1")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json")
        };
        
        request.Headers.Add("X-Workspace-Id", "1");
        request.Headers.Add("Test-UserId", "1");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
    
    [Fact]
    public async Task CreateStatus_ReturnsForbidden_WhenGuest()
    {
        // Arrange
        
        var requestBody = new CreateOrUpdateStatusRequest
        {
            ColorHex = "#FFFFFF",
            MaxTasks = null,
            Weight = 0,
            Name = "Status"
        };
        
        var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl + "?boardId=1")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json")
        };
        
        request.Headers.Add("X-Workspace-Id", "1");
        request.Headers.Add("Test-UserId", "6");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    
    [Fact]
    public async Task UpdateStatus_ReturnsOk_WhenAuthorizedInWorkspace()
    {
        // Arrange
        
        var requestBody = new CreateOrUpdateStatusRequest
        {
            ColorHex = "#FFFFFF",
            MaxTasks = null,
            Weight = 0,
            Name = "Status"
        };
        
        var request = new HttpRequestMessage(HttpMethod.Put, BaseUrl + "/1")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json")
        };
        
        request.Headers.Add("X-Workspace-Id", "1");
        request.Headers.Add("Test-UserId", "1");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task UpdateStatus_ReturnsForbidden_WhenGuest()
    {
        // Arrange
        
        var requestBody = new CreateOrUpdateStatusRequest
        {
            ColorHex = "#FFFFFF",
            MaxTasks = null,
            Weight = 0,
            Name = "Status"
        };
        
        var request = new HttpRequestMessage(HttpMethod.Put, BaseUrl + "/1")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json")
        };
        
        request.Headers.Add("X-Workspace-Id", "1");
        request.Headers.Add("Test-UserId", "6");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteStatus_ReturnsNoContent_WhenNotGuest()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/3");
        request.Headers.Add("X-Workspace-Id", "2");
        request.Headers.Add("Test-UserId", "2");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
    
    [Fact]
    public async Task DeleteTag_ReturnsForbidden_WhenGuest()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/3");
        request.Headers.Add("X-Workspace-Id", "2");
        request.Headers.Add("Test-UserId", "3");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}