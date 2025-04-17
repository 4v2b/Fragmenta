using Fragmenta.Api.Dtos;

namespace Fragmenta.Api.Contracts
{
    public interface IAttachmentService
    {
        Task<List<AttachmentTypeDto>> GetAllTypes();
        
        Task<bool> IsFileExtensionAllowed(long boardId, string filename);
    }
}
