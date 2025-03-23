using Fragmenta.Api.Dtos;

namespace Fragmenta.Api.Contracts
{
    public interface ITaskService
    {
        List<TaskPreviewDto> GetTasks(long boardId);

        TaskPreviewDto? GetTask(long taskId);

        bool UpdateTask(long taskId, UpdateTaskRequest request);

        bool DeleteTask(long taskId);

        TaskPreviewDto? CreateTask(long statusId, CreateOrUpdateTaskRequest request);

        void ShallowUpdate(List<ShallowUpdateTaskRequest> request);
    }
}
