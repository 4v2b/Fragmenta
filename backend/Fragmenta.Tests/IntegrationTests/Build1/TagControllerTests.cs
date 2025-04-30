using System.Net;
using System.Text;
using System.Text.Json;
using Fragmenta.Api.Dtos;

namespace Fragmenta.Tests.IntegrationTests;

public class TagControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private const string BaseUrl = "/api/tags";

    public TagControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTags_ReturnsOk_WhenMemberOfWorkspace()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, BaseUrl + "?boardId=1");
        request.Headers.Add("X-Workspace-Id", "1");
        request.Headers.Add("Test-UserId", "1");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task GetTags_ReturnsForbidden_WhenNotMemberOfWorkspace()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, BaseUrl + "?boardId=2");
        request.Headers.Add("X-Workspace-Id", "2");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateTag_ReturnsCreated_WhenAuthorizedInWorkspace()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl + "?name=Tag&boardId=1");
        request.Headers.Add("X-Workspace-Id", "1");
        request.Headers.Add("Test-UserId", "1");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateTag_ReturnsForbidden_WhenGuest()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl + "?name=Tag&boardId=1");
        request.Headers.Add("X-Workspace-Id", "1");
        request.Headers.Add("Test-UserId", "6");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTag_ReturnsNoContent_WhenNotGuest()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/2");
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
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/1");
        request.Headers.Add("X-Workspace-Id", "1");
        request.Headers.Add("Test-UserId", "6");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}