using System.Text;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Fragmenta.Api.Contracts;
using Fragmenta.Api.Controllers;
using Fragmenta.Api.Services;
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

namespace Fragmenta.Api.Tests.IntegrationTests
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
                    ["MigrateDatabaseOnStartup"] = "false"
                });
            });
            
            builder.ConfigureServices(services =>
            {
                var contextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ApplicationContext));
                if (contextDescriptor != null)
                    services.Remove(contextDescriptor);
            
                var optionsDescriptor = services.SingleOrDefault(d => 
                    d.ServiceType == typeof(DbContextOptions<ApplicationContext>));
                if (optionsDescriptor != null)
                    services.Remove(optionsDescriptor);


                services.RemoveAll<IUserAccountService>();
                services.RemoveAll<IWorkspaceService>();
                services.RemoveAll<IWorkspaceAccessService>();
                services.RemoveAll<IRefreshTokenService>();
                services.RemoveAll<IBoardService>();
                services.RemoveAll<IStatusService>();
                services.RemoveAll<ITaskService>();
                services.RemoveAll<ITagService>();
                services.RemoveAll<IResetTokenService>();
                services.RemoveAll<IMailingService>();
                services.RemoveAll<IAttachmentService>();
                services.RemoveAll<IBoardAccessService>();
                services.RemoveAll<IAuthService>();
                services.RemoveAll<IUserLookupService>();
                services.RemoveAll<IRefreshTokenLookupService>();
                services.RemoveAll<IEmailHttpClient>();

                services.RemoveAll<IHostedService>(); // Видалити HostedService, інакше BoardCleanupBackgroundService буде валити через старий контекст
                
// Реєструємо сервіси заново
                services.AddScoped<IUserAccountService, UserAccountService>();
                services.AddScoped<IWorkspaceService, WorkspaceService>();
                services.AddScoped<IWorkspaceAccessService, WorkspaceAccessService>();
                services.AddScoped<IRefreshTokenService, RefreshTokenService>();
                services.AddScoped<IBoardService, BoardService>();
                services.AddScoped<IStatusService, StatusService>();
                services.AddScoped<ITaskService, TaskService>();
                services.AddScoped<ITagService, TagService>();
                services.AddScoped<IResetTokenService, ResetTokenService>();
                services.AddScoped<IMailingService, MailingService>();
                services.AddScoped<IAttachmentService, AttachmentService>();
                services.AddScoped<IBoardAccessService, BoardAccessService>();
                services.AddScoped<IAuthService, AuthService>();
                services.AddScoped<IUserLookupService, UserLookupService>();
                services.AddScoped<IRefreshTokenLookupService, RefreshTokenLookupService>();

                // Додати фейкові сервіси
                services.AddScoped<IEmailHttpClient, FakeEmailHttpClient>();
                
                var blobFactoryDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IBlobClientFactory));
                if (blobFactoryDescriptor != null)
                {
                    services.Remove(blobFactoryDescriptor);
                }

                // Створити моки
                var mockBlobClient = new Mock<BlobClient>();
                mockBlobClient
                    .Setup(b => b.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(Mock.Of<Response<BlobContentInfo>>()));

                mockBlobClient
                    .Setup(b => b.DownloadAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Response.FromValue(
                        BlobsModelFactory.BlobDownloadInfo(content: new MemoryStream(Encoding.UTF8.GetBytes("fake content"))),
                        Mock.Of<Response>()));

                // Реєстрація фейкового IBlobClientFactory
                services.AddSingleton<IBlobClientFactory>(new FakeBlobClientFactory(mockBlobClient.Object));

                // Реєструємо InMemory БД
                services.AddDbContext<ApplicationContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });
                
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<ApplicationContext>();
                
                    db.Database.EnsureCreated();
                    SeedTestData(db);
                }

                // Підміна автентифікації
                services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = "Test";
                        options.DefaultChallengeScheme = "Test";
                        options.DefaultScheme = "Test";
                    })
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
            });
        }
        
        private void SeedTestData(ApplicationContext context)
        {
            // Clear existing data
            context.Users.RemoveRange(context.Users);
        
            // Add test data
            context.Users.Add(new User
            {
                Id = 1, 
                Name = "testuser",
                Email = "test@example.com",
                PasswordHash = new byte[] { 1, 2, 3 },
                PasswordSalt = new byte[] { 4, 5, 6 }
            });
        
            context.SaveChanges();
        }
        
    }
}