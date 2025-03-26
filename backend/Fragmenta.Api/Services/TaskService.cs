using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
using Fragmenta.Dal;
using Fragmenta.Dal.Models;
using Microsoft.EntityFrameworkCore;
using Task = Fragmenta.Dal.Models.Task;

namespace Fragmenta.Api.Services
{
    public class TaskService : ITaskService
    {
        private readonly ILogger<TaskService> _logger;
        private readonly ApplicationContext _context;

        public TaskService(ILogger<TaskService> logger, ApplicationContext context)
        {
            _logger = logger;
            _context = context;
        }

        public TaskPreviewDto? CreateTask(long statusId, CreateOrUpdateTaskRequest request)
        {
            _logger.LogInformation("Creating task for status with id: {StatusId}", statusId);
            var assigeee = request.AssigneeId != null ? _context.Users.Find(request.AssigneeId) : null;

            _logger.LogInformation("Found assignee for requested id {Id} : {Name} {Email}", request.AssigneeId, assigeee?.Name, assigeee?.Email);

            _logger.LogInformation("Existing statuses with id's: {StatusId}", string.Join(", ", _context.Statuses.Select(e => e.Id.ToString())));

            var status = _context.Statuses.Find(statusId);

            _logger.LogInformation("Status id: {StatusId}", status?.Id);

            List<Tag> tags = new();

            foreach (var tagId in request.TagsId)
            {
                var tag = _context.Tags.Find(tagId);

                if (tag != null)
                {
                    tags.Add(tag);
                }
            }

            if (status == null)
            {
                return null;
            }

            var task = new Task()
            {
                Assignee = assigeee,
                Description = request.Description,
                DueDate = request.DueDate,
                Priority = request.Priority,
                Status = status,
                Tags = tags,
                Title = request.Title,
                Weight = request.Weight
            };

            _context.Add(task);
            _context.SaveChanges();

            _logger.LogInformation("Created task with id: {TaskId}", task.Id);

            return new TaskPreviewDto()
            {
                Id = task.Id,
                AssigneeId = task.AssigneeId,
                Description = task.Description,
                DueDate = task.DueDate,
                Priority = task.Priority,
                StatusId = task.StatusId,
                TagsId = tags
                        .Select(i => i.Id)
                        .ToList(),
                Title = task.Title,
                Weight = task.Weight
            };
        }

        public bool DeleteTask(long taskId)
        {
            var task = _context.Tasks.Find(taskId);
            if (task == null)
            {
                return false;
            }

            _context.Remove(task);

            _context.SaveChanges();
            return true;
        }

        public TaskPreviewDto? GetTask(long taskId)
        {
            throw new NotImplementedException();
        }

        public List<TaskPreviewDto> GetTasks(long boardId)
        {
            return _context.Tasks
                .Include(e => e.Status)
                .Include(e => e.Tags)
                .Where(e => e.Status.BoardId == boardId)
                .Select(e => new TaskPreviewDto()
                {
                    Id = e.Id,
                    AssigneeId = e.AssigneeId,
                    Description = e.Description,
                    DueDate = e.DueDate,
                    Priority = e.Priority,
                    StatusId = e.StatusId,
                    TagsId = e.Tags
                        .Select(i => i.Id)
                        .ToList(),
                    Title = e.Title,
                    Weight = e.Weight
                })
                .ToList();
        }

        public void ShallowUpdate(List<ShallowUpdateTaskRequest> request)
        {
            List<Task> tasks = new();
            foreach (var update in request)
            {
                var task = _context.Tasks.Find(update.Id);

                if (task == null)
                {
                    continue;
                }

                task.Weight = update.Weight;
                task.StatusId = update.StatusId;
                tasks.Add(task);
            }

            _context.UpdateRange(tasks);

            _context.SaveChanges();
        }

        public bool UpdateTask(long taskId, UpdateTaskRequest request)
        {
            var task = _context.Tasks.Find(taskId);

            if(task == null)
            {
                return false;
            }

            if(task.AssigneeId != request.AssigneeId)
            {
                var assignee = request.AssigneeId == null ? _context.Users.Find(request.AssigneeId) : null;
                task.Assignee = assignee;
            }

            task.Description = request.Description;
            task.Title = request.Title;
            task.DueDate = request.DueDate;

            List<Tag> tags = new();

            foreach (var tagId in request.TagsId)
            {
                var tag = _context.Tags.Find(tagId);

                if (tag != null)
                {
                    tags.Add(tag);
                }
            }

            task.Tags = tags;

            task.Priority = request.Priority;

            _context.Update(task);
            _context.SaveChanges();

            return true;
        }
    }
}
