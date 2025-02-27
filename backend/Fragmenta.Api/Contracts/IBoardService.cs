using Fragmenta.Api.Dtos;

namespace Fragmenta.Api.Contracts
{
    public interface IBoardService
    {
        List<BoardDto> GetBoards(long workspaceId);

        BoardDto? CreateBoard(long workspaceId, CreateBoardRequest request);

        BoardDto? UpdateBoard(long boardId, CreateBoardRequest request);

        List<UserDto> UpdateGuestList(long[] usersId);

        List<BoardDto> GetGuestBoards(long workspaceId, long guestId);
    }
}
