using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
using Fragmenta.Api.Services;
using Fragmenta.Dal.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace Fragmenta.Tests.UnitTests;

public class StatusServiceTests : UnitTestsBase
{
    private readonly ILogger<StatusService> _logger;

    public StatusServiceTests()
    {
        _logger = new NullLogger<StatusService>();
    }

    private CreateOrUpdateStatusRequest CreateStatus(int maxTasks = 0)
    {
        return new CreateOrUpdateStatusRequest()
        {
            ColorHex = "#FFFFFF",
            MaxTasks = maxTasks,
            Name = Guid.NewGuid().ToString(),
            Weight = 0
        };
    }

    private Board GetBoard(long id) => new Board
    {
        Id = id,
        Name = Guid.NewGuid().ToString()
    };

    [Fact]
    public async Task CreateStatusAsync_NoExistingBoard_ReturnsNull()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var status = CreateStatus();
        var service = new StatusService(_logger, context);

        // Act
        var result = await service.CreateStatusAsync(1, status);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateStatusAsync_ExistingBoard_ReturnsNewStatus()
    {
        // Arrange
        var context = CreateInMemoryContext();
        
        var status = CreateStatus();
        var board = GetBoard(1);

        context.Boards.Add(board);
        await context.SaveChangesAsync();
        
        var service = new StatusService(_logger, context);

        // Act
        var result = await service.CreateStatusAsync(1, status);

        // Assert
        Assert.NotNull(result);
    }
    
    [Fact]
    public async Task DeleteStatusAsync_NonExistingStatus_ReturnsFalse()
    {
        // Arrange
        var context = CreateInMemoryContext();
        
        var service = new StatusService(_logger, context);

        // Act
        var result = await service.DeleteStatusAsync(1);

        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public async Task DeleteStatusAsync_ExistingStatus_ReturnsTrue()
    {
        // Arrange
        var context = CreateInMemoryContext();

        var status = new Status()
        {
            ColorHex = "#FFFFFF",
            Id = 1,
            TaskLimit = 0,
            Name = "Status",
            Weight = 0
        };

        context.Statuses.Add(status);
        await context.SaveChangesAsync();
        
        var service = new StatusService(_logger, context);

        // Act
        var result = await service.DeleteStatusAsync(1);

        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public async Task UpdateStatusAsync_ExistingStatus_ReturnsUpdatedStatus()
    {
        // Arrange
        var context = CreateInMemoryContext();

        var status = new Status()
        {
            ColorHex = "#000000",
            Id = 1,
            TaskLimit = 0,
            Name = "Status",
            Weight = 0
        };

        context.Statuses.Add(status);
        await context.SaveChangesAsync();
        
        var updatedStatus = CreateStatus();
        
        var service = new StatusService(_logger, context);

        // Act
        var result = await service.UpdateStatusAsync(1, updatedStatus);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updatedStatus.ColorHex, result.ColorHex);
        Assert.Equal(updatedStatus.Name, result.Name);
    }
    
    [Fact]
    public async Task UpdateStatusAsync_NonExistingStatus_ReturnsNull()
    {
        // Arrange
        var context = CreateInMemoryContext();
        
        var updatedStatus = CreateStatus();
        
        var service = new StatusService(_logger, context);

        // Act
        var result = await service.UpdateStatusAsync(1, updatedStatus);

        // Assert
        Assert.Null(result);
    }
}