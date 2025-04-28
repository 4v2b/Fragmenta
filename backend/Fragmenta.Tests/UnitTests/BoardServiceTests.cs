using Fragmenta.Api.Dtos;
using Fragmenta.Api.Services;
using Fragmenta.Dal;
using Fragmenta.Dal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Task = System.Threading.Tasks.Task;

namespace Fragmenta.Tests.UnitTests;

public class BoardServiceTests : UnitTestsBase
{
    private BoardService CreateService(ApplicationContext context)
    {
        return new BoardService(new NullLogger<BoardService>(), context);
    }

    [Fact]
    public async Task CreateBoardAsync_ReturnsNull_WhenWorkspaceNotFound()
    {
        var context = CreateInMemoryContext();
        var service = CreateService(context);

        var request = new CreateBoardRequest
        {
            Name = "Board A",
            AllowedTypeIds = new List<long> { 1 }
        };

        var result = await service.CreateBoardAsync(999, request);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateBoardAsync_CreatesBoard_WhenWorkspaceExists()
    {
        var context = CreateInMemoryContext();
        context.Workspaces.Add(new Workspace { Id = 1, Name = "Workspace" });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.CreateBoardAsync(1, new CreateBoardRequest
        {
            Name = "Board A",
            AllowedTypeIds = new List<long> { 1 }
        });

        Assert.NotNull(result);
        Assert.Equal("Board A", result!.Name);
        Assert.Single(result.AllowedTypeIds);
    }

    [Fact]
    public async Task GetBoardsAsync_ReturnsBoardsForWorkspace()
    {
        var context = CreateInMemoryContext();
        context.Boards.Add(new Board { Id = 1, Name = "Board 1", WorkspaceId = 5 });
        context.Boards.Add(new Board { Id = 2, Name = "Board 2", WorkspaceId = 5 });
        context.Boards.Add(new Board { Id = 3, Name = "Board 3", WorkspaceId = 99 });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var boards = await service.GetBoardsAsync(5);

        Assert.Equal(2, boards.Count);
    }

    [Fact]
    public async Task UpdateBoardAsync_UpdatesNameAndTypes()
    {
        var context = CreateInMemoryContext();
        var board = new Board { Id = 10, Name = "Old", AttachmentTypes = [], WorkspaceId = 1 };
        context.Boards.Add(board);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var updated = await service.UpdateBoardAsync(10, new UpdateBoardRequest
        {
            Name = "Updated",
            AllowedTypeIds = [ 1 ],
            ArchivedAt = null
        });

        Assert.Equal("Updated", updated!.Name);
        Assert.Equal(1, updated.AllowedTypeIds.Count);
    }

    [Fact]
    public async Task DeleteBoardAsync_ReturnsFalse_IfBoardNotFoundOrNotArchived()
    {
        var context = CreateInMemoryContext();
        context.Boards.Add(new Board { Id = 5, Name = "Not Archived", ArchivedAt = null });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result1 = await service.DeleteBoardAsync(100); // not found
        var result2 = await service.DeleteBoardAsync(5);   // not archived

        Assert.False(result1);
        Assert.False(result2);
    }

    [Fact]
    public async Task DeleteBoardAsync_Deletes_WhenArchived()
    {
        var context = CreateInMemoryContext();
        context.Boards.Add(new Board { Id = 7, Name = "To Delete", ArchivedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var result = await service.DeleteBoardAsync(7);

        Assert.True(result);
        Assert.Empty(context.Boards);
    }

    [Fact]
    public async Task GetBoardAsync_ReturnsFullBoard()
    {
        var context = CreateInMemoryContext();

        var board = new Board { Id = 1, Name = "TestBoard" };
        var type = new AttachmentType { Id = 42, Value = "pdf", Boards = new List<Board> { board } };
        var status = new Status
        {
            Id = 9,
            BoardId = 1,
            Name = "In Progress",
            ColorHex = "#00FF00",
            Weight = 1,
            TaskLimit = 10
        };

        context.Boards.Add(board);
        context.AttachmentTypes.Add(type);
        context.Statuses.Add(status);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var result = await service.GetBoardAsync(1);

        Assert.NotNull(result);
        Assert.Single(result!.AllowedTypeIds);
        Assert.Single(result.Statuses);
    }
    
     [Fact]
    public async Task UpdateBoardAsync_ReturnsNull_WhenBoardNotFound()
    {
        var context = CreateInMemoryContext();
        var service = CreateService(context);

        var result = await service.UpdateBoardAsync(999, new UpdateBoardRequest
        {
            Name = "Doesn't matter",
            AllowedTypeIds = new List<long>(),
            ArchivedAt = null
        });

        Assert.Null(result);
    }

    [Fact]
    public async Task GetGuestBoardsAsync_ReturnsBoards_WhenUserHasAccess()
    {
        var context = CreateInMemoryContext();
        var user = new User { Id = 2, Name = "User", PasswordHash = [], PasswordSalt = [],  Email = ""};
        var workspace = new Workspace { Id = 1, Name = "Workspace"};
        var board1 = new Board { Id = 1, Name = "Shared 1", Users = new List<User> { user }, Workspace = workspace };
        var board2 = new Board { Id = 2, Name = "Shared 2", Users = new List<User> { user }, Workspace = workspace };
        var board3 = new Board { Id = 3, Name = "Not shared" };

        context.Users.Add(user);
        context.Boards.AddRange(board1, board2, board3);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var result = await service.GetGuestBoardsAsync(1, 2 );

        Assert.Equal(2, result.Count);
        Assert.All(result, b => Assert.Contains("Shared", b.Name));
    }

    [Fact]
    public async Task GetGuestBoardsAsync_ReturnsEmpty_WhenNoAccess()
    {
        var context = CreateInMemoryContext();
        var workspace = new Workspace { Id = 1, Name = "Workspace"};
        var board = new Board { Id = 1, Name = "Board", Users = new List<User>(), Workspace = workspace};
        context.Users.Add(new User { Id = 2, Name = "User", PasswordHash = [], PasswordSalt = [], Email = ""});
        context.Boards.Add(board);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var result = await service.GetGuestBoardsAsync(1, 5);

        Assert.Empty(result);
    }

    [Fact]
    public async Task CleanupArchivedBoardsAsync_RemovesBoardsArchivedOver7DaysAgo()
    {
        var context = CreateInMemoryContext();
        context.Boards.AddRange(
            new Board { Id = 1, Name = "Old Archived", ArchivedAt = DateTime.UtcNow.AddDays(-31) },
            new Board { Id = 2, Name = "Recent Archived", ArchivedAt = DateTime.UtcNow.AddDays(-3) },
            new Board { Id = 3, Name = "Active", ArchivedAt = null }
        );
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var source = new CancellationTokenSource();
        await service.CleanupArchivedBoardsAsync(source.Token);

        var remaining = await context.Boards.ToListAsync();
        Assert.Equal(2, remaining.Count);
        Assert.DoesNotContain(remaining, b => b.Name == "Old Archived");
    }

    [Fact]
    public async Task CleanupArchivedBoardsAsync_DoesNothing_IfNoExpiredBoards()
    {
        var context = CreateInMemoryContext();
        context.Boards.Add(new Board { Id = 10, Name = "Archived 6d", ArchivedAt = DateTime.UtcNow.AddDays(-6) });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        
        var source = new CancellationTokenSource();
        await service.CleanupArchivedBoardsAsync(source.Token);

        var boards = await context.Boards.ToListAsync();
        Assert.Single(boards);
        Assert.Equal("Archived 6d", boards[0].Name);
    }
}