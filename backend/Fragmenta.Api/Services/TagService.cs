using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
using Fragmenta.Dal;
using Fragmenta.Dal.Models;
using Microsoft.EntityFrameworkCore;

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

        public async Task<TagDto?> CreateTagAsync(string name, long boardId)
        {
            if(await _context.Tags.AnyAsync(t => t.Name == name && t.BoardId == boardId))
            {
                return null;
            }

            var entity = new Tag()
            {
                Name = name,
                BoardId = boardId
            };

            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();


            return new TagDto() { Id = entity.Id, Name = entity.Name };
        }

        public async Task<bool> DeleteTagAsync(long tagId)
        {
            var taskTagsToRemove = await _context.TaskTags
                .Where(tt => tt.TagId == tagId)
                .ToListAsync();

            _context.TaskTags.RemoveRange(taskTagsToRemove);
            await _context.SaveChangesAsync();
            
            var tag = await _context.Tags.FindAsync(tagId);

            if (tag == null)
            {
                return false;
            }

            _context.Remove(tag);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<TagDto>> GetTagsAsync(long boardId)
        {
            return await _context.Tags.Where(e => e.BoardId == boardId).Select(e => new TagDto { Id = e.Id, Name = e.Name}).ToListAsync();
        }
    }
}
