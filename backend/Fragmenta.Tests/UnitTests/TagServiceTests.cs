using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
using Fragmenta.Api.Services;
using Fragmenta.Dal.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace Fragmenta.Tests.UnitTests;

public class TagServiceTests : UnitTestsBase
{
    private readonly ILogger<TagService> _logger;

    public TagServiceTests()
    {
        _logger = new NullLogger<TagService>();
    }

    private Board GetBoard(long id) => new Board
    {
        Id = id,
        Name = Guid.NewGuid().ToString()
    };

    [Fact]
    public async Task CreateTagAsync_DuplicatedTagOnSameBoard_ReturnsNull()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new TagService(_logger, context);
        var nameDup = "Tag 1";

        var board = GetBoard(1);
        context.Boards.Add(board);
        
        context.Tags.Add(new Tag() { Id = 1, Name = nameDup, Board = board });
        await context.SaveChangesAsync();

        
        // Act
        var result = await service.CreateTagAsync(nameDup, board.Id);

        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task CreateTagAsync_DuplicatedTagOnDifferentBoards_ReturnsTag()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new TagService(_logger, context);
        var nameDup = "Tag 1";

        var board1 = GetBoard(1);
        var board2 = GetBoard(2);
        context.Boards.AddRange(board1, board2);
        
        context.Tags.Add(new Tag() { Id = 1, Name = nameDup, Board = board1 });
        await context.SaveChangesAsync();

        
        // Act
        var result = await service.CreateTagAsync(nameDup, board2.Id);

        // Assert
        Assert.NotNull(result);
    }
    
    [Fact]
    public async Task CreateTagAsync_ExistingBoard_ReturnsTag()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new TagService(_logger, context);

        var board = GetBoard(1);
        context.Boards.Add(board);
        await context.SaveChangesAsync();

        
        // Act
        var result = await service.CreateTagAsync("Tag 1", board.Id);

        // Assert
        Assert.NotNull(result);
    }
    
    [Fact]
    public async Task DeleteTagAsync_NonExistingTag_ReturnsFalse()
    {
        // Arrange
        var context = CreateInMemoryContext();
        
        var service = new TagService(_logger, context);

        // Act
        var result = await service.DeleteTagAsync(1);

        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public async Task DeleteTagAsync_ExistingTag_ReturnsTrue()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new TagService(_logger, context);
        var nameDup = "Tag 1";

        var board = GetBoard(1);
        context.Boards.Add(board);
        
        context.Tags.Add(new Tag() { Id = 1, Name = nameDup, Board = board });
        await context.SaveChangesAsync();
        
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteTagAsync(1);

        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public async Task GetTagsAsync_ExistingBoard_ReturnsTagsList()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new TagService(_logger, context);
        var board = GetBoard(1);
        context.Boards.Add(board);
        
        context.Tags.Add(new Tag() { Id = 1, Name = "Tag 1", Board = board });
        context.Tags.Add(new Tag() { Id = 2, Name = "Tag 1", Board = board });
        await context.SaveChangesAsync();

        
        // Act
        var result = await service.GetTagsAsync(board.Id);

        // Assert
        Assert.NotEmpty(result);
    }
    
    [Fact]
    public async Task GetTagsAsync_NonExistingBoard_ReturnsEmptyList()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new TagService(_logger, context);
        var board = GetBoard(1);
        context.Boards.Add(board);
        
        context.Tags.Add(new Tag() { Id = 1, Name = "Tag 1", Board = board });
        context.Tags.Add(new Tag() { Id = 2, Name = "Tag 1", Board = board });
        await context.SaveChangesAsync();

        
        // Act
        var result = await service.GetTagsAsync(42);

        // Assert
        Assert.Empty(result);
    }
   
}