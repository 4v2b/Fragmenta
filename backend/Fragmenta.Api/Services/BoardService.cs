using Azure;
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

            var board = new Board()
            {
                Name = request.Name,
                Workspace = workspace
            };

            _context.Boards.Add(board);
            _context.SaveChanges();

            List<WorkspaceAccess> workspaceAccesses = new();

            foreach (long id in request.GuestsId)
            {
                var workspaceAccess = _context.WorkspaceAccesses.Find(workspaceId, id);
                if (workspaceAccess == null)
                {
                    workspaceAccesses.Add(new WorkspaceAccess()
                    {
                        JoinedAt = DateTime.UtcNow,
                        RoleId = (long)Role.Guest,
                        UserId = id,
                        WorkspaceId = workspaceId
                    });
                }
            }

            var accesses = request.GuestsId.Select(id => new BoardAccess()
            {
                BoardId = board.Id,
                UserId = id
            });

            _context.WorkspaceAccesses.AddRange(workspaceAccesses);

            board.AccessList.AddRange(accesses);

            _context.SaveChanges();

            return new BoardDto() { Id = board.Id, Name = board.Name, GuestsId = request.GuestsId.ToList() };
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
                        GuestsId = e.AccessList.Select(a => a.UserId).ToList()
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
                        ArchivedAt = e.ArchivedAt,
                        GuestsId = e.AccessList.Select(a => a.UserId).ToList()
                    })
                    .ToList();
        }

        public BoardDto? UpdateBoard(long boardId, CreateBoardRequest request)
        {
            throw new NotImplementedException();
        }

        public List<UserDto> UpdateGuestList(long[] usersId)
        {
            throw new NotImplementedException();
        }
    }
}
