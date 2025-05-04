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

        // Create test users with hashed passwords
        await SeedUsersAsync();

        // Add other seed methods
    }

    private async Task SeedUsersAsync()
    {
        var salt = SaltGenerator.GenerateSalt();

        await _context.Tasks.ForEachAsync(t => t.Assignee = null);
        
        await _context.SaveChangesAsync();
        
        
        _context.Users.RemoveRange(_context.Users);
        // STEP 1: Create and insert Users
        var users = new List<User>
        {
            new() { Name = "testuser1", Email = "test1@example.com", PasswordHash = _hasher.Hash("Password1234", salt), PasswordSalt = salt },
            new() { Name = "testuser2", Email = "test2@example.com", PasswordHash = _hasher.Hash("Password1234", salt), PasswordSalt = salt },
            new() { Name = "testuser3", Email = "test3@example.com", PasswordHash = _hasher.Hash("Password1234", salt), PasswordSalt = salt },
            new() { Name = "testuser4", Email = "test4@example.com", PasswordHash = _hasher.Hash("Password1234", salt), PasswordSalt = salt },
            new() { Name = "testuser5", Email = "test5@example.com", PasswordHash = _hasher.Hash("Password1234", salt), PasswordSalt = salt },
            new() { Name = "testuser6", Email = "test6@example.com", PasswordHash = _hasher.Hash("Password1234", salt), PasswordSalt = salt },
            new() { Name = "testuser7", Email = "test7@example.com", PasswordHash = _hasher.Hash("Password1234", salt), PasswordSalt = salt },
            new() { Name = "testuser8", Email = "test8@example.com", PasswordHash = _hasher.Hash("Password1234", salt), PasswordSalt = salt },
            new() { Name = "testuser9", Email = "test9@example.com", PasswordHash = _hasher.Hash("Password1234", salt), PasswordSalt = salt },
            new() { Name = "testuser10", Email = "test10@example.com", PasswordHash = _hasher.Hash("Password1234", salt), PasswordSalt = salt },
            new() { Name = "testuser12", Email = "test12@example.com", PasswordHash = _hasher.Hash("Password1234", salt), PasswordSalt = salt },
            new() { Name = "testuser11", Email = "vitalijber2004@gmail.com", PasswordHash = _hasher.Hash("Password1234", salt), PasswordSalt = salt },
            new() { Name = "testuser13", Email = "test13@example.com", PasswordHash = _hasher.Hash("Password1234", salt), PasswordSalt = salt },
            new() { Name = "testuser14", Email = "test14@example.com", PasswordHash = _hasher.Hash("Password1234", salt), PasswordSalt = salt },
            new() { Name = "testuser15", Email = "test15@example.com", PasswordHash = _hasher.Hash("Password1234", salt), PasswordSalt = salt },
        };

        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();
        
        await using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                var users2 = new List<User>
                {
                    new() { Id = 9999, Name = "testuser9999", Email = "test9999@example.com", PasswordHash = _hasher.Hash("Password1234", salt), PasswordSalt = salt },
                    new() { Id = 10000, Name = "testuser10000", Email = "test10000@example.com", PasswordHash = _hasher.Hash("Password1234", salt), PasswordSalt = salt },
                };
                
                await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Users ON");
                await _context.Users.AddRangeAsync(users2);
                await _context.SaveChangesAsync();
                await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Users OFF");
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        
        _context.ResetTokens.Add(new ResetToken
            { ExpiresAt = DateTime.UtcNow.AddHours(1), UserId = 9999, TokenHash = _hasher.Hash("reset-token-9999") });
        _context.ResetTokens.Add(new ResetToken
            { ExpiresAt = DateTime.UtcNow.AddHours(1), UserId = 10000, TokenHash = _hasher.Hash("reset-token-10000") });
        
        await _context.SaveChangesAsync();
        
        _context.Workspaces.RemoveRange(_context.Workspaces);

        // STEP 2: Create and insert Workspaces
        var workspaces = new List<Workspace>
        {
            new() { Name = "Workspace 1" },
            new() { Name = "Workspace 2" },
            new() { Name = "Workspace 3" }
        };

        _context.Workspaces.AddRange(workspaces);
        await _context.SaveChangesAsync();
        
        var accesses = new List<WorkspaceAccess>
        {
            new() { Workspace = workspaces[1], RoleId = 1, User = users[0] }, // Workspace 2 user1 owner
            new() { Workspace = workspaces[2], RoleId = 1, User = users[0] }, 
            new() { Workspace = workspaces[1], RoleId = 2, User = users[1] }, // Workspace 2 user2 admin
            new() { Workspace = workspaces[1], RoleId = 2, User = users[4] }, // Workspace 2 user5 admin
            new() { Workspace = workspaces[1], RoleId = 3, User = users[2] }, // Workspace 2 user3 member
            new() { Workspace = workspaces[1], RoleId = 4, User = users[5] }, // Workspace 2 user6 guest
            new() { Workspace = workspaces[1], RoleId = 4, User = users[6] }, // Workspace 2 user7 guest
            //new() { Workspace = workspaces[2], RoleId = 3, User = users[6] }, 
            new() { Workspace = workspaces[1], RoleId = 3, User = users[7] }, // Workspace 2 user8 member
            new() { Workspace = workspaces[1], RoleId = 3, User = users[10] }, // Workspace 2 user12 member
            new() { Workspace = workspaces[1], RoleId = 4, User = users[8] }, // Workspace 2 user9 guest
            new() { Workspace = workspaces[1], RoleId = 3, User = users[12] }, // Workspace 2 user13 member
            new() { Workspace = workspaces[1], RoleId = 2, User = users[13] }, // Workspace 2 user14 admin
            new() { Workspace = workspaces[2], RoleId = 1, User = users[14] }, // Workspace 3 user15 owner
        };

        _context.WorkspaceAccesses.AddRange(accesses);
        await _context.SaveChangesAsync();

        var board1Status = new Status { Name = "Status", ColorHex = "#FFFFFF", TaskLimit = 0, Weight = 0 };
        var board1Tasks = new List<Dal.Models.Task>
        {
            new() { Title = "Task 1", Priority = 0, Assignee = users[0], Status = board1Status },
            new() { Title = "Task 2", Priority = 0, Assignee = null, Status = board1Status },
        };
        var board1Tag = new Tag { Name = "design", Tasks = board1Tasks };

        var board1 = new Board
        {
            Name = "Board",
            AttachmentTypes = [],
            Workspace = workspaces[0],
            Statuses = [ board1Status ],
            Tags = [ board1Tag ],
        };

        var board2Status1 = new Status { Name = "Status 1", ColorHex = "#EABFFF", TaskLimit = 0, Weight = 50 };
        var board2Status2 = new Status { Name = "Status 2", ColorHex = "#FAF", TaskLimit = 0, Weight = 100 };
        var board2Status3 = new Status { Name = "Status 3", ColorHex = "#667788", TaskLimit = 0, Weight = 200 };
        var board2Task1 = new Dal.Models.Task { Title = "Task 1", Priority = 0, Status = board2Status2, Weight = 0};
        var board2Task2 = new Dal.Models.Task { Title = "Task 2", Priority = 0, Status = board2Status2 , Weight = 100};
        var board2Task5 = new Dal.Models.Task { Title = "Task 5", Priority = 0, Status = board2Status2 , Weight = 150};
        var board2Task3 = new Dal.Models.Task { Title = "Task 3", Priority = 0, Assignee = users[1], Status = board2Status1, Weight = 0};
        var board2Task4 = new Dal.Models.Task { Title = "Task 4", Priority = 0, Status = board2Status1 , Weight = 100};
        var board2Tag = new Tag { Name = "QA", Tasks = [ board2Task1, board2Task2, board2Task3, board2Task4, board2Task5 ] };

        //await _context.Tasks.AddRangeAsync(board2Task1, board2Task2, board2Task3);
        //await _context.SaveChangesAsync();
        
        var type = await _context.AttachmentTypes.FirstOrDefaultAsync(a => a.Value == ".txt");
        
        var board2 = new Board
        {
            Name = "Board",
            AttachmentTypes = [type],
            Workspace = workspaces[1],
            Statuses = [ board2Status1, board2Status2, board2Status3 ],
            Tags = [ board2Tag ],
        };

        var board3 = new Board
        {
            Name = "Board",
            AttachmentTypes = [],
            Workspace = workspaces[0],
            ArchivedAt = DateTime.UtcNow.AddDays(-1)
        };
        
        var board4 = new Board
        {
            Name = "Board 1",
            AttachmentTypes = [],
            Workspace = workspaces[1],
            Statuses = [],
            Tags = [],
        };
        
        var board5 = new Board
        {
            Name = "Board 2",
            AttachmentTypes = [],
            Workspace = workspaces[1],
            Statuses = [],
            Tags = [],
            ArchivedAt = DateTime.UtcNow.AddDays(-1)
        };
        
        var board6 = new Board
        {
            Name = "Board 3",
            AttachmentTypes = [],
            Workspace = workspaces[1],
            Statuses = [],
            Tags = [],
            ArchivedAt = DateTime.UtcNow.AddDays(-1)
        };
        
        var board7 = new Board
        {
            Name = "Board Archived",
            AttachmentTypes = [],
            Workspace = workspaces[2],
            Statuses = [],
            Tags = [],
            ArchivedAt = DateTime.UtcNow.AddDays(-1)
        };

        _context.Boards.AddRange(board1, board2, board3, board4, board5, board6, board7);
        await _context.SaveChangesAsync();
        
        _context.BoardAccesses.AddRange(
            new BoardAccess { Board = board1, User = users[5] },
            //new BoardAccess { Board = board2, User = users[2] },
            new BoardAccess { Board = board2, User = users[6] },
            new BoardAccess { Board = board2, User = users[8] });
        await _context.SaveChangesAsync();

    }
}