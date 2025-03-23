using Fragmenta.Api.Dtos;

namespace Fragmenta.Api.Contracts
{
    public interface ITagService
    {
        List<TagDto> GetTags(long boardId);

        bool DeleteTag(long tagId);

        TagDto? CreateTag(string name, long boardId);
    }
}
