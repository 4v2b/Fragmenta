using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Fragmenta.Api.Configuration;
using Fragmenta.Api.Contracts;
using Fragmenta.Dal;
using Fragmenta.Dal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;

public class AttachmentService : IAttachmentService
{
    private readonly ApplicationContext _context;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly IAttachmentTypeService _typeService;
    private readonly AzureStorageOptions _options;

    public AttachmentService(ApplicationContext context, IOptions<AzureStorageOptions> blobStorageConfig,
                             IAttachmentTypeService typeService)
    {
        _options = blobStorageConfig.Value ?? throw new ArgumentNullException(nameof(blobStorageConfig)); 

        _context = context;
        _blobServiceClient = new BlobServiceClient(_options.ConnectionString);
        _typeService = typeService;
    }

    public async Task<Attachment> UploadAttachment(IFormFile file, long taskId, long userId)
    {
        var task = await _context.Tasks.FindAsync(taskId);
        if (task == null) throw new InvalidOperationException("Task not found");

        var board = await _context.Boards.Include(e => e.Statuses)
                            .FirstOrDefaultAsync(e => e.Statuses.Any(i => i.Id == task.StatusId));
        if (board == null) throw new InvalidOperationException("Board not found");

        bool isAllowed = await _typeService.IsFileExtensionAllowed(board.Id, file.FileName);
        if (!isAllowed) throw new InvalidOperationException("File extension is not allowed");

        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileType = await _context.AttachmentTypes
            .FirstOrDefaultAsync(t => t.Value.Equals(extension, StringComparison.OrdinalIgnoreCase));
        if (fileType == null) throw new InvalidOperationException("Unknown file type");

        string blobName = $"{Guid.NewGuid()}{extension}";
        var blobClient = GetBlobClient(blobName);

        try
        {
            await using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType }
            });
        }
        catch (Exception ex)
        {
            throw new Exception("Error uploading file to blob storage", ex);
        }

        var attachment = new Attachment
        {
            FileName = blobName,
            OriginalName = file.FileName,
            TypeId = fileType.Id,
            TaskId = taskId,
            UserId = userId
        };

        _context.Attachments.Add(attachment);
        await _context.SaveChangesAsync();

        return attachment;
    }

    public async Task<Stream> DownloadAttachment(long attachmentId)
    {
        var attachment = await _context.Attachments.FindAsync(attachmentId);
        if (attachment == null) throw new InvalidOperationException("Attachment not found");

        var blobClient = GetBlobClient(attachment.FileName);

        try
        {
            var download = await blobClient.DownloadAsync();
            return download.Value.Content;
        }
        catch (Exception ex)
        {
            throw new Exception("Error downloading file from blob storage", ex);
        }
    }

    private BlobClient GetBlobClient(string blobName)
    {
        var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);
        return blobContainerClient.GetBlobClient(blobName);
    }
}
