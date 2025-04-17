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
using Fragmenta.Api.Dtos;

public class AttachmentService : IAttachmentService
{
    private readonly ApplicationContext _context;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly AzureStorageOptions _options;

    public AttachmentService(ApplicationContext context, IOptions<AzureStorageOptions> blobStorageConfig)
    {
        _options = blobStorageConfig.Value ?? throw new ArgumentNullException(nameof(blobStorageConfig)); 

        _context = context;
        _blobServiceClient = new BlobServiceClient(_options.ConnectionString);
    }

    public async Task<Attachment> UploadAttachment(IFormFile file, long taskId, long userId)
    {
        var task = await _context.Tasks.FindAsync(taskId);
        if (task == null) throw new InvalidOperationException("Task not found");

        var board = await _context.Boards.Include(e => e.Statuses)
                            .FirstOrDefaultAsync(e => e.Statuses.Any(i => i.Id == task.StatusId));
        if (board == null) throw new InvalidOperationException("Board not found");

        bool isAllowed = await IsFileExtensionAllowed(board.Id, file.FileName);
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

    public async Task<List<AttachmentTypeDto>> GetAllTypes()
    {
        var allTypes = await _context.AttachmentTypes.ToListAsync();

        var dtoMap = allTypes.ToDictionary(
            x => x.Id,
            x => new AttachmentTypeDto
            {
                Id = x.Id,
                Value = x.Value,
                Children = []
            });

        foreach (var entity in allTypes)
        {
            if (entity.ParentId is not null)
            {
                dtoMap[entity.ParentId.Value].Children.Add(dtoMap[entity.Id]);
            }
        }

        return dtoMap.Values
            .Where(dto => allTypes.First(e => e.Id == dto.Id).ParentId is null)
            .ToList();
    }

    public async Task<bool> IsFileExtensionAllowed(long boardId, string filename)
    {
        var extension = Path.GetExtension(filename).ToLowerInvariant();

        var allowedTypeIds = _context.AttachmentTypes.Include(e => e.Boards).Where(e => e.Boards.Any(i => i.Id == boardId)).Select(e => e.Id);

        var allowedTypes = await _context.AttachmentTypes
            .Include(e => e.Children)
            .Where(t => allowedTypeIds.Contains(t.Id) ||
                        t.Children.Any(c => allowedTypeIds.Contains(c.Id)))
            .ToListAsync();

        return allowedTypes.Any(t => t.Value.Equals(extension, StringComparison.OrdinalIgnoreCase));
    }
}
