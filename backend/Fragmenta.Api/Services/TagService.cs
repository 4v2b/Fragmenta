using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
using Fragmenta.Dal;
using Fragmenta.Dal.Models;

namespace Fragmenta.Api.Services
{
    public class TagService : ITagService
    {
        private readonly ILogger<TagService> _logger;
        private readonly ApplicationContext _context;

        public TagService(ILogger<TagService> logger, ApplicationContext context)
        {
            _logger = logger;
            _context = context;
        }

        public TagDto? CreateTag(string name, long boardId)
        {
            if(_context.Tags.Any(t => t.Name == name && t.BoardId == boardId))
            {
                return null;
            }

            var entity = new Tag()
            {
                Name = name,
                BoardId = boardId
            };

            _context.Add(entity);
            _context.SaveChanges();


            return new TagDto() { Id = entity.Id, Name = entity.Name };
        }

        public bool DeleteTag(long tagId)
        {
            var tag = _context.Tags.Find(tagId);

            if (tag == null)
            {
                return false;
            }

            _context.Remove(tag);

            return true;
        }

        public List<TagDto> GetTags(long boardId)
        {
            return _context.Tags.Where(e => e.BoardId == boardId).Select(e => new TagDto { Id = e.Id, Name = e.Name}).ToList();
        }
    }
}
