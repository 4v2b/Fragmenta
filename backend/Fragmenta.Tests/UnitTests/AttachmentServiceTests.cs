using System.Text;
using Azure;
using Azure.Storage.Blobs;
using Fragmenta.Api.Configuration;
using Fragmenta.Dal;
using Fragmenta.Dal.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Task = System.Threading.Tasks.Task;
using Azure.Storage.Blobs.Models;
using Fragmenta.Api.Contracts;
using Fragmenta.Api.Services;

namespace Fragmenta.Tests.UnitTests;

public class AttachmentServiceTests : UnitTestsBase
{
 private AttachmentService CreateService(ApplicationContext context, Mock<IBlobClientFactory>? blobFactoryMock = null)
    {
        var options = Options.Create(new AzureStorageOptions { ContainerName = "test-container" });
        return new AttachmentService(context, options, blobFactoryMock?.Object ?? Mock.Of<IBlobClientFactory>());
    }

    [Fact]
    public async Task DownloadAttachmentAsync_Throws_WhenBlobFails()
    {
        var context = CreateInMemoryContext();
        context.Attachments.Add(new Attachment { Id = 1, FileName = "fail.png", OriginalName = "fail.png"});
        await context.SaveChangesAsync();

        var blobClientMock = new Mock<BlobClient>();
        blobClientMock.Setup(b => b.DownloadAsync(default))
                      .ThrowsAsync(new Exception("Download failed"));

        var blobFactoryMock = new Mock<IBlobClientFactory>();
        blobFactoryMock.Setup(f => f.GetBlobClientAsync("fail.png")).ReturnsAsync(blobClientMock.Object);

        var service = CreateService(context, blobFactoryMock);
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DownloadAttachmentAsync(1));

        Assert.Equal("Error downloading file from blob storage", ex.Message);
    }

    [Fact]
    public async Task UploadAttachmentAsync_UploadsToBlobAndSavesInDb()
    {
        var context = CreateInMemoryContext();
        context.Tasks.Add(new Dal.Models.Task { Id = 1, StatusId = 1 ,  Title = "Task",});
        context.Boards.Add(new Board { Id = 1, Name = "Test Board", AttachmentTypes = [new AttachmentType{ Id = 999, Value = ".txt"}], Statuses = [new Status { Id = 1, Name = "Status 1",  ColorHex = "#FFFFFF", TaskLimit = 0 }] });
        await context.SaveChangesAsync();

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("document.txt");
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream("test content"u8.ToArray()));
        fileMock.Setup(f => f.Length).Returns(12);
        fileMock.Setup(f => f.ContentType).Returns("text/plain");

        
        
        var blobClientMock = new Mock<BlobClient>();
        var mockResponse = new Mock<Response<BlobContentInfo>>();
        blobClientMock
            .Setup(b => b.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), default))
            .ReturnsAsync(mockResponse.Object);

        var blobFactoryMock = new Mock<IBlobClientFactory>();
        blobFactoryMock.Setup(f => f.GetBlobClientAsync(It.IsAny<string>()))
                       .ReturnsAsync(blobClientMock.Object);
        
        var service = CreateService(context, blobFactoryMock);
        var result = await service.UploadAttachmentAsync(fileMock.Object, 1);

        Assert.NotNull(result);
        Assert.Equal(1, context.Attachments.Count());
    }

    [Fact]
    public async Task GetAttachmentPreviewsAsync_ReturnsList()
    {
        var context = CreateInMemoryContext();
        context.Attachments.Add(new Attachment
        {
            Id = 1, TaskId = 5, FileName = "a.txt", OriginalName = "file.txt", SizeBytes = 100, CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var previews = await service.GetAttachmentPreviewsAsync(5);

        Assert.Single(previews);
        Assert.Equal("file.txt", previews[0].OriginalName);
    }

    [Fact]
    public async Task GetAllTypesAsync_ReturnsHierarchy()
    {
        var context = CreateInMemoryContext();
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var result = await service.GetAllTypesAsync();

        Assert.Single(result);
        Assert.True(result[0].Children.Count > 1);
    }

    [Fact]
    public async Task IsFileExtensionAllowedAsync_ReturnsTrue_IfAllowed()
    {
        var context = CreateInMemoryContext();
        context.Boards.Add(new Board { Id = 1, Name = "Test Board", AttachmentTypes = [new AttachmentType{ Id = 999, Value = ".jpg"}] });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var result = await service.IsFileExtensionAllowedAsync(1, "image.jpg");

        Assert.True(result);
    }

    [Fact]
    public async Task IsFileExtensionAllowedAsync_ReturnsFalse_IfNotAllowed()
    {
        var context = CreateInMemoryContext();
        var service = CreateService(context);
        var result = await service.IsFileExtensionAllowedAsync(1, "image.png");

        Assert.False(result);
    }

    // Додаткові тести можуть бути написані на:
    // - Unknown file type
    // - Disallowed extension
    // - Blob upload failure (мокається BlobClient)
}