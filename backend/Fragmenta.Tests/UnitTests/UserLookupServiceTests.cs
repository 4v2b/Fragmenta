using Fragmenta.Dal;
using Fragmenta.Dal.Models;

namespace Fragmenta.Tests.UnitTests;

public class UserLookupServiceTests : UnitTestsBase
{
    private async Task<ApplicationContext> CreateInMemoryContextWithUsers()
    {
        var context = CreateInMemoryContext();
        
        context.Users.AddRange([new User
        {
            Id = 1,
            Email = "test1@example.com",
            Name = "Test 1",
            PasswordSalt = [],
            PasswordHash = [1, 2, 3, 4]
        },
        new User
        {
            Id = 2,
            Email = "test2@example.com",
            Name = "Test 2",
            PasswordSalt = [],
            PasswordHash = [1, 2, 3, 4]
        },
        new User
        {
            Id = 3,
            Email = "test3@example.com",
            Name = "Test 3",
            PasswordSalt = [],
            PasswordHash = [1, 2, 3, 4]
        },
        new User
        {
            Id = 4,
            Email = "test4@example.com",
            Name = "Test 4",
            PasswordSalt = [],
            PasswordHash = [1, 2, 3, 4]
        },
        ]);

        await context.SaveChangesAsync();
        return context;
    } 
}