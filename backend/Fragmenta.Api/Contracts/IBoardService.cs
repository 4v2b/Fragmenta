using Fragmenta.Api.Dtos;

namespace Fragmenta.Api.Contracts
{
    public interface IBoardService
    {
        List<BoardDto> GetBoards(long workspaceId);

        bool CanViewBoard(long boardId, long userId);

        List<BoardDto> GetGuestBoards(long workspaceId, long guestId);

        BoardDto? CreateBoard(long workspaceId, CreateBoardRequest request);

        BoardDto? UpdateBoard(long boardId, UpdateBoardRequest request);

        bool RemoveGuest(long boardId, long guestId);

        List<MemberDto> AddGuests(long boardId, long[] usersId);

        List<GuestDto> GetGuests(long boardId);
    }
}
