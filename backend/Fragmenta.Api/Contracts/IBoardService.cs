using Fragmenta.Api.Dtos;

namespace Fragmenta.Api.Contracts
{
    public interface IBoardService
    {
        List<BoardDto> GetBoards(long workspaceId);
        
        FullBoardDto? GetBoard(long boardId);

        List<BoardDto> GetGuestBoards(long workspaceId, long guestId);

        BoardDto? CreateBoard(long workspaceId, CreateBoardRequest request);

        BoardDto? UpdateBoard(long boardId, UpdateBoardRequest request);
        
        bool DeleteBoard(long boardId);
    }
}
