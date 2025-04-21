using Azure;
using Azure.Core;
using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
using Fragmenta.Dal;
using Fragmenta.Dal.Models;
using Microsoft.EntityFrameworkCore;
using Role = Fragmenta.Api.Enums.Role;

namespace Fragmenta.Api.Services
{
    public class BoardService : IBoardService
    {
        private readonly ILogger<BoardService> _logger;
        private readonly ApplicationContext _context;

        public BoardService(ILogger<BoardService> logger, ApplicationContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<BoardDto?> CreateBoardAsync(long workspaceId, CreateBoardRequest request)
        {
            var workspace = await _context.Workspaces.FindAsync(workspaceId);

            if (workspace == null)
            {
                return null;
            }

            var attachmentTypes = await _context.AttachmentTypes.Where(e => request.AllowedTypeIds.Contains(e.Id))
                .ToListAsync();

            var board = new Board()
            {
                Name = request.Name,
                Workspace = workspace,
                AttachmentTypes = attachmentTypes,
            };

            await _context.Boards.AddAsync(board);
            await _context.SaveChangesAsync();

            return new BoardDto() { Id = board.Id, Name = board.Name, ArchivedAt = null, AllowedTypeIds = board.AttachmentTypes.Select(a => a.Id).ToList() };
        }

        public async Task<List<BoardDto>> GetBoardsAsync(long workspaceId)
        {
            return await _context.Boards
                .Where(e => e.WorkspaceId == workspaceId)
                .Include(e => e.AccessList)
                .Select(e => new BoardDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    ArchivedAt = e.ArchivedAt,
                    AllowedTypeIds = e.AttachmentTypes.Select(a => a.Id).ToList()
                })
                .ToListAsync();
        }

        public async Task<List<BoardDto>> GetGuestBoardsAsync(long workspaceId, long guestId)
        {
            return await _context.Boards
                .Include(e => e.AccessList)
                .Where(e => e.WorkspaceId == workspaceId && e.AccessList.Any(a => a.UserId == guestId))
                .Select(e => new BoardDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    ArchivedAt = e.ArchivedAt,
                    AllowedTypeIds = e.AttachmentTypes.Select(a => a.Id).ToList()
                })
                .ToListAsync();
        }

        public async Task<BoardDto?> UpdateBoardAsync(long boardId, UpdateBoardRequest request)
        {
            var board = await _context.Boards
                .Include(e => e.AccessList)
                .Include(e => e.AttachmentTypes) // Include existing types
                .SingleOrDefaultAsync(e => e.Id == boardId);

            if (board == null)
            {
                return null;
            }
            
            var uniqueTypeIds = request.AllowedTypeIds.Distinct().ToList();
            
            board.AttachmentTypes.Clear();
            
            var attachmentTypes = await _context.AttachmentTypes
                .Where(e => uniqueTypeIds.Contains(e.Id))
                .ToListAsync();
    
            foreach (var type in attachmentTypes)
            {
                board.AttachmentTypes.Add(type);
            }

            board.Name = request.Name;
            board.ArchivedAt = request.ArchivedAt;

            await _context.SaveChangesAsync();

            return new BoardDto()
            {
                Id = board.Id,
                Name = board.Name,
                ArchivedAt = board.ArchivedAt,
                AllowedTypeIds = board.AttachmentTypes.Select(a => a.Id).ToList()
            };
        }

        public async Task<bool> DeleteBoardAsync(long boardId)
        {
            var board = await _context.Boards.FindAsync(boardId);

            if (board == null || !board.ArchivedAt.HasValue )
            {
                return false;
            }
            
            _context.Remove(board);
            await _context.SaveChangesAsync();

            return true;
        }
        
        public async Task<FullBoardDto?> GetBoardAsync(long boardId)
        {
            var board = await _context.Boards.FindAsync(boardId);

            if(board == null)
            {
                return null;
            }

            var allowedTypes = await _context.AttachmentTypes.Include(e => e.Boards)
                .Where(e => e.Boards.Any(b => b.Id == boardId)).Select(a => a.Id).ToListAsync();

            return new FullBoardDto()
            {
                Id = board.Id,
                Name = board.Name,
                Statuses =
                    _context.Statuses
                        .Where(e => e.BoardId == boardId)
                        .Select(e => new StatusDto()
                        {
                            Name = e.Name,
                            ColorHex = e.ColorHex,
                            Id = e.Id,
                            MaxTasks = e.TaskLimit > 0 ? e.TaskLimit : null,
                            Weight = e.Weight
                        })
                        .ToList(),
                AllowedTypeIds = allowedTypes
            };
        }
    }
}