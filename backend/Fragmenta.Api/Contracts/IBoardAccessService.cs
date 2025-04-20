using Fragmenta.Api.Dtos;

namespace Fragmenta.Api.Contracts;

public interface IBoardAccessService
{
    bool CanViewBoard(long boardId, long userId);
    
    bool RemoveGuest(long boardId, long guestId);

    List<MemberDto> AddGuests(long boardId, long[] usersId);

    List<GuestDto> GetGuests(long boardId);
}