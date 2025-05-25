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

namespace Fragmenta.Api.Services;
public class AttachmentService : IAttachmentService
{
    private readonly ApplicationContext _context;
    private readonly IBlobClientFactory _blobClientFactory;

    public AttachmentService(ApplicationContext context,  IBlobClientFactory blobClientFactory)
    {
        _context = context;
        _blobClientFactory = blobClientFactory;
    }


    public async Task<List<AttachmentDto>> GetAttachmentPreviewsAsync(long taskId)
    {
        return await _context.Attachments.Where(a => a.TaskId == taskId).Select(a => new AttachmentDto()
        {
            CreatedAt = a.CreatedAt, Id = a.Id, FileName = a.FileName, OriginalName = a.OriginalName,
            SizeBytes = a.SizeBytes
        }).ToListAsync();
    }
    
    public async Task<AttachmentDto?> GetAttachmentPreviewAsync(long attachmentId)
    {
        var entity = await _context.Attachments.FindAsync(attachmentId);

        return entity is null
            ? null
            : new AttachmentDto()
            {
                CreatedAt = entity.CreatedAt, Id = entity.Id, FileName = entity.FileName, OriginalName = entity.OriginalName,
                SizeBytes = entity.SizeBytes
            };
    }

    public async Task<bool> DeleteAttachmentAsync(long attachmentId)
    {
        var attachment = await _context.Attachments.FindAsync(attachmentId);
        if (attachment == null)
            return false;

        try
        {
            var blobClient = await _blobClientFactory.GetBlobClientAsync(attachment.FileName);
            await blobClient.DeleteAsync();

            _context.Attachments.Remove(attachment);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<AttachmentDto> UploadAttachmentAsync(IFormFile file, long taskId)
    {
        var task = await _context.Tasks.FindAsync(taskId);
        if (task == null) throw new InvalidOperationException("Task not found");

        var board = await _context.Boards
            .Include(b => b.Statuses)
            .FirstOrDefaultAsync(b => b.Statuses.Any(s => s.Id == task.StatusId));
        if (board == null) throw new InvalidOperationException("Board not found");

        if (!await IsFileExtensionAllowedAsync(board.Id, file.FileName))
            throw new InvalidOperationException("File extension is not allowed");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileType = await _context.AttachmentTypes
            .FirstOrDefaultAsync(t => t.Value.Equals(extension));
        if (fileType == null)
            throw new InvalidOperationException("Unknown file type");

        var blobName = $"{Guid.NewGuid()}{extension}";
        var blobClient =await _blobClientFactory.GetBlobClientAsync(blobName);

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
            throw new InvalidOperationException("Error uploading file to blob storage", ex);
        }

        var attachment = new Attachment
        {
            FileName = blobName,
            OriginalName = file.FileName,
            TypeId = fileType.Id,
            TaskId = taskId,
            SizeBytes = file.Length
        };

        _context.Attachments.Add(attachment);
        await _context.SaveChangesAsync();

        return new AttachmentDto
        {
            Id = attachment.Id,
            FileName = attachment.FileName,
            OriginalName = attachment.OriginalName,
            SizeBytes = attachment.SizeBytes,
            CreatedAt = attachment.CreatedAt
        };
    }

    public async Task<Stream> DownloadAttachmentAsync(long attachmentId)
    {
        var attachment = await _context.Attachments.FindAsync(attachmentId);
        if (attachment == null)
            throw new InvalidOperationException("Attachment not found");

        var blobClient = await _blobClientFactory.GetBlobClientAsync(attachment.FileName);

        try
        {
            var download = await blobClient.DownloadAsync();
            return download.Value.Content;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error downloading file from blob storage", ex);
        }
    }

    public async Task<List<AttachmentTypeDto>> GetAllTypesAsync()
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

    public async Task<bool> IsFileExtensionAllowedAsync(long boardId, string filename)
    {
        var extension = Path.GetExtension(filename).ToLowerInvariant();

        var allowedTypeIds = _context.AttachmentTypes.Include(e => e.Boards)
            .Where(e => e.Boards.Any(i => i.Id == boardId)).Select(e => e.Id);

        var allowedTypes = await _context.AttachmentTypes
            .Include(e => e.Children)
            .Where(t => allowedTypeIds.Contains(t.Id) ||
                        t.Children.Any(c => allowedTypeIds.Contains(c.Id)))
            .ToListAsync();

        return allowedTypes.Any(t => t.Value.Equals(extension, StringComparison.OrdinalIgnoreCase));
    }
}
