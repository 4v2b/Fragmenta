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

public class AttachmentControllerTests : IClassFixture<TestWebApplicationFactoryContainers>
{
    private readonly HttpClient _client;
    private const string BaseUrl = "/api";
    private readonly BlobServiceClient _blobServiceClient;

    public AttachmentControllerTests(TestWebApplicationFactoryContainers factory)
    {
        _client = factory.CreateClient();
        _blobServiceClient = factory.Services.GetRequiredService<BlobServiceClient>();
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

    // Helper method to upload a test file and ensure it exists in Azurite
    /*private async Task UploadTestFile(long attachmentId)
    {
        // Create a temporary file for testing
        var fileName = $"test-file-{attachmentId}.txt";
        var fileContent = $"This is test file {attachmentId}";
        var bytes = Encoding.UTF8.GetBytes(fileContent);

        // Ensure the container exists in Azurite
        var containerName = "attachments";
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();

        // Upload the blob directly to Azurite
        var blobName = $"{attachmentId}";
        var blobClient = containerClient.GetBlobClient(blobName);
        using var ms = new MemoryStream(bytes);
        await blobClient.UploadAsync(ms, true);
    }*/
    
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
}