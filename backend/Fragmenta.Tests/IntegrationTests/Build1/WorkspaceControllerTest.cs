using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Fragmenta.Api.Dtos;
using Fragmenta.Dal;
using Fragmenta.Dal.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Fragmenta.Tests.IntegrationTests;

public class WorkspaceControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;
    private const string BaseUrl = "/api/workspaces";

    public WorkspaceControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        SetupAuthentication();
    }

    private void SetupAuthentication()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WhenAuthenticated()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, BaseUrl);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var workspaces = JsonSerializer.Deserialize<List<WorkspaceDto>>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(workspaces);
        Assert.True(workspaces.Count > 0);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenValidRequest()
    {
        // Arrange
        var requestBody = new CreateOrUpdateWorkspaceRequest
        {
            Name = "New Test Workspace"
        };

        var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json")
        };

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var workspace = JsonSerializer.Deserialize<WorkspaceDto>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(workspace);
        Assert.Equal("New Test Workspace", workspace.Name);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenInvalidRequest()
    {
        // Arrange
        var requestBody = new CreateOrUpdateWorkspaceRequest
        {
            Name = "" // Empty name should be invalid
        };

        var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl)
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

    [Fact]
    public async Task Update_ReturnsOk_WhenAuthorizedAsOwner()
    {
        // Arrange
        var requestBody = new CreateOrUpdateWorkspaceRequest
        {
            Name = "Updated Workspace Name"
        };

        var request = new HttpRequestMessage(HttpMethod.Put, $"{BaseUrl}/1")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json")
        };
        request.Headers.Add("Test-UserId", "1");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var workspace = JsonSerializer.Deserialize<WorkspaceDto>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(workspace);
        Assert.Equal("Updated Workspace Name", workspace.Name);
    }

    [Fact]
    public async Task Update_ReturnsForbidden_WhenNotAdmin()
    {
        // Arrange
        // Set user ID to one that doesn't have admin rights for workspace 2

        var requestBody = new CreateOrUpdateWorkspaceRequest
        {
            Name = "Updated Workspace Name"
        };

        var request = new HttpRequestMessage(HttpMethod.Put, $"{BaseUrl}/2")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json")
        };

        request.Headers.Add("Test-UserId", "3"); // Regular member, not admin

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenAuthorizedAsOwner()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/2");
        request.Headers.Add("Test-UserId", "2");
        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsForbidden_WhenNotOwner()
    {
        // Arrange
        // Set user ID to one that doesn't have owner rights 
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/1");
        request.Headers.Add("Test-UserId", "2"); // Admin but not owner

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}