using Fragmenta.Dal;
using Microsoft.EntityFrameworkCore;

namespace Fragmenta.Tests;

public class UnitTestsBase
{
    protected ApplicationContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}