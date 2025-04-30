namespace Fragmenta.Tests.IntegrationTests;

using Fragmenta.Api.Controllers;
using Fragmenta.Api.Dtos;
using System.Net;
using System.Net.Http.Json;
using Xunit;

public class BoardControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private const string BaseUrl = "/api/boards";

    public BoardControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetBoards_ReturnsOk_WhenMemberOfWorkspace()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, BaseUrl);
        request.Headers.Add("X-Workspace-Id", "1");
        request.Headers.Add("Test-UserId", "1");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetBoards_ReturnsForbidden_WhenNotMemberOfWorkspace()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, BaseUrl);
        request.Headers.Add("X-Workspace-Id", "2");
        request.Headers.Add("Test-UserId", "4");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetBoard_ReturnsOk_WhenMemberWithAccess()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/1");
        request.Headers.Add("X-Workspace-Id", "1");
        request.Headers.Add("Test-UserId", "1");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateBoard_ReturnsCreated_WhenAuthorized()
    {
        // Arrange
        var createRequest = new CreateBoardRequest { Name = "Test Board", AllowedTypeIds = []};
        var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl);
        request.Headers.Add("X-Workspace-Id", "1");
        request.Headers.Add("Test-UserId", "1");
        request.Content = JsonContent.Create(createRequest);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBoard_ReturnsOk_WhenAuthorized()
    {
        // Arrange
        var updateRequest = new UpdateBoardRequest { Name = "Updated Board", ArchivedAt = null, AllowedTypeIds = []};
        var request = new HttpRequestMessage(HttpMethod.Put, $"{BaseUrl}/1");
        request.Headers.Add("X-Workspace-Id", "1");
        request.Headers.Add("Test-UserId", "1");
        request.Content = JsonContent.Create(updateRequest);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DeleteBoard_ReturnsNoContent_WhenAuthorized()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/4");
        request.Headers.Add("X-Workspace-Id", "1");
        request.Headers.Add("Test-UserId", "1");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task AddGuests_ReturnsCreated_WhenAuthorized()
    {
        // Arrange
        var guestsRequest = new AddGuestsRequest { UsersId = [ 2, 3 ] };
        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/1/guests");
        request.Headers.Add("X-Workspace-Id", "1");
        request.Headers.Add("Test-UserId", "1");
        request.Content = JsonContent.Create(guestsRequest);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetGuests_ReturnsOk_WhenAuthorized()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/1/guests");
        request.Headers.Add("X-Workspace-Id", "1");
        request.Headers.Add("Test-UserId", "1");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RemoveGuest_ReturnsNoContent_WhenAuthorized()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/1/guests/6");
        request.Headers.Add("X-Workspace-Id", "1");
        request.Headers.Add("Test-UserId", "1");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}