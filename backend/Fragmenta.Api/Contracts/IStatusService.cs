using Fragmenta.Api.Dtos;

namespace Fragmenta.Api.Contracts
{
    public interface IStatusService
    {
        StatusDto? CreateStatus(long boardId, CreateOrUpdateStatusRequest request);

        StatusDto? UpdateStatus(long statusId, CreateOrUpdateStatusRequest request);

        bool DeleteStatus(long statusId);
    }
}
