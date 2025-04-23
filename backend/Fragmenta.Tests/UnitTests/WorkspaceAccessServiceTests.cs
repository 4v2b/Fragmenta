using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
using Fragmenta.Api.Enums;
using Fragmenta.Api.Services;
using Fragmenta.Dal.Models;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using Task = System.Threading.Tasks.Task;
using Xunit;
using Role = Fragmenta.Api.Enums.Role;

namespace Fragmenta.Tests.UnitTests
{
    public class WorkspaceAccessServiceTests : UnitTestsBase
    {
        [Fact]
        public async Task AddMemberAsync_ReturnsNewMember_WhenUserNotAlreadyMember()
        {
            // Arrange
            var context = CreateInMemoryContext();
            
            var workspace = new Workspace { Id = 1, Name = "Test Workspace" };
            var user = new User { Id = 1, Name = "Test User", Email = "test@example.com",
                PasswordSalt = [],
                PasswordHash = [1, 2, 3, 4] };
            
            context.Workspaces.Add(workspace);
            context.Users.Add(user);
            await context.SaveChangesAsync();
            
            var service = new WorkspaceAccessService(NullLogger<WorkspaceAccessService>.Instance, context);

            // Act
            var result = await service.AddMemberAsync(workspace.Id, user.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal(user.Email, result.Email);
            Assert.Equal(user.Name, result.Name);
            Assert.Equal(Enum.GetName(Role.Member), result.Role);
            
            var access = await context.WorkspaceAccesses.FindAsync(workspace.Id, user.Id);
            Assert.NotNull(access);
            Assert.Equal((long)Role.Member, access.RoleId);
        }

        [Fact]
        public async Task AddMemberAsync_ReturnsNull_WhenUserAlreadyMember()
        {
            // Arrange
            var context = CreateInMemoryContext();
            
            var workspace = new Workspace { Id = 1, Name = "Test Workspace" };
            var user = new User { Id = 1, Name = "Test User", Email = "test@example.com",
                PasswordSalt = [],
                PasswordHash = [1, 2, 3, 4] };
            
            context.Workspaces.Add(workspace);
            context.Users.Add(user);
            
            var access = new WorkspaceAccess { WorkspaceId = workspace.Id, UserId = user.Id, RoleId = (long)Role.Member, JoinedAt = DateTime.UtcNow };
            context.WorkspaceAccesses.Add(access);
            await context.SaveChangesAsync();
            
            var service = new WorkspaceAccessService(NullLogger<WorkspaceAccessService>.Instance, context);

            // Act
            var result = await service.AddMemberAsync(workspace.Id, user.Id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddMembersAsync_ReturnsMembers_WhenUsersFound()
        {
            // Arrange
            var context = CreateInMemoryContext();
            
            var workspace = new Workspace { Id = 1, Name = "Test Workspace" };
            var user1 = new User { Id = 1, Name = "User One", Email = "user1@example.com" ,
                PasswordSalt = [],
                PasswordHash = [1, 2, 3, 4]};
            var user2 = new User { Id = 2, Name = "User Two", Email = "user2@example.com",
                PasswordSalt = [],
                PasswordHash = [1, 2, 3, 4] };
            
            context.Workspaces.Add(workspace);
            context.Users.AddRange(user1, user2);
            await context.SaveChangesAsync();
            
            var service = new WorkspaceAccessService(NullLogger<WorkspaceAccessService>.Instance, context);

            // Act
            var result = await service.AddMembersAsync(workspace.Id, new long[] { user1.Id, user2.Id });

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, m => m.Id == user1.Id && m.Email == user1.Email);
            Assert.Contains(result, m => m.Id == user2.Id && m.Email == user2.Email);
            
            var accesses = context.WorkspaceAccesses.Where(a => a.WorkspaceId == workspace.Id).ToList();
            Assert.Equal(2, accesses.Count);
        }

        [Fact]
        public async Task DeleteMemberAsync_ReturnsTrue_AndRemovesAccess_WhenMemberExists()
        {
            // Arrange
            var context = CreateInMemoryContext();
            
            var workspace = new Workspace { Id = 1, Name = "Test Workspace" };
            var user = new User { Id = 1, Name = "Test User", Email = "test@example.com",
                PasswordSalt = [],
                PasswordHash = [1, 2, 3, 4] };
            
            context.Workspaces.Add(workspace);
            context.Users.Add(user);
            
            var access = new WorkspaceAccess { WorkspaceId = workspace.Id, UserId = user.Id, RoleId = (long)Role.Member, JoinedAt = DateTime.UtcNow };
            context.WorkspaceAccesses.Add(access);
            await context.SaveChangesAsync();
            
            var service = new WorkspaceAccessService(NullLogger<WorkspaceAccessService>.Instance, context);

            // Act
            var result = await service.DeleteMemberAsync(workspace.Id, user.Id);

            // Assert
            Assert.True(result);
            Assert.Null(await context.WorkspaceAccesses.FindAsync(workspace.Id, user.Id));
        }

        [Fact]
        public async Task DeleteMemberAsync_ReturnsFalse_WhenMemberDoesNotExist()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var service = new WorkspaceAccessService(NullLogger<WorkspaceAccessService>.Instance, context);

            // Act
            var result = await service.DeleteMemberAsync(1, 1);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteMemberAsync_RemovesBoardAccesses_WhenUserIsGuest()
        {
            // Arrange
            var context = CreateInMemoryContext();
            
            var workspace = new Workspace { Id = 1, Name = "Test Workspace" };
            var user = new User { Id = 1, Name = "Test User", Email = "test@example.com",
                PasswordSalt = [],
                PasswordHash = [1, 2, 3, 4] };
            
            context.Workspaces.Add(workspace);
            context.Users.Add(user);
            
            var access = new WorkspaceAccess { WorkspaceId = workspace.Id, UserId = user.Id, RoleId = (long)Role.Guest, JoinedAt = DateTime.UtcNow };
            context.WorkspaceAccesses.Add(access);
            
            var board = new Board { Id = 1, Name = "Test Board", WorkspaceId = workspace.Id, Workspace = workspace };
            context.Boards.Add(board);
            
            var boardAccess = new BoardAccess { BoardId = board.Id, UserId = user.Id, Board = board, User = user };
            context.BoardAccesses.Add(boardAccess);
            
            await context.SaveChangesAsync();
            
            var service = new WorkspaceAccessService(NullLogger<WorkspaceAccessService>.Instance, context);

            // Act
            var result = await service.DeleteMemberAsync(workspace.Id, user.Id);

            // Assert
            Assert.True(result);
            Assert.Null(await context.WorkspaceAccesses.FindAsync(workspace.Id, user.Id));
            Assert.Empty(context.BoardAccesses.Where(a => a.UserId == user.Id && a.Board.WorkspaceId == workspace.Id));
        }

        [Fact]
        public async Task GetMembersAsync_ReturnsAllMembers_ForWorkspace()
        {
            // Arrange
            var context = CreateInMemoryContext();
            
            var workspace = new Workspace { Id = 1, Name = "Test Workspace" };
            var user1 = new User { Id = 1, Name = "User One", Email = "user1@example.com",
                PasswordSalt = [],
                PasswordHash = [1, 2, 3, 4] };
            var user2 = new User { Id = 2, Name = "User Two", Email = "user2@example.com",
                PasswordSalt = [],
                PasswordHash = [1, 2, 3, 4] };
            
            context.Workspaces.Add(workspace);
            context.Users.AddRange(user1, user2);
            
            var access1 = new WorkspaceAccess { WorkspaceId = workspace.Id, UserId = user1.Id, RoleId = (long)Role.Member, JoinedAt = DateTime.UtcNow, User = user1, Workspace = workspace };
            var access2 = new WorkspaceAccess { WorkspaceId = workspace.Id, UserId = user2.Id, RoleId = (long)Role.Admin, JoinedAt = DateTime.UtcNow, User = user2, Workspace = workspace };
            
            context.WorkspaceAccesses.AddRange(access1, access2);
            await context.SaveChangesAsync();
            
            var service = new WorkspaceAccessService(NullLogger<WorkspaceAccessService>.Instance, context);

            // Act
            var result = await service.GetMembersAsync(workspace.Id);

            // Assert
            Assert.Equal(2, result.Count);
            
            var member = result.FirstOrDefault(m => m.Id == user1.Id);
            Assert.NotNull(member);
            Assert.Equal(user1.Email, member.Email);
            Assert.Equal(user1.Name, member.Name);
            Assert.Equal(Enum.GetName(Role.Member), member.Role);
            
            var admin = result.FirstOrDefault(m => m.Id == user2.Id);
            Assert.NotNull(admin);
            Assert.Equal(user2.Email, admin.Email);
            Assert.Equal(user2.Name, admin.Name);
            Assert.Equal(Enum.GetName(Role.Admin), admin.Role);
        }

        [Fact]
        public async Task GetRoleAsync_ReturnsRole_WhenMemberExists()
        {
            // Arrange
            var context = CreateInMemoryContext();
            
            var workspace = new Workspace { Id = 1, Name = "Test Workspace" };
            var user = new User { Id = 1, Name = "Test User", Email = "test@example.com",
                PasswordSalt = [],
                PasswordHash = [1, 2, 3, 4] };
            
            context.Workspaces.Add(workspace);
            context.Users.Add(user);

            var access = new WorkspaceAccess { WorkspaceId = workspace.Id, UserId = user.Id, RoleId = (long)Role.Admin, JoinedAt = DateTime.UtcNow };
            context.WorkspaceAccesses.Add(access);
            await context.SaveChangesAsync();
            
            var service = new WorkspaceAccessService(NullLogger<WorkspaceAccessService>.Instance, context);

            // Act
            var result = await service.GetRoleAsync(workspace.Id, user.Id);

            // Assert
            Assert.Equal(Role.Admin, result);
        }

        [Fact]
        public async Task GetRoleAsync_ReturnsNull_WhenMemberDoesNotExist()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var service = new WorkspaceAccessService(NullLogger<WorkspaceAccessService>.Instance, context);

            // Act
            var result = await service.GetRoleAsync(1, 1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GrantAdminPermissionAsync_ReturnsTrue_AndUpdatesRole_WhenMemberExists()
        {
            // Arrange
            var context = CreateInMemoryContext();
            
            var workspace = new Workspace { Id = 1, Name = "Test Workspace" };
            var user = new User { Id = 1, Name = "Test User", Email = "test@example.com" ,
                PasswordSalt = [],
                PasswordHash = [1, 2, 3, 4]};
            
            context.Workspaces.Add(workspace);
            context.Users.Add(user);
            
            var access = new WorkspaceAccess { WorkspaceId = workspace.Id, UserId = user.Id, RoleId = (long)Role.Member, JoinedAt = DateTime.UtcNow };
            context.WorkspaceAccesses.Add(access);
            await context.SaveChangesAsync();
            
            var service = new WorkspaceAccessService(NullLogger<WorkspaceAccessService>.Instance, context);

            // Act
            var result = await service.GrantAdminPermissionAsync(workspace.Id, user.Id);

            // Assert
            Assert.True(result);
            
            var updatedAccess = await context.WorkspaceAccesses.FindAsync(workspace.Id, user.Id);
            Assert.Equal((long)Role.Admin, updatedAccess.RoleId);
        }

        [Fact]
        public async Task RevokeAdminPermissionAsync_ReturnsTrue_AndUpdatesRole_WhenAdminExists()
        {
            // Arrange
            var context = CreateInMemoryContext();
            
            var workspace = new Workspace { Id = 1, Name = "Test Workspace" };
            var user = new User { Id = 1, Name = "Test User", Email = "test@example.com",
                PasswordSalt = [],
                PasswordHash = [1, 2, 3, 4] };
            
            context.Workspaces.Add(workspace);
            context.Users.Add(user);
            
            var access = new WorkspaceAccess { WorkspaceId = workspace.Id, UserId = user.Id, RoleId = (long)Role.Admin, JoinedAt = DateTime.UtcNow };
            context.WorkspaceAccesses.Add(access);
            await context.SaveChangesAsync();
            
            var service = new WorkspaceAccessService(NullLogger<WorkspaceAccessService>.Instance, context);

            // Act
            var result = await service.RevokeAdminPermissionAsync(workspace.Id, user.Id);

            // Assert
            Assert.True(result);
            
            var updatedAccess = await context.WorkspaceAccesses.FindAsync(workspace.Id, user.Id);
            Assert.Equal((long)Role.Member, updatedAccess.RoleId);
        }
    }
}