namespace Fragmenta.Tests.IntegrationTests;

using Fragmenta.Api.Controllers;
using Fragmenta.Api.Dtos;
using System.Net;
using System.Net.Http.Json;
using Xunit;

public class TaskControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private const string BaseUrl = "/api/tasks";

    public TaskControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTasks_ReturnsOk_WhenMemberWithAccess()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}?boardId=1");
        request.Headers.Add("X-Workspace-Id", "1");
        request.Headers.Add("Test-UserId", "1");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetTasks_ReturnsForbidden_WhenGuestWithoutAccess()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}?boardId=1");
        request.Headers.Add("X-Workspace-Id", "1");
        request.Headers.Add("Test-UserId", "4"); // Guest without access

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    

    [Fact]
    public async Task CreateTask_ReturnsCreated_WhenAuthorized()
    {
        // Arrange
        var createRequest = new CreateOrUpdateTaskRequest
        {
            Title = "Test Task",
            Description = "Description",
            Priority = 0,
            AssigneeId = null,
            Weight = 100, TagsId = [], DueDate = null
        };
        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}?statusId=1");
        request.Headers.Add("X-Workspace-Id", "1");
        request.Headers.Add("Test-UserId", "1");
        request.Content = JsonContent.Create(createRequest);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateTask_ReturnsForbidden_WhenNotAuthorized()
    {
        // Arrange - assuming role Guest cannot manage board content
        var createRequest = new CreateOrUpdateTaskRequest
        {
            Title = "Test Task",
            Description = "Description",
            Priority = 0,
            AssigneeId = null,
            Weight = 100, TagsId = [], DueDate = null
        };
        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}?statusId=1");
        request.Headers.Add("X-Workspace-Id", "1");
        request.Headers.Add("Test-UserId", "6"); // User with Guest role
        request.Content = JsonContent.Create(createRequest);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Reorder_ReturnsNoContent_WhenAuthorized()
    {
        // Arrange
        var reorderRequest = new ShallowUpdateTaskRequest
        {
            Id = 1,
            BoardId = 1,
            StatusId = 2,
            Weight = 100
        };
        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/reorder");
        request.Headers.Add("X-Workspace-Id", "1");
        request.Headers.Add("Test-UserId", "1");
        request.Content = JsonContent.Create(reorderRequest);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTask_ReturnsNoContent_WhenAuthorized()
    {
        // Arrange
        var updateRequest = new UpdateTaskRequest
        {
            Title = "Test Task",
            Description = "Description",
            Priority = 0,
            AssigneeId = null,
            TagsId = [], DueDate = null
        };
        var request = new HttpRequestMessage(HttpMethod.Put, $"{BaseUrl}/2");
        request.Headers.Add("X-Workspace-Id", "1");
        request.Headers.Add("Test-UserId", "1");
        request.Content = JsonContent.Create(updateRequest);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTask_ReturnsNoContent_WhenAuthorized()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/1");
        request.Headers.Add("X-Workspace-Id", "1");
        request.Headers.Add("Test-UserId", "1");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}