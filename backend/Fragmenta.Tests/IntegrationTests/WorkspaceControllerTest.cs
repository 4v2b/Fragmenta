using System.Net;
using System.Net.Http.Json;
using Fragmenta.Api.Tests.IntegrationTests;
using Fragmenta.Dal;
using Fragmenta.Dal.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Fragmenta.Tests.IntegrationTests;

public class WorkspaceControllerTests : IClassFixture<TestWebApplicationFactory>
{
    //private readonly ApplicationContext _dbContext;
    private readonly HttpClient _client;

    public WorkspaceControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        //_dbContext = factory.Services.GetService<ApplicationContext>();
        
        /*_dbContext.Users.Add(new User
        {
            Id = 1,
            Name = "testuser",
            Email = "test@example.com",
            PasswordHash = [],
            PasswordSalt = []
        });
        
        _dbContext.SaveChanges();*/
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/workspaces");

        // Assert

        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}