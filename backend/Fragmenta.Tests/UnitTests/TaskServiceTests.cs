using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
using Fragmenta.Api.Services;
using Fragmenta.Dal.Models;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using TaskEntity = Fragmenta.Dal.Models.Task;
using Task = System.Threading.Tasks.Task;

namespace Fragmenta.Tests.UnitTests
{
    public class TaskServiceTests : UnitTestsBase
    {
        [Fact]
        public async Task CreateTaskAsync_ReturnsTaskPreview_WhenStatusExists()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var user = new User { Id = 1, Name = "Test User", Email = "test@example.com" , PasswordHash = [1, 2, 3, 4], PasswordSalt = []};
            var tag = new Tag { Id = 1, Name = "Test Tag" };
            var board = new Board { Id = 1, Name = "Test Board" };
            var status = new Status { Id = 1, Name = "Test Status", BoardId = board.Id, Board = board,  ColorHex = "#FFFFFF", TaskLimit = 0 };
            
            context.Users.Add(user);
            context.Tags.Add(tag);
            context.Boards.Add(board);
            context.Statuses.Add(status);
            await context.SaveChangesAsync();
            
            var request = new CreateOrUpdateTaskRequest
            {
                AssigneeId = user.Id,
                Description = "Test Description",
                DueDate = DateTime.Now.AddDays(1),
                Priority = 2,
                TagsId = new List<long> { tag.Id },
                Title = "Test Task",
                Weight = 1
            };
            
            var service = new TaskService(NullLogger<TaskService>.Instance, context);

            // Act
            var result = await service.CreateTaskAsync(status.Id, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.Title, result.Title);
            Assert.Equal(request.Description, result.Description);
            Assert.Equal(request.Priority, result.Priority);
            Assert.Equal(request.Weight, result.Weight);
            Assert.Equal(status.Id, result.StatusId);
            Assert.Equal(user.Id, result.AssigneeId);
            Assert.Contains(tag.Id, result.TagsId);
        }

        [Fact]
        public async Task CreateTaskAsync_ReturnsNull_WhenStatusDoesNotExist()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var request = new CreateOrUpdateTaskRequest
            {
                Title = "Test Task",
                Description = "Test Description",
                AssigneeId = null,
                DueDate = null,
                Priority = 0,
                Weight = 0,
                TagsId = []
            };
            
            var service = new TaskService(NullLogger<TaskService>.Instance, context);

            // Act
            var result = await service.CreateTaskAsync(999, request);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteTaskAsync_ReturnsTrue_WhenTaskExists()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var task = new TaskEntity { Id = 1, Title = "Test Task" };
            
            context.Tasks.Add(task);
            await context.SaveChangesAsync();
            
            var service = new TaskService(NullLogger<TaskService>.Instance, context);

            // Act
            var result = await service.DeleteTaskAsync(task.Id);

            // Assert
            Assert.True(result);
            Assert.Null(await context.Tasks.FindAsync(task.Id));
        }

        [Fact]
        public async Task DeleteTaskAsync_ReturnsFalse_WhenTaskDoesNotExist()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var service = new TaskService(NullLogger<TaskService>.Instance, context);

            // Act
            var result = await service.DeleteTaskAsync(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetTasksAsync_ReturnsTasksForBoard()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var board = new Board { Id = 1, Name = "Test Board" };
            var status1 = new Status { Id = 1, Name = "Status 1", BoardId = board.Id, Board = board,  ColorHex = "#FFFFFF", TaskLimit = 0 };
            var status2 = new Status { Id = 2, Name = "Status 2", BoardId = board.Id, Board = board, ColorHex = "#FFFFFF", TaskLimit = 0 };
            var otherBoard = new Board { Id = 2, Name = "Another Board" };
            var otherStatus = new Status { Id = 3, Name = "Other Status", ColorHex = "#FFFFFF", TaskLimit = 0, BoardId = otherBoard.Id, Board = otherBoard };
            
            context.Boards.AddRange(board, otherBoard);
            context.Statuses.AddRange(status1, status2, otherStatus);
            await context.SaveChangesAsync();
            
            var task1 = new TaskEntity { Id = 1, Title = "Task 1", StatusId = status1.Id, Status = status1 };
            var task2 = new TaskEntity { Id = 2, Title = "Task 2", StatusId = status2.Id, Status = status2 };
            var otherTask = new TaskEntity { Id = 3, Title = "Other Task", StatusId = otherStatus.Id, Status = otherStatus };
            
            context.Tasks.AddRange(task1, task2, otherTask);
            await context.SaveChangesAsync();
            
            var service = new TaskService(NullLogger<TaskService>.Instance, context);

            // Act
            var result = await service.GetTasksAsync(board.Id);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, t => t.Id == task1.Id);
            Assert.Contains(result, t => t.Id == task2.Id);
            Assert.DoesNotContain(result, t => t.Id == otherTask.Id);
        }

        [Fact]
        public async Task ShallowUpdateAsync_UpdatesTaskWeightAndStatus()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var board = new Board { Id = 1, Name = "Test Board" };
            var status1 = new Status { Id = 1, Name = "Status 1", BoardId = board.Id, Board = board, ColorHex = "#FFFFFF", TaskLimit = 0};
            var status2 = new Status { Id = 2, Name = "Status 2", BoardId = board.Id, Board = board, ColorHex = "#FFFFFF", TaskLimit = 0 };
            
            context.Boards.Add(board);
            context.Statuses.AddRange(status1, status2);
            await context.SaveChangesAsync();
            
            var task = new TaskEntity { Id = 1, Title = "Test Task", StatusId = status1.Id, Status = status1, Weight = 1 };
            
            context.Tasks.Add(task);
            await context.SaveChangesAsync();
            
            var request = new ShallowUpdateTaskRequest
            {
                Id = task.Id,
                StatusId = status2.Id,
                Weight = 5,
                BoardId = board.Id
            };
            
            var service = new TaskService(NullLogger<TaskService>.Instance, context);

            // Act
            await service.ShallowUpdateAsync(request);

            // Assert
            var updatedTask = await context.Tasks.FindAsync(task.Id);
            Assert.Equal(status2.Id, updatedTask.StatusId);
            Assert.Equal(5, updatedTask.Weight);
        }

        [Fact]
        public async Task UpdateTaskAsync_ReturnsTrue_AndUpdatesTask_WhenTaskExists()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var user1 = new User { Id = 1, Email = "email2", Name = "User 1", PasswordHash = [1, 2, 3, 4], PasswordSalt = [] };
            var user2 = new User { Id = 2, Email = "email2",  Name = "User 2", PasswordHash = [1, 2, 3, 4], PasswordSalt = []};
            var tag1 = new Tag { Id = 1, Name = "Tag 1" };
            var tag2 = new Tag { Id = 2, Name = "Tag 2" };
            var status = new Status { Id = 1, Name = "Status", ColorHex = "#FFFFFF", TaskLimit = 0 };
            
            context.Users.AddRange(user1, user2);
            context.Tags.AddRange(tag1, tag2);
            context.Statuses.Add(status);
            await context.SaveChangesAsync();
            
            var task = new TaskEntity
            {
                Id = 1,
                Title = "Original Title",
                Description = "Original Description",
                AssigneeId = user1.Id,
                StatusId = status.Id,
                Priority = 1,
                DueDate = DateTime.Now
            };
            task.Tags = new List<Tag> { tag1 };
            
            context.Tasks.Add(task);
            await context.SaveChangesAsync();
            
            var request = new UpdateTaskRequest
            {
                AssigneeId = user2.Id,
                Description = "Updated Description",
                DueDate = DateTime.Now.AddDays(7),
                Priority = 3,
                TagsId = new List<long> { tag2.Id },
                Title = "Updated Title"
            };
            
            var service = new TaskService(NullLogger<TaskService>.Instance, context);

            // Act
            var result = await service.UpdateTaskAsync(task.Id, request);

            // Assert
            Assert.True(result);
            
            var updatedTask = await context.Tasks.FindAsync(task.Id);
            Assert.Equal("Updated Title", updatedTask.Title);
            Assert.Equal("Updated Description", updatedTask.Description);
            Assert.Equal(user2.Id, updatedTask.AssigneeId);
            Assert.Equal(3, updatedTask.Priority);
            Assert.Single(updatedTask.Tags);
            Assert.Contains(updatedTask.Tags, t => t.Id == tag2.Id);
        }

        [Fact]
        public async Task UpdateTaskAsync_ReturnsFalse_WhenTaskDoesNotExist()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var request = new UpdateTaskRequest
            {
                Title = "Updated Title",
                Description = "Updated Description",
                AssigneeId = null,
                DueDate = null,
                Priority = 0,
                TagsId = []
            };
            
            var service = new TaskService(NullLogger<TaskService>.Instance, context);

            // Act
            var result = await service.UpdateTaskAsync(999, request);

            // Assert
            Assert.False(result);
        }
    }
}