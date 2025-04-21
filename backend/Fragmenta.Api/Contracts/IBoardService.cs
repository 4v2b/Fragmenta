using Fragmenta.Api.Dtos;

namespace Fragmenta.Api.Contracts
{
    public interface IBoardService
    {
        Task<List<BoardDto>> GetBoardsAsync(long workspaceId);
        
        Task<FullBoardDto?> GetBoardAsync(long boardId);

        Task<List<BoardDto>> GetGuestBoardsAsync(long workspaceId, long guestId);

        Task<BoardDto?> CreateBoardAsync(long workspaceId, CreateBoardRequest request);

        Task<BoardDto?> UpdateBoardAsync(long boardId, UpdateBoardRequest request);
        
        Task<bool> DeleteBoardAsync(long boardId);
    }
}
