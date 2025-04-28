using System.ComponentModel;
using System.Net;
using Azure;
using Azure.Storage.Blobs;
using Fragmenta.Api.Configuration;
using Fragmenta.Api.Contracts;
using Fragmenta.Api.Utils;
using Fragmenta.Dal;
using Fragmenta.Dal.Models;
using Fragmenta.Tests.Fakes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.Azurite;
using Testcontainers.MsSql;
using WireMock.RequestBuilders;
using WireMock.Server;
using Response = WireMock.ResponseBuilders.Response;
using Task = System.Threading.Tasks.Task;

namespace Fragmenta.Tests.IntegrationTests;

public class TestWebApplicationFactoryContainers : WebApplicationFactory<Program>, IAsyncLifetime
{
    private WireMockServer? _wireMockServer;
    private readonly AzuriteContainer _azuriteContainer = new AzuriteBuilder().Build();

    public async Task InitializeAsync()
    {
        await _azuriteContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _azuriteContainer.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<ApplicationContext>();
            
            _wireMockServer = WireMockServer.Start(8088);

            _wireMockServer.Given(
                    Request.Create()
                        .WithPath("/api/send")
                        .UsingPost()
                )
                .RespondWith(
                    Response.Create()
                        .WithStatusCode(200)
                );

            services.PostConfigure<SmtpOptions>(options => { options.RequestUrl = "http://localhost:8088/api/send"; });

            services.RemoveAll<IEmailHttpClient>();
            services.AddScoped<IEmailHttpClient, FakeEmailHttpClient>();

            // Replace in-memory DB with SQL Server
            services.RemoveAll<DbContextOptions<ApplicationContext>>();
            services.AddDbContext<ApplicationContext>(options => { options.UseInMemoryDatabase("TestDb"); });

            // Use real Azurite for blob storage
            services.RemoveAll<BlobServiceClient>();
            services.AddSingleton(sp =>
            {
                var connectionString = _azuriteContainer.GetConnectionString();
                var options = new BlobClientOptions(BlobClientOptions.ServiceVersion.V2023_01_03);
                return new BlobServiceClient(connectionString, options);
            });

            // Seed test data
            var sp = services.BuildServiceProvider();
            var passwordHasher = sp.GetRequiredService<IHashingService>();

            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ApplicationContext>();

                db.Database.EnsureCreated();
                SeedTestData(db, passwordHasher);
            }

            // Auth setup remains the same
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                    options.DefaultScheme = "Test";
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
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
                AttachmentTypes = [new AttachmentType() { Value = ".txt", Id = 1001, Parent = null }],
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
                                Title = "Task",
                                Priority = 0,
                                Attachments =
                                [
                                    /*new Attachment
                                    {
                                        Id = 1,
                                        FileName = "1",
                                        OriginalName = "test1.txt",
                                        TaskId = 1,
                                        SizeBytes = 1000,
                                        TypeId = 1001
                                    },
                                    new Attachment
                                    {
                                        Id = 2,
                                        FileName = "2",
                                        OriginalName = "test2.txt",
                                        TaskId = 1,
                                        SizeBytes = 1000,
                                        TypeId = 1001
                                    }*/
                                ]
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
            });

        context.SaveChanges();
    }
}