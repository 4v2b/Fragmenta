using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
using Fragmenta.Dal;
using Fragmenta.Dal.Models;
using Microsoft.EntityFrameworkCore;
using TaskEntity = Fragmenta.Dal.Models.Task;
using Task = System.Threading.Tasks.Task;

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

        public async Task<TaskPreviewDto?> CreateTaskAsync(long statusId, CreateOrUpdateTaskRequest request)
        {
            _logger.LogInformation("Creating task for status with id: {StatusId}", statusId);
            var assignee = request.AssigneeId != null ? await _context.Users.FindAsync(request.AssigneeId) : null;

            _logger.LogInformation("Found assignee for requested id {Id} : {Name} {Email}", request.AssigneeId,
                assignee?.Name, assignee?.Email);

            _logger.LogInformation("Existing statuses with id's: {StatusId}",
                string.Join(", ", _context.Statuses.Select(e => e.Id.ToString())));

            var status = await _context.Statuses.FindAsync(statusId);

            _logger.LogInformation("Status id: {StatusId}", status?.Id);

            List<Tag> tags = [];

            foreach (var tagId in request.TagsId)
            {
                var tag = await _context.Tags.FindAsync(tagId);

                if (tag != null)
                {
                    tags.Add(tag);
                }
            }

            if (status == null)
            {
                return null;
            }

            var task = new TaskEntity()
            {
                Assignee = assignee,
                Description = request.Description,
                DueDate = request.DueDate,
                Priority = request.Priority,
                Status = status,
                Tags = tags,
                Title = request.Title,
                Weight = request.Weight
            };

            await _context.AddAsync(task);
            await  _context.SaveChangesAsync();

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

        public async Task<bool> DeleteTaskAsync(long taskId)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
            {
                return false;
            }

            _context.Remove(task);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<TaskPreviewDto?> GetTaskAsync(long taskId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<TaskPreviewDto>> GetTasksAsync(long boardId)
        {
            return await _context.Tasks
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
                .ToListAsync();
        }

        public async Task ShallowUpdateAsync(ShallowUpdateTaskRequest request)
        {
            var task = await _context.Tasks.FindAsync(request.Id);

            if (task == null)
                return;
            
            task.Weight = request.Weight;
            task.StatusId = request.StatusId;

            _context.Update(task);

            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateTaskAsync(long taskId, UpdateTaskRequest request)
        {
            var task = await _context.Tasks.FindAsync(taskId);

            if (task == null)
                return false;

            if (task.AssigneeId != request.AssigneeId)
            {
                var assignee = request.AssigneeId == null ? await _context.Users.FindAsync(request.AssigneeId) : null;
                task.Assignee = assignee;
            }

            task.Description = request.Description;
            task.Title = request.Title;
            task.DueDate = request.DueDate;

            List<Tag> tags = new();

            foreach (var tagId in request.TagsId)
            {
                var tag = await _context.Tags.FindAsync(tagId);

                if (tag != null)
                {
                    tags.Add(tag);
                }
            }

            task.Tags = tags;
            task.Priority = request.Priority;

            _context.Update(task);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}