using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
using Fragmenta.Dal;
using Microsoft.EntityFrameworkCore;
using System;

namespace Fragmenta.Api.Services
{
    public class AttachmentTypeService : IAttachmentTypeService
    {
        private readonly ApplicationContext _context;

        public AttachmentTypeService(ApplicationContext context)
        {
            _context = context;
        }

        public Task<List<AttachmentTypeDto>> GetAllTypes()
        {
            throw new NotImplementedException();
        }

        public Task UpdateBoardAllowedTypes(long boardId, List<long> typeIds)
    {
            throw new NotImplementedException();
}

public Task<List<AttachmentTypeDto>> GetAllowedTypesForBoard(long boardId)
        {
            throw new NotImplementedException();
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
}
