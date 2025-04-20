using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
using Fragmenta.Dal;
using Fragmenta.Dal.Models;
using Microsoft.EntityFrameworkCore;
using Role = Fragmenta.Api.Enums.Role;

namespace Fragmenta.Api.Controllers;

public class BoardAccessService : IBoardAccessService
{
    private readonly ILogger<BoardAccessService> _logger;
    private readonly ApplicationContext _context;

    public BoardAccessService(ILogger<BoardAccessService> logger, ApplicationContext context)
    {
        _logger = logger;
        _context = context;
    }

    public bool RemoveGuest(long boardId, long guestId)
    {
        var board = _context.Boards.Include(e => e.AccessList).SingleOrDefault(e => e.Id == boardId);

        var access = _context.BoardAccesses.Find(boardId, guestId);

        if (board == null || access == null)
        {
            _logger.LogInformation("User with id {Id} is not a guest on board {BoardId}", guestId, boardId);
            return false;
        }

        _context.Remove(access);
        _context.SaveChanges();

        var isStillGuest = _context.BoardAccesses
            .Include(e => e.Board)
            .Any(e => e.Board.WorkspaceId == board.WorkspaceId && e.UserId == guestId && e.BoardId != board.Id);

        if (!isStillGuest)
        {
            _logger.LogInformation("User with id {Id} is no longer a guest in workspace {WorkspaceId}", guestId,
                board.WorkspaceId);
            var userToRemove = _context.WorkspaceAccesses
                .SingleOrDefault(e =>
                    e.RoleId == (long)Role.Guest &&
                    e.WorkspaceId == board.WorkspaceId && e.UserId == guestId);

            if (userToRemove != null)
            {
                _logger.LogInformation("User with id {Id} is being deleted from workspace {WorkspaceId}", guestId,
                    board.WorkspaceId);
                _context.WorkspaceAccesses.Remove(userToRemove);
            }
        }

        _context.SaveChanges();

        return true;
    }

    public List<MemberDto> AddGuests(long boardId, long[] usersId)
    {
        var board = _context.Boards.Include(e => e.AccessList).SingleOrDefault(e => e.Id == boardId);

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

                _logger.LogInformation("User with id {Id} joined workspace {WorkspaceId}", id, board.WorkspaceId);
            }
            else
            {
                _logger.LogInformation("User with id {Id} is already in workspace {WorkspaceId}", id,
                    board.WorkspaceId);
            }
        }

        var accesses = usersId.Select(id => new BoardAccess()
        {
            BoardId = board.Id,
            UserId = id
        });

        _logger.LogInformation("Users with id {Ids} joined board {WorkspaceId}", string.Join(' ', usersId), board.Id);

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

    public List<GuestDto> GetGuests(long boardId)
    {
        return _context.Users.Include(u => u.Boards).Where(e => e.Boards.Any(b => b.Id == boardId))
            .Select(u => new GuestDto { Email = u.Email, Id = u.Id, Name = u.Name }).ToList();
    }

    public bool CanViewBoard(long boardId, long userId)
    {
        return _context.BoardAccesses.Find(boardId, userId) is not null;
    }
}