using Fragmenta.Api.Dtos;

namespace Fragmenta.Api.Contracts;

public interface IBoardAccessService
{
    Task<bool> CanViewBoardAsync(long boardId, long userId);
    
    Task<bool> RemoveGuestAsync(long boardId, long guestId);

    Task<List<MemberDto>> AddGuestsAsync(long boardId, long[] usersId);

    Task<List<GuestDto>> GetGuestsAsync(long boardId);
}