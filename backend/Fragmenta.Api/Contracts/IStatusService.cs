using Fragmenta.Api.Dtos;

namespace Fragmenta.Api.Contracts
{
    public interface IStatusService
    {
        Task<StatusDto?> CreateStatusAsync(long boardId, CreateOrUpdateStatusRequest request);

        Task<StatusDto?> UpdateStatusAsync(long statusId, CreateOrUpdateStatusRequest request);

        Task<bool> DeleteStatusAsync(long statusId);
    }
}
