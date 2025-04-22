using Fragmenta.Api.Controllers;
using Fragmenta.Api.Services;
using Fragmenta.Dal.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Task = System.Threading.Tasks.Task;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using RoleEnum = Fragmenta.Api.Enums.Role;

namespace Fragmenta.Tests.UnitTests;

public class BoardAccessServiceTests : UnitTestsBase
{
    private readonly ILogger<BoardAccessService> _logger;

    public BoardAccessServiceTests()
    {
        _logger = new NullLogger<BoardAccessService>();
    }

    [Fact]
    public async Task RemoveGuestAsync_UserIsGuest_RemovesGuestSuccessfully()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new BoardAccessService(_logger, context);
        
        var workspace = new Workspace { Id = 1, Name = "Workspace" };
        var board = new Board { Id = 1, WorkspaceId = workspace.Id , Name = "Board" };
        var user = new User { Id = 1, Email = "test@example.com", Name = "Test User", PasswordHash = [], PasswordSalt = [] };
        var access = new BoardAccess { BoardId = board.Id, UserId = user.Id };
        var workspaceAccess = new WorkspaceAccess 
        { 
            WorkspaceId = workspace.Id, 
            UserId = user.Id,
            RoleId = (long)RoleEnum.Guest,
            JoinedAt = DateTime.UtcNow
        };

        await context.Workspaces.AddAsync(workspace);
        await context.Boards.AddAsync(board);
        await context.Users.AddAsync(user);
        await context.BoardAccesses.AddAsync(access);
        await context.WorkspaceAccesses.AddAsync(workspaceAccess);
        await context.SaveChangesAsync();

        // Act
        var result = await service.RemoveGuestAsync(board.Id, user.Id);

        // Assert
        Assert.True(result);
        Assert.Empty(await context.BoardAccesses.ToListAsync());
        Assert.Empty(await context.WorkspaceAccesses.ToListAsync());
    }

    [Fact]
    public async Task RemoveGuestAsync_UserHasMultipleBoards_RemovesBoardAccessOnly()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new BoardAccessService(_logger, context);
        
        var workspace = new Workspace { Id = 1,Name = "Workspace", };
        var board1 = new Board { Id = 1,Name = "Board1", WorkspaceId = workspace.Id };
        var board2 = new Board { Id = 2, Name = "Board2",WorkspaceId = workspace.Id };
        var user = new User { Id = 1, Email = "test@example.com", Name = "Test User", PasswordHash = [], PasswordSalt = []};
        
        var access1 = new BoardAccess { BoardId = board1.Id, UserId = user.Id };
        var access2 = new BoardAccess { BoardId = board2.Id, UserId = user.Id };
        
        var workspaceAccess = new WorkspaceAccess 
        { 
            WorkspaceId = workspace.Id, 
            UserId = user.Id,
            RoleId = (long)RoleEnum.Guest,
            JoinedAt = DateTime.UtcNow
        };

        await context.Workspaces.AddAsync(workspace);
        await context.Boards.AddRangeAsync(board1, board2);
        await context.Users.AddAsync(user);
        await context.BoardAccesses.AddRangeAsync(access1, access2);
        await context.WorkspaceAccesses.AddAsync(workspaceAccess);
        await context.SaveChangesAsync();

        // Act
        var result = await service.RemoveGuestAsync(board1.Id, user.Id);

        // Assert
        Assert.True(result);
        Assert.Single(await context.BoardAccesses.ToListAsync());
        Assert.Single(await context.WorkspaceAccesses.ToListAsync());
    }

    [Fact]
    public async Task RemoveGuestAsync_BoardNotFound_ReturnsFalse()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new BoardAccessService(_logger, context);

        // Act
        var result = await service.RemoveGuestAsync(1, 1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task AddGuestsAsync_NewGuests_AddsSuccessfully()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new BoardAccessService(_logger, context);
        
        var workspace = new Workspace { Id = 1, Name = "Workspace" };
        var board = new Board { Id = 1, Name = "Board", WorkspaceId = workspace.Id };
        var user1 = new User { Id = 1, Email = "user1@example.com", Name = "User One", PasswordHash = [], PasswordSalt = []};
        var user2 = new User { Id = 2, Email = "user2@example.com", Name = "User Two", PasswordHash = [], PasswordSalt = []};

        await context.Workspaces.AddAsync(workspace);
        await context.Boards.AddAsync(board);
        await context.Users.AddRangeAsync(user1, user2);
        await context.SaveChangesAsync();

        // Act
        var result = await service.AddGuestsAsync(board.Id, new long[] { user1.Id, user2.Id });

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(2, await context.BoardAccesses.CountAsync());
        Assert.Equal(2, await context.WorkspaceAccesses.CountAsync());
        
        foreach (var member in result)
        {
            Assert.Equal("Guest", member.Role);
        }
    }

    [Fact]
    public async Task AddGuestsAsync_ExistingWorkspaceUsers_AddsOnlyBoardAccess()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new BoardAccessService(_logger, context);
        
        var workspace = new Workspace { Id = 1, Name = "Workspace"};
        var board = new Board { Id = 1, Name = "Board", WorkspaceId = workspace.Id };
        var user = new User { Id = 1, Email = "user@example.com", Name = "Test User", PasswordHash = [], PasswordSalt = []};
        
        var workspaceAccess = new WorkspaceAccess 
        { 
            WorkspaceId = workspace.Id, 
            UserId = user.Id,
            RoleId = (long)RoleEnum.Guest,
            JoinedAt = DateTime.UtcNow
        };

        await context.Workspaces.AddAsync(workspace);
        await context.Boards.AddAsync(board);
        await context.Users.AddAsync(user);
        await context.WorkspaceAccesses.AddAsync(workspaceAccess);
        await context.SaveChangesAsync();

        // Act
        var result = await service.AddGuestsAsync(board.Id, new long[] { user.Id });

        // Assert
        Assert.Single(result);
        Assert.Single(await context.BoardAccesses.ToListAsync());
        Assert.Single(await context.WorkspaceAccesses.ToListAsync());
    }

    [Fact]
    public async Task GetGuestsAsync_ReturnsAllGuests()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new BoardAccessService(_logger, context);
        
        var board = new Board { Id = 1, Name = "Board"};
        var user1 = new User { Id = 1, Email = "user1@example.com", Name = "User One", PasswordHash = [], PasswordSalt = [] };
        var user2 = new User { Id = 2, Email = "user2@example.com", Name = "User Two", PasswordHash = [], PasswordSalt = [] };
        
        var access1 = new BoardAccess { BoardId = board.Id, UserId = user1.Id, Board = board, User = user1 };
        var access2 = new BoardAccess { BoardId = board.Id, UserId = user2.Id, Board = board, User = user2 };

        await context.Boards.AddAsync(board);
        await context.Users.AddRangeAsync(user1, user2);
        await context.BoardAccesses.AddRangeAsync(access1, access2);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetGuestsAsync(board.Id);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, g => g.Id == user1.Id);
        Assert.Contains(result, g => g.Id == user2.Id);
    }

    [Fact]
    public async Task CanViewBoardAsync_UserHasAccess_ReturnsTrue()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new BoardAccessService(_logger, context);
        
        var board = new Board { Id = 1, Name = "Board"};
        var user = new User { Id = 1, Email = "user1@example.com", Name = "User One", PasswordHash = [], PasswordSalt = [] };
        var access = new BoardAccess { BoardId = board.Id, UserId = user.Id };

        await context.Boards.AddAsync(board);
        await context.Users.AddAsync(user);
        await context.BoardAccesses.AddAsync(access);
        await context.SaveChangesAsync();

        // Act
        var result = await service.CanViewBoardAsync(board.Id, user.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CanViewBoardAsync_UserNoAccess_ReturnsFalse()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new BoardAccessService(_logger, context);
        
        var board = new Board { Id = 1, Name = "Board"};
        await context.Boards.AddAsync(board);
        await context.SaveChangesAsync();

        // Act
        var result = await service.CanViewBoardAsync(board.Id, 999);

        // Assert
        Assert.False(result);
    }
}