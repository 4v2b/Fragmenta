using Fragmenta.Api.Dtos;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Xunit;
using Azure.Storage.Blobs;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Fragmenta.Tests.IntegrationTests;

public class ExternalServicesIntegrationTests : IClassFixture<TestWebApplicationFactoryContainers>
{
    private readonly HttpClient _client;
    private const string BaseUrl = "/api";

    public ExternalServicesIntegrationTests(TestWebApplicationFactoryContainers factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UploadAttachment_ReturnsCreated_WhenAuthorized()
    {
        // Arrange
        // Create a temporary file for testing
        var fileName = "test-file.txt";
        var filecontent = "This is a test file";
        var bytes = Encoding.UTF8.GetBytes(filecontent);

        // Set up the multipart form data
        var multipartContent = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
        multipartContent.Add(fileContent, "file", fileName);

        // Create and configure the request
        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/attachments?taskId=1")
        {
            Content = multipartContent
        };
        request.Headers.Add("X-Workspace-Id", "1");
        request.Headers.Add("Test-UserId", "1");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task DownloadAttachment_ReturnsFile_WhenAuthorized()
    {
        // Arrange - First upload a file to ensure there's something to download
        await UploadTestFile(2); // This will ensure an attachment with ID 1 exists
        
        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/attachments/2");
        request.Headers.Add("X-Workspace-Id", "1");
        request.Headers.Add("Test-UserId", "1");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content.Headers.ContentDisposition);
        Assert.NotEmpty(await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task DeleteAttachment_ReturnsNoContent_WhenAuthorized()
    {
        // Arrange - First upload a file to ensure there's something to delete
        await UploadTestFile(1); // This will ensure an attachment with ID 2 exists
        
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/attachments/1");
        request.Headers.Add("X-Workspace-Id", "1");
        request.Headers.Add("Test-UserId", "1");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
    
    private async Task UploadTestFile(long attachmentId)
    {
        var fileName = $"test-file-{attachmentId}.txt";
        var fileContent = $"This is test file {attachmentId}";
        var bytes = Encoding.UTF8.GetBytes(fileContent);

        var multipartContent = new MultipartFormDataContent();
        var fileContentContent = new ByteArrayContent(bytes);
        fileContentContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
        multipartContent.Add(fileContentContent, "file", fileName);

        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/attachments?taskId=1")
        {
            Content = multipartContent
        };
        request.Headers.Add("X-Workspace-Id", "1");
        request.Headers.Add("Test-UserId", "1");

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
    
    [Fact]
    public async Task ForgotPassword_ReturnsOk_WhenEmailExists()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/forgot-password?email=test1@example.com");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ForgotPassword_ReturnsBadRequest_WhenEmailDoesNotExist()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/forgot-password?email=nonexistent@example.com");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<ErrorResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        Assert.NotNull(error);
        Assert.Equal("auth.errors.userDoesntExist", error.Message);
    }
    private class ErrorResponse
    {
        public string Message { get; set; }
    }
}