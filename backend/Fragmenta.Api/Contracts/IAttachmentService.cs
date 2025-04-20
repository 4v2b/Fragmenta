using Fragmenta.Api.Dtos;
using Fragmenta.Dal.Models;

namespace Fragmenta.Api.Contracts
{
    public interface IAttachmentService
    {
        Task<List<AttachmentTypeDto>> GetAllTypes();
        
        Task<bool> IsFileExtensionAllowed(long boardId, string filename);

        Task<Attachment> UploadAttachment(IFormFile file, long taskId);

        Task<Stream> DownloadAttachment(long attachmentId);

        Task<List<AttachmentDto>> GetAttachmentPreviews(long taskId);
    }
}
