using Fragmenta.Api.Contracts;
using Fragmenta.Api.Utils;
using Fragmenta.Dal;
using Fragmenta.Dal.Models;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace Fragmenta.Api.Services;

public class DatabaseSeeder
{
    private readonly ApplicationContext _context;
    private readonly IConfiguration _configuration;
    private readonly IHashingService _hasher;

    public DatabaseSeeder(ApplicationContext context, IConfiguration configuration, IHashingService hasher)
    {
        _context = context;
        _configuration = configuration;
        _hasher = hasher;
    }

    public async Task SeedIfNeededAsync()
    {
        bool shouldSeed = _configuration.GetValue<bool>("DatabaseOptions:SeedTestData");
        
        if (!shouldSeed)
            return;
            
        // Check if already seeded
        if (await _context.Users.AnyAsync())
            return;
            
        // Create test users with hashed passwords
        await SeedUsersAsync();
        
        // Add other seed methods
    }
    
    private async Task SeedUsersAsync()
{
    var salt = SaltGenerator.GenerateSalt();

    _context.Users.RemoveRange(_context.Users);
    await _context.SaveChangesAsync();
    
    await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT('[Users]', RESEED, 0)");
    await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT('[Workspaces]', RESEED, 0)");
    await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT('[Boards]', RESEED, 0)");

    _context.Users.AddRange(new User
        {
            Name = "testuser1",
            Email = "test1@example.com",
            PasswordHash = _hasher.Hash("Password1234", salt),
            PasswordSalt = salt
        },
        new User
        {
            Name = "testuser2",
            Email = "test2@example.com",
            PasswordHash = _hasher.Hash("Password1234", salt),
            PasswordSalt = salt
        },
        new User
        {
            Name = "testuser3",
            Email = "test3@example.com",
            PasswordHash = _hasher.Hash("Password1234", salt),
            PasswordSalt = salt
        },
        new User
        {
            Name = "testuser4",
            Email = "test4@example.com",
            PasswordHash = _hasher.Hash("Password1234", salt),
            PasswordSalt = salt
        },
        new User
        {
            Name = "testuser5",
            Email = "test5@example.com",
            PasswordHash = _hasher.Hash("Password1234", salt),
            PasswordSalt = salt
        },
        new User
        {
            Name = "testuser6",
            Email = "test6@example.com",
            PasswordHash = _hasher.Hash("Password1234", salt),
            PasswordSalt = salt
        });

    await _context.SaveChangesAsync();

    _context.Workspaces.AddRange(
        new Workspace() { Name = "Workspace 1"},
        new Workspace() { Name = "Workspace 2" },
        new Workspace() { Name = "Workspace 3" }
    );
    
    await _context.SaveChangesAsync();
    
    _context.WorkspaceAccesses.AddRange(new WorkspaceAccess()
        {
            WorkspaceId = 2,
            RoleId = 1,
            UserId = 1
        }, new WorkspaceAccess()
        {
            WorkspaceId = 2,
            RoleId = 1,
            UserId = 2
        },
        new WorkspaceAccess()
        {
            WorkspaceId = 3,
            RoleId = 3,
            UserId = 1
        }, new WorkspaceAccess()
        {
            WorkspaceId = 1,
            RoleId = 2,
            UserId = 2
        }, new WorkspaceAccess()
        {
            WorkspaceId = 1,
            RoleId = 2,
            UserId = 5
        }, new WorkspaceAccess()
        {
            WorkspaceId = 1,
            RoleId = 3,
            UserId = 3
        }, new WorkspaceAccess()
        {
            WorkspaceId = 2,
            RoleId = 4,
            UserId = 3
        }, new WorkspaceAccess()
        {
            WorkspaceId = 1,
            RoleId = 4,
            UserId = 6
        });
    
    await _context.SaveChangesAsync();

    _context.Boards.AddRange(
        new Board()
        {
            Name = "Board",
            AttachmentTypes = [],
            WorkspaceId = 1,
            Statuses =
            [
                new Status()
                {
                    ColorHex = "#FFFFFF",
                    TaskLimit = 0,
                    Weight = 0,
                    Name = "Status"
                }
            ],
            Tags =
            [
                new Tag()
                {
                    Name = "design",
                    Tasks =
                    [
                        new()
                        {
                            StatusId = 1,
                            AssigneeId = 1,
                            Title = "Task 2",
                            Priority = 0
                        },
                        new()
                        {
                            StatusId = 1,
                            AssigneeId = null,
                            Title = "Task 2",
                            Priority = 0
                        }
                    ]
                }
            ]
        },
        new Board()
        {
            Name = "Board",
            AttachmentTypes = [],
            WorkspaceId = 2,
            Statuses =
            [
                new Status()
                {
                    ColorHex = "#FFFFFF",
                    TaskLimit = 0,
                    Weight = 0,
                    Name = "Status 1"
                },
                new Status()
                {
                    ColorHex = "#FAF",
                    TaskLimit = 0,
                    Weight = 10,
                    Name = "Status 2"
                }
            ],
            Tags =
            [
                new Tag()
                {
                    Name = "QA",
                    Tasks =
                    [
                        new()
                        {
                            StatusId = 2,
                            Title = "Task Board 2",
                            Priority = 0
                        }
                    ]
                }
            ]
        }, new Board()
        {
            Name = "Board",
            AttachmentTypes = [],
            WorkspaceId = 1,
            Id = 4,
            ArchivedAt = DateTime.UtcNow.AddDays(-1)
        });

    await _context.SaveChangesAsync();
    
    _context.BoardAccesses.Add(new BoardAccess() { BoardId = 1, UserId = 6 });
    
    await _context.SaveChangesAsync();
}
}