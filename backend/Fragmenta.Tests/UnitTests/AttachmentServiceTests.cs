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

namespace Fragmenta.Tests.UnitTests;

public class AttachmentServiceTests : UnitTestsBase
{
    private AttachmentService CreateService(ApplicationContext context, Mock<BlobServiceClient> blobClientMock)
    {
        var options = Options.Create(new AzureStorageOptions
        {
            ContainerName = "test-container"
        });

        return new AttachmentService(context, options, blobClientMock.Object);
    }

    [Fact]
    public async Task GetAttachmentPreviewsAsync_ReturnsCorrectPreviews()
    {
        var context = CreateInMemoryContext();
        var mockBlob = new Mock<BlobServiceClient>();
        var service = CreateService(context, mockBlob);

        context.Attachments.Add(new Attachment
        {
            Id = 1, TaskId = 10, FileName = "file", OriginalName = "file", SizeBytes = 123, CreatedAt = DateTime.UtcNow
        });
        context.SaveChanges();

        var result = await service.GetAttachmentPreviewsAsync(10);

        Assert.Single(result);
        Assert.Equal("file", result[0].FileName);
    }

    [Fact]
    public async Task IsFileExtensionAllowedAsync_ReturnsTrue_WhenAllowed()
    {
        var context = CreateInMemoryContext();
        var mockBlob = new Mock<BlobServiceClient>();
        var service = CreateService(context, mockBlob);

        var type = new AttachmentType { Id = 1111, Value = ".png" };
        var board = new Board { Id = 99, Name = "Board", AttachmentTypes = new List<AttachmentType> { type } };
        type.Boards = new List<Board> { board };

        context.AttachmentTypes.Add(type);
        context.Boards.Add(board);
        await context.SaveChangesAsync();

        var result = await service.IsFileExtensionAllowedAsync(99, "photo.PNG");

        Assert.True(result);
    }

    [Fact]
    public async Task UploadAttachmentAsync_Throws_WhenTaskNotFound()
    {
        var context = CreateInMemoryContext();
        var mockBlob = new Mock<BlobServiceClient>();
        var service = CreateService(context, mockBlob);

        var formFileMock = new Mock<IFormFile>();
        formFileMock.Setup(f => f.FileName).Returns("file.png");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UploadAttachmentAsync(formFileMock.Object, 404));
        Assert.Equal("Task not found", ex.Message);
    }

    [Fact]
    public async Task UploadAttachmentAsync_UploadsSuccessfully()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var extension = ".png";
        var task = new Dal.Models.Task { Id = 1, StatusId = 1, Title = "Task"};
        var board = new Board
        {
            Id = 99,
            Name = "Board",
            Statuses = [new Status { Id = 1, Name = "Status", ColorHex = "#FFFFFF"}],
            AttachmentTypes = []
        };
        var type = new AttachmentType { Id = 10, Value = extension };
        type.Boards = [board];

        context.Tasks.Add(task);
        context.Boards.Add(board);
        context.AttachmentTypes.Add(type);
        await context.SaveChangesAsync();

        var formFileMock = new Mock<IFormFile>();
        formFileMock.Setup(f => f.FileName).Returns("test.png");
        formFileMock.Setup(f => f.Length).Returns(100);
        formFileMock.Setup(f => f.ContentType).Returns("image/png");
        formFileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream([1, 2, 3]));

        var blobClientMock = new Mock<BlobClient>();
        blobClientMock.Setup(b => b.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), default))
            .Returns(Task.FromResult((Response<BlobContentInfo>)null!));

        var containerClientMock = new Mock<BlobContainerClient>();
        containerClientMock.Setup(c => c.CreateIfNotExistsAsync(PublicAccessType.None, null, CancellationToken.None))
            .Returns(Task.FromResult((Response<BlobContainerInfo>)null!));
        containerClientMock.Setup(c => c.GetBlobClient(It.IsAny<string>())).Returns(blobClientMock.Object);

        var blobServiceMock = new Mock<BlobServiceClient>();
        blobServiceMock.Setup(b => b.GetBlobContainerClient(It.IsAny<string>())).Returns(containerClientMock.Object);

        var service = CreateService(context, blobServiceMock);

        // Act
        var result = await service.UploadAttachmentAsync(formFileMock.Object, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test.png", result.OriginalName);
        Assert.Equal(100, result.SizeBytes);
    }

    [Fact]
    public async Task DownloadAttachmentAsync_ReturnsStream_WhenAttachmentExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        context.Attachments.Add(new Attachment { Id = 1, FileName = "blob.png", OriginalName = "file.png"});
        await context.SaveChangesAsync();

        var stream = new MemoryStream([9, 8, 7]);
        var downloadInfoMock = BlobsModelFactory.BlobDownloadInfo(content: stream);

        var blobClientMock = new Mock<BlobClient>();
        blobClientMock.Setup(b => b.DownloadAsync(default))
            .ReturnsAsync(Response.FromValue(downloadInfoMock, null!));

        var containerClientMock = new Mock<BlobContainerClient>();
        containerClientMock.Setup(c => c.CreateIfNotExistsAsync(PublicAccessType.None, null, CancellationToken.None))
            .Returns(Task.FromResult((Response<BlobContainerInfo>)null!));
        containerClientMock.Setup(c => c.GetBlobClient(It.IsAny<string>())).Returns(blobClientMock.Object);

        var blobServiceMock = new Mock<BlobServiceClient>();
        blobServiceMock.Setup(b => b.GetBlobContainerClient(It.IsAny<string>())).Returns(containerClientMock.Object);

        var service = CreateService(context, blobServiceMock);

        // Act
        var result = await service.DownloadAttachmentAsync(1);

        // Assert
        using var reader = new StreamReader(result);
        var buffer = new byte[3];
        await result.ReadAsync(buffer);
        Assert.Equal([9, 8, 7], buffer);
    }

    [Fact]
    public async Task DownloadAttachmentAsync_Throws_WhenBlobFails()
    {
        var context = CreateInMemoryContext();
        context.Attachments.Add(new Attachment { Id = 2, FileName = "broken.png", OriginalName = "file.png"});
        await context.SaveChangesAsync();

        var blobClientMock = new Mock<BlobClient>();
        blobClientMock.Setup(b => b.DownloadAsync(default))
            .ThrowsAsync(new Exception("Blob not found"));

        var containerClientMock = new Mock<BlobContainerClient>();
        containerClientMock.Setup(c => c.CreateIfNotExistsAsync(PublicAccessType.None, null, CancellationToken.None))
            .Returns(Task.FromResult((Response<BlobContainerInfo>)null!));
        containerClientMock.Setup(c => c.GetBlobClient(It.IsAny<string>())).Returns(blobClientMock.Object);

        var blobServiceMock = new Mock<BlobServiceClient>();
        blobServiceMock.Setup(b => b.GetBlobContainerClient(It.IsAny<string>())).Returns(containerClientMock.Object);

        var service = CreateService(context, blobServiceMock);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DownloadAttachmentAsync(2));
        Assert.Equal("Error downloading file from blob storage", ex.Message);
    }

    [Fact]
    public async Task GetAllTypesAsync_ReturnsCorrectHierarchy()
    {
        var context = CreateInMemoryContext();

        var parent = new AttachmentType { Id = 1001, Value = ".doc", ParentId = 1 };
        var child = new AttachmentType { Id = 1002, Value = ".docx", ParentId = 1 };

        context.AttachmentTypes.AddRange(parent, child);
        await context.SaveChangesAsync();

        var blobMock = new Mock<BlobServiceClient>();
        var service = CreateService(context, blobMock);

        var result = await service.GetAllTypesAsync();

        Assert.Single(result);
        Assert.NotEmpty(result.First().Children);
    }

    // Додаткові тести можуть бути написані на:
    // - Unknown file type
    // - Disallowed extension
    // - Blob upload failure (мокається BlobClient)
}