using System.Text;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Fragmenta.Api.Contracts;
using Fragmenta.Api.Controllers;
using Fragmenta.Api.Services;
using Fragmenta.Api.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Fragmenta.Dal;
using Fragmenta.Dal.Models;
using Fragmenta.Tests.Fakes; // заміни на свій namespace
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace Fragmenta.Tests.IntegrationTests
{
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["MigrateDatabaseOnStartup"] = "false",
                    ["UseMsSql"] = "false",
                });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ApplicationContext>();
                services.RemoveAll<DbContextOptions<ApplicationContext>>();
                services.AddDbContext<ApplicationContext>(options => { options.UseInMemoryDatabase("TestDb"); });
                
                services.RemoveAll<IEmailHttpClient>();
                services.AddScoped<IEmailHttpClient, FakeEmailHttpClient>();

                services.RemoveAll<IBlobClientFactory>();
                var mockBlobClient = new Mock<BlobClient>();
                mockBlobClient
                    .Setup(b => b.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Mock.Of<Response<BlobContentInfo>>());
                mockBlobClient
                    .Setup(b => b.DownloadAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Response.FromValue(
                        BlobsModelFactory.BlobDownloadInfo(
                            content: new MemoryStream(Encoding.UTF8.GetBytes("fake content"))),
                        Mock.Of<Response>()));

                services.AddSingleton<IBlobClientFactory>(new FakeBlobClientFactory(mockBlobClient.Object));
                
                services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = "Test";
                        options.DefaultChallengeScheme = "Test";
                        options.DefaultScheme = "Test";
                    })
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
                
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ApplicationContext>();
                var passwordHasher = scopedServices.GetRequiredService<IHashingService>();

                db.Database.EnsureCreated();
                SeedTestData(db, passwordHasher);
            });
        }


        private void SeedTestData(ApplicationContext context, IHashingService hasher)
        {
            var salt = SaltGenerator.GenerateSalt();

            context.Users.RemoveRange(context.Users);

            context.Users.AddRange(new User
                {
                    Id = 1,
                    Name = "testuser1",
                    Email = "test1@example.com",
                    PasswordHash = hasher.Hash("Password1234", salt),
                    PasswordSalt = salt
                },
                new User
                {
                    Id = 2,
                    Name = "testuser2",
                    Email = "test2@example.com",
                    PasswordHash = hasher.Hash("Password1234", salt),
                    PasswordSalt = salt
                },
                new User
                {
                    Id = 3,
                    Name = "testuser3",
                    Email = "test3@example.com",
                    PasswordHash = hasher.Hash("Password1234", salt),
                    PasswordSalt = salt
                },
                new User
                {
                    Id = 4,
                    Name = "testuser4",
                    Email = "test4@example.com",
                    PasswordHash = hasher.Hash("Password1234", salt),
                    PasswordSalt = salt
                },
                new User
                {
                    Id = 5,
                    Name = "testuser5",
                    Email = "test5@example.com",
                    PasswordHash = hasher.Hash("Password1234", salt),
                    PasswordSalt = salt
                },
                new User
                {
                    Id = 6,
                    Name = "testuser6",
                    Email = "test6@example.com",
                    PasswordHash = hasher.Hash("Password1234", salt),
                    PasswordSalt = salt
                });

            context.WorkspaceAccesses.AddRange(new WorkspaceAccess()
                {
                    Workspace = new Workspace() { Name = "Workspace 1", Id = 1 },
                    RoleId = 1,
                    UserId = 1
                }, new WorkspaceAccess()
                {
                    Workspace = new Workspace() { Name = "Workspace 2", Id = 2 },
                    RoleId = 1,
                    UserId = 2
                },
                new WorkspaceAccess()
                {
                    Workspace = new Workspace() { Name = "Workspace 3", Id = 3 },
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
            context.BoardAccesses.Add(new BoardAccess() { BoardId = 1, UserId = 6 });
            context.ResetTokens.Add(new ResetToken()
            {
                UserId = 2, Id = 10,
                TokenHash = hasher.Hash("valid-reset-token"), CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            });

            context.Boards.AddRange(
                new Board()
                {
                    Name = "Board",
                    AttachmentTypes = [],
                    WorkspaceId = 1,
                    Id = 1,
                    Statuses =
                    [
                        new Status()
                        {
                            Id = 1,
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
                    Id = 2,
                    Statuses =
                    [
                        new Status()
                        {
                            Id = 2,
                            ColorHex = "#FFFFFF",
                            TaskLimit = 0,
                            Weight = 0,
                            Name = "Status 1"
                        },
                        new Status()
                        {
                            Id = 3,
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

            context.SaveChanges();
        }
    }
}