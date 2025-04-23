using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
using Fragmenta.Api.Enums;
using Fragmenta.Api.Services;
using Fragmenta.Dal.Models;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Role = Fragmenta.Api.Enums.Role;
using Task = System.Threading.Tasks.Task;

namespace Fragmenta.Tests.UnitTests
{
    public class WorkspaceServiceTests : UnitTestsBase
    {
        [Fact]
        public async Task CreateAsync_ReturnsWorkspace_WhenUserAndRoleExist()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var user = new User { Id = 1, Name = "Test User", Email = "test@example.com",
                PasswordSalt = [],
                PasswordHash = [1, 2, 3, 4] };

            context.Users.Add(user);
            
            await context.SaveChangesAsync();
            var role = context.Roles.Find((long)Role.Owner);
            var service = new WorkspaceService(NullLogger<WorkspaceService>.Instance, context);
            var workspaceName = "Test Workspace";

            // Act
            var result = await service.CreateAsync(workspaceName, user.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(workspaceName, result.Name);
            
            // Check that workspace was saved
            var savedWorkspace = await context.Workspaces.FindAsync(result.Id);
            Assert.NotNull(savedWorkspace);
            Assert.Equal(workspaceName, savedWorkspace.Name);
            
            // Check that access was created with correct role
            var access = await context.WorkspaceAccesses.FindAsync(savedWorkspace.Id, user.Id);
            Assert.NotNull(access);
            Assert.Equal(role.Id, access.RoleId);
        }

        [Fact]
        public async Task CreateAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            // Arrange
            var context = CreateInMemoryContext();
            
            var service = new WorkspaceService(NullLogger<WorkspaceService>.Instance, context);

            // Act
            var result = await service.CreateAsync("Test Workspace", 999);

            // Assert
            Assert.Null(result);
            Assert.Empty(context.Workspaces);
        }

        [Fact]
        public async Task CreateAsync_ReturnsNull_WhenRoleDoesNotExist()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var user = new User { Id = 1, Name = "Test User", Email = "test@example.com",
                PasswordSalt = [],
                PasswordHash = [1, 2, 3, 4] };

            var ownerRole = context.Roles.Find((long)Role.Owner);
            context.Roles.Remove(ownerRole);

            context.Users.Add(user);
            await context.SaveChangesAsync();
            
            var service = new WorkspaceService(NullLogger<WorkspaceService>.Instance, context);

            // Act
            var result = await service.CreateAsync("Test Workspace", user.Id);

            // Assert
            Assert.Null(result);
            Assert.Empty(context.Workspaces);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsTrue_AndRemovesWorkspace_WhenWorkspaceExists()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var workspace = new Workspace { Id = 1, Name = "Test Workspace" };
            
            context.Workspaces.Add(workspace);
            await context.SaveChangesAsync();
            
            var service = new WorkspaceService(NullLogger<WorkspaceService>.Instance, context);

            // Act
            var result = await service.DeleteAsync(workspace.Id);

            // Assert
            Assert.True(result);
            Assert.Null(await context.Workspaces.FindAsync(workspace.Id));
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenWorkspaceDoesNotExist()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var service = new WorkspaceService(NullLogger<WorkspaceService>.Instance, context);

            // Act
            var result = await service.DeleteAsync(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsWorkspaces_ForUser()
        {
            // Arrange
            var context = CreateInMemoryContext();
            
            var user = new User { Id = 1, Name = "Test User", Email = "test@example.com" ,
                PasswordSalt = [],
                PasswordHash = [1, 2, 3, 4] };
            var workspace1 = new Workspace { Id = 1, Name = "Workspace 1" };
            var workspace2 = new Workspace { Id = 2, Name = "Workspace 2" };
            var ownerRole = context.Roles.Find((long)Role.Owner);
            var memberRole = context.Roles.Find((long)Role.Member);
            
            context.Users.Add(user);
            context.Workspaces.AddRange(workspace1, workspace2);
            
            var access1 = new WorkspaceAccess { WorkspaceId = workspace1.Id, UserId = user.Id, RoleId = ownerRole.Id, JoinedAt = DateTime.UtcNow, User = user, Workspace = workspace1, Role = ownerRole };
            var access2 = new WorkspaceAccess { WorkspaceId = workspace2.Id, UserId = user.Id, RoleId = memberRole.Id, JoinedAt = DateTime.UtcNow, User = user, Workspace = workspace2, Role = memberRole };
            
            context.WorkspaceAccesses.AddRange(access1, access2);
            await context.SaveChangesAsync();
            
            var service = new WorkspaceService(NullLogger<WorkspaceService>.Instance, context);

            // Act
            var result = await service.GetAllAsync(user.Id);

            // Assert
            Assert.Equal(2, result.Count);
            
            var workspace1Result = result.FirstOrDefault(w => w.Id == workspace1.Id);
            Assert.NotNull(workspace1Result);
            Assert.Equal(workspace1.Name, workspace1Result.Name);
            Assert.Equal(Enum.GetName(Role.Owner), workspace1Result.Role);
            
            var workspace2Result = result.FirstOrDefault(w => w.Id == workspace2.Id);
            Assert.NotNull(workspace2Result);
            Assert.Equal(workspace2.Name, workspace2Result.Name);
            Assert.Equal(Enum.GetName(Role.Member), workspace2Result.Role);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsUpdatedWorkspace_WhenWorkspaceExists()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var workspace = new Workspace { Id = 1, Name = "Original Name" };
            
            context.Workspaces.Add(workspace);
            await context.SaveChangesAsync();
            
            var service = new WorkspaceService(NullLogger<WorkspaceService>.Instance, context);
            var newName = "Updated Name";

            // Act
            var result = await service.UpdateAsync(newName, workspace.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(workspace.Id, result.Id);
            Assert.Equal(newName, result.Name);
            
            var updatedWorkspace = await context.Workspaces.FindAsync(workspace.Id);
            Assert.Equal(newName, updatedWorkspace.Name);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsNull_WhenWorkspaceDoesNotExist()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var service = new WorkspaceService(NullLogger<WorkspaceService>.Instance, context);

            // Act
            var result = await service.UpdateAsync("New Name", 999);

            // Assert
            Assert.Null(result);
        }
    }
}