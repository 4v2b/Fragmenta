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

    public async Task<bool>  RemoveGuestAsync(long boardId, long guestId)
    {
        var board = await _context.Boards.Include(e => e.AccessList).SingleOrDefaultAsync(e => e.Id == boardId);

        var access = await _context.BoardAccesses.FindAsync(boardId, guestId);

        if (board == null || access == null)
        {
            _logger.LogInformation("User with id {Id} is not a guest on board {BoardId}", guestId, boardId);
            return false;
        }

        _context.Remove(access);
        await _context.SaveChangesAsync();

        var isStillGuest = await _context.BoardAccesses
            .Include(e => e.Board)
            .AnyAsync(e => e.Board.WorkspaceId == board.WorkspaceId && e.UserId == guestId && e.BoardId != board.Id);

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

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<MemberDto>> AddGuestsAsync(long boardId, long[] usersId)
    {
        var board = await _context.Boards.Include(e => e.AccessList).SingleOrDefaultAsync(e => e.Id == boardId);

        if (board == null)
        {
            return [];
        }

        List<WorkspaceAccess> workspaceAccesses = new();

        foreach (long id in usersId)
        {
            var workspaceAccess = await _context.WorkspaceAccesses.FindAsync(board.WorkspaceId, id);
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

        await _context.WorkspaceAccesses.AddRangeAsync(workspaceAccesses);
        await _context.BoardAccesses.AddRangeAsync(accesses);

        await _context.SaveChangesAsync();

        return await _context.BoardAccesses
            .Where(e => e.BoardId == board.Id)
            .Include(e => e.User)
            .Select(e => new MemberDto
            {
                Email = e.User.Email,
                Id = e.UserId,
                Name = e.User.Name,
                Role = Enum.GetName(Role.Guest)!
            })
            .ToListAsync();
    }

    public async Task<List<GuestDto>> GetGuestsAsync(long boardId)
    {
        return await _context.Users.Include(u => u.Boards).Where(e => e.Boards.Any(b => b.Id == boardId))
            .Select(u => new GuestDto { Email = u.Email, Id = u.Id, Name = u.Name }).ToListAsync();
    }

    public async Task<bool> CanViewBoardAsync(long boardId, long userId)
    {
        return await _context.BoardAccesses.FindAsync(boardId, userId) is not null;
    }
}