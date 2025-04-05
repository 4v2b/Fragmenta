using Fragmenta.Api.Dtos;

namespace Fragmenta.Api.Contracts
{
    public interface IAttachmentTypeService
    {
        Task<List<AttachmentTypeDto>> GetAllTypes();

        Task UpdateBoardAllowedTypes(long boardId, List<long> typeIds);

        Task<List<AttachmentTypeDto>> GetAllowedTypesForBoard(long boardId);

        Task<bool> IsFileExtensionAllowed(long boardId, string filename);
    }
}
