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

        public BoardDto? CreateBoard(long workspaceId, CreateBoardRequest request)
        {
            var workspace = _context.Workspaces.Find(workspaceId);

            if (workspace == null)
            {
                return null;
            }

            var attachmentTypes = _context.AttachmentTypes.Where(e => request.AllowedTypeIds.Contains(e.Id))
                .ToList();

            var board = new Board()
            {
                Name = request.Name,
                Workspace = workspace,
                AttachmentTypes = attachmentTypes,
            };

            _context.Boards.Add(board);
            _context.SaveChanges();

            return new BoardDto() { Id = board.Id, Name = board.Name, ArchivedAt = null, AllowedTypeIds = board.AttachmentTypes.Select(a => a.Id).ToList() };
        }

        public List<BoardDto> GetBoards(long workspaceId)
        {
            return _context.Boards
                .Where(e => e.WorkspaceId == workspaceId)
                .Include(e => e.AccessList)
                .Select(e => new BoardDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    ArchivedAt = e.ArchivedAt,
                    AllowedTypeIds = e.AttachmentTypes.Select(a => a.Id).ToList()
                })
                .ToList();
        }

        public List<BoardDto> GetGuestBoards(long workspaceId, long guestId)
        {
            return _context.Boards
                .Include(e => e.AccessList)
                .Where(e => e.WorkspaceId == workspaceId && e.AccessList.Any(a => a.UserId == guestId))
                .Select(e => new BoardDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    ArchivedAt = e.ArchivedAt,
                    AllowedTypeIds = e.AttachmentTypes.Select(a => a.Id).ToList()
                })
                .ToList();
        }

        public BoardDto? UpdateBoard(long boardId, UpdateBoardRequest request)
        {
            var board = _context.Boards
                .Include(e => e.AccessList)
                .Include(e => e.AttachmentTypes) // Include existing types
                .SingleOrDefault(e => e.Id == boardId);

            if (board == null)
            {
                return null;
            }
            
            var uniqueTypeIds = request.AllowedTypeIds.Distinct().ToList();
            
            board.AttachmentTypes.Clear();
            
            var attachmentTypes = _context.AttachmentTypes
                .Where(e => uniqueTypeIds.Contains(e.Id))
                .ToList();
    
            foreach (var type in attachmentTypes)
            {
                board.AttachmentTypes.Add(type);
            }

            board.Name = request.Name;
            board.ArchivedAt = request.ArchivedAt;

            _context.SaveChanges();

            return new BoardDto()
            {
                Id = board.Id,
                Name = board.Name,
                ArchivedAt = board.ArchivedAt,
                AllowedTypeIds = board.AttachmentTypes.Select(a => a.Id).ToList()
            };
        }

        public bool DeleteBoard(long boardId)
        {
            var board = _context.Boards.Find(boardId);

            if (board == null || !board.ArchivedAt.HasValue )
            {
                return false;
            }
            
            _context.Remove(board);
            _context.SaveChanges();

            return true;
        }
        
        public FullBoardDto? GetBoard(long boardId)
        {
            var board = _context.Boards.Find(boardId);

            if(board == null)
            {
                return null;
            }

            var allowedTypes = _context.AttachmentTypes.Include(e => e.Boards)
                .Where(e => e.Boards.Any(b => b.Id == boardId)).Select(a => a.Id).ToList();

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