using Fragmenta.Api.Dtos;

namespace Fragmenta.Api.Contracts
{
    public interface ITagService
    {
        Task<List<TagDto>> GetTagsAsync(long boardId);

        Task<bool> DeleteTagAsync(long tagId);

        Task<TagDto?> CreateTagAsync(string name, long boardId);
    }
}
