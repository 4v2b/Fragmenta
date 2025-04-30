using Microsoft.Extensions.DependencyInjection;

namespace Fragmenta.Tests.IntegrationTests;

using Fragmenta.Api.Dtos;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Xunit;
using Azure.Storage.Blobs;
using System.IO;

public class AttachmentControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private const string BaseUrl = "/api";

    public AttachmentControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAttachmentTypes_ReturnsOk_WhenAuthenticated()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/attachment-types");
        request.Headers.Add("X-Workspace-Id", "1");
        request.Headers.Add("Test-UserId", "1");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAttachments_ReturnsOk_WhenAuthorized()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/attachments?taskId=1");
        request.Headers.Add("X-Workspace-Id", "1");
        request.Headers.Add("Test-UserId", "1");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task GetAttachments_ReturnsForbidden_WhenNotMember()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/attachments?taskId=1");
        request.Headers.Add("X-Workspace-Id", "1");
        request.Headers.Add("Test-UserId", "4");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}