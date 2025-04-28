using System.Net;
using System.Text;
using System.Text.Json;
using Fragmenta.Api.Dtos;

namespace Fragmenta.Tests.IntegrationTests;

public class AccessControllerTests : IClassFixture<TestWebApplicationFactoryContainers>
{
    private readonly HttpClient _client;
    private const string BaseUrl = "/api/members";
    private string _authToken;

    public AccessControllerTests(TestWebApplicationFactoryContainers factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetMembers_ReturnsOk_WhenAuthorized()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, BaseUrl);
        request.Headers.Add("X-Workspace-Id", "1");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetMembers_ReturnsForbidden_WhenNotMemberOfWorkspace()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, BaseUrl);
        request.Headers.Add("X-Workspace-Id", "2");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AddMembers_ReturnsCreated_WhenAuthorizedAsAdmin()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl);
        request.Headers.Add("X-Workspace-Id", "1");
        request.Content = new StringContent(
            JsonSerializer.Serialize(new MemberRequest { UsersId = new[] { 4L } }),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task AddMembers_ReturnsForbidden_WhenNotAdmin()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl);
        request.Headers.Add("X-Workspace-Id", "3"); // Workspace where user is regular member
        request.Content = new StringContent(
            JsonSerializer.Serialize(new MemberRequest { UsersId = new[] { 5L } }),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task RemoveMember_ReturnsNoContent_WhenAuthorizedAsAdmin()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/2");
        request.Headers.Add("X-Workspace-Id", "1");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task RemoveMember_ReturnsNoContent_WhenRemoveSelfAsMember()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/1"); // Try to remove admin
        request.Headers.Add("X-Workspace-Id", "3");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
    
    [Fact]
    public async Task RemoveMember_ReturnsForbidden_WhenRemoveSelfAsOwner()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/1"); // Try to remove admin
        request.Headers.Add("X-Workspace-Id", "1");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task RevokeAdmin_ReturnsNoContent_WhenAuthorizedAsOwner()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, $"/members/5/revoke");
        request.Headers.Add("X-Workspace-Id", "1");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GrantAdmin_ReturnsNoContent_WhenAuthorizedAsOwner()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, $"/members/3/grant");
        request.Headers.Add("X-Workspace-Id", "1");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}