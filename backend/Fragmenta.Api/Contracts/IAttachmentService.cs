using Fragmenta.Api.Dtos;
using Fragmenta.Dal.Models;

namespace Fragmenta.Api.Contracts
{
    public interface IAttachmentService
    {
        Task<List<AttachmentTypeDto>> GetAllTypesAsync();
        
        Task<bool> IsFileExtensionAllowedAsync(long boardId, string filename);
        
        Task<bool> DeleteAttachmentAsync(long attachmentId);

        Task<AttachmentDto> UploadAttachmentAsync(IFormFile file, long taskId);

        Task<Stream> DownloadAttachmentAsync(long attachmentId);

        Task<List<AttachmentDto>> GetAttachmentPreviewsAsync(long taskId);
    }
}
