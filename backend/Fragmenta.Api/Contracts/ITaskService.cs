using Fragmenta.Api.Dtos;

namespace Fragmenta.Api.Contracts
{
    public interface ITaskService
    {
        Task<List<TaskPreviewDto>> GetTasksAsync(long boardId);

        Task<bool> UpdateTaskAsync(long taskId, UpdateTaskRequest request);

        Task<bool> DeleteTaskAsync(long taskId);

        Task<TaskPreviewDto?> CreateTaskAsync(long statusId, CreateOrUpdateTaskRequest request);

        Task ShallowUpdateAsync(ShallowUpdateTaskRequest request);
    }
}
