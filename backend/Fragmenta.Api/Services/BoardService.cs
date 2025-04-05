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

            var attachmentTypes = _context.AttachmentTypes.Where(e => request.AllowedTypeIds.Exists(i => i == e.Id)).ToList();

            var board = new Board()
            {
                Name = request.Name,
                Workspace = workspace,
                AttachmentTypes = attachmentTypes,
            };

            _context.Boards.Add(board);
            _context.SaveChanges();

            return new BoardDto() { Id = board.Id, Name = board.Name, ArchivedAt = null };
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
                        ArchivedAt = e.ArchivedAt
                    })
                    .ToList();
        }

        public List<BoardDto> GetGuestBoards(long workspaceId, long guestId)
        {
            return _context.Boards
                    .Include(e => e.AccessList)
                    .Where(e => e.WorkspaceId == workspaceId && e.AccessList.Any(e => e.UserId == guestId))
                    .Select(e => new BoardDto
                    {
                        Id = e.Id,
                        Name = e.Name,
                        ArchivedAt = e.ArchivedAt
                    })
                    .ToList();
        }

        public BoardDto? UpdateBoard(long boardId, UpdateBoardRequest request)
        {
            var board = _context.Boards.Include(e => e.AccessList).Single(e => e.Id == boardId);

            var attachmentTypes = _context.AttachmentTypes.Where(e => request.AllowedTypeIds.Exists(i => i == e.Id)).ToList();

            if (board == null)
            {
                return null;
            }

            board.Name = request.Name;
            board.ArchivedAt = request.ArchivedAt;

            board.AttachmentTypes = attachmentTypes;

            _context.SaveChanges();

            return new BoardDto() { Id = board.Id, Name = board.Name, ArchivedAt = board.ArchivedAt };
        }

        public bool RemoveGuest(long boardId, long guestId)
        {
            var board = _context.Boards.Include(e => e.AccessList).Single(e => e.Id == boardId);

            var access = _context.BoardAccesses.Find(boardId, guestId);

            if (board == null || access == null)
            {
                return false;
            }

            _context.Remove(access);
            _context.SaveChanges();

            var existsInWorkspace = _context.BoardAccesses
                .Include(e => e.Board)
                .Any(e => e.Board.WorkspaceId == board.WorkspaceId && e.UserId == guestId && e.BoardId != board.Id);

            if (!existsInWorkspace)
            {
                var userToRemove = _context.WorkspaceAccesses
                    .Single(e =>
                        e.RoleId == (long)Role.Guest &&
                        e.WorkspaceId == board.WorkspaceId && e.UserId == guestId);

                if (userToRemove != null)
                {
                    _context.WorkspaceAccesses.Remove(userToRemove);
                }
            }

            _context.SaveChanges();

            return true;
        }

        public List<MemberDto> AddGuests(long boardId, long[] usersId)
        {
            var board = _context.Boards.Include(e => e.AccessList).Single(e => e.Id == boardId);

            if (board == null)
            {
                return [];
            }

            List<WorkspaceAccess> workspaceAccesses = new();

            foreach (long id in usersId)
            {
                var workspaceAccess = _context.WorkspaceAccesses.Find(board.WorkspaceId, id);
                if (workspaceAccess == null)
                {
                    workspaceAccesses.Add(new WorkspaceAccess()
                    {
                        JoinedAt = DateTime.UtcNow,
                        RoleId = (long)Role.Guest,
                        UserId = id,
                        WorkspaceId = board.WorkspaceId
                    });
                }
            }

            var accesses = usersId.Select(id => new BoardAccess()
            {
                BoardId = board.Id,
                UserId = id
            });

            _context.WorkspaceAccesses.AddRange(workspaceAccesses);
            _context.BoardAccesses.AddRange(accesses);

            _context.SaveChanges();

            return _context.BoardAccesses
                .Where(e => e.BoardId == board.Id)
                .Include(e => e.User)
                .Select(e => new MemberDto
                    {
                        Email = e.User.Email,
                        Id = e.UserId,
                        Name = e.User.Name,
                        Role = Enum.GetName(Role.Guest)!
                    })
                .ToList();
        }
    }
}
