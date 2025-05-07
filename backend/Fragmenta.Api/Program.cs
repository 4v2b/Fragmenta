using Fragmenta.Api.Configuration;
using Fragmenta.Api.Contracts;
using Fragmenta.Api.Services;
using Fragmenta.Dal;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog.Events;
using Serilog;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fragmenta.Api.Controllers;
using Fragmenta.Api.Middleware;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(options: new WebApplicationOptions()
{
    EnvironmentName = "Staging"
});

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File("log-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1",
        new OpenApiInfo
        {
            Title = "Task management app API",
            Description = "An ASP.NET Core Web API for managing workspaces, boards and tasks", Version = "v1"
        });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            []
        }
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

builder.Services.AddDbContext<ApplicationContext>(
    options =>
    {
        var isTesting = builder.Environment.IsEnvironment("Testing");
        if (!isTesting || (isTesting && builder.Configuration.GetValue<bool>("DatabaseOptions:UseMsSql")))
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        }
    }
);

var jwtOptionsSection = builder.Configuration.GetRequiredSection("Jwt");

builder.Services.Configure<JwtOptions>(jwtOptionsSection);

builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.Configure<AzureStorageOptions>(builder.Configuration.GetSection("AzureStorage"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(jwtOptions =>
{
    var configKey = jwtOptionsSection["Key"];
    var key = Encoding.UTF8.GetBytes(configKey);

    jwtOptions.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = jwtOptionsSection["Issuer"],
        ValidAudience = jwtOptionsSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
});

var origins = builder.Configuration.GetSection("AllowedCorsOrigins").Get<string[]>() ?? [];

builder.WebHost.UseUrls("http://0.0.0.0:7241");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder => builder.WithOrigins(origins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

builder.Services.Configure<AzureStorageOptions>(builder.Configuration.GetSection("AzureStorage"));

builder.Services.AddSingleton<BlobServiceClient>(sp =>
{
    var connectionString = sp.GetRequiredService<IOptions<AzureStorageOptions>>().Value.ConnectionString;
    var options = new BlobClientOptions(BlobClientOptions.ServiceVersion.V2023_01_03);
    return new BlobServiceClient(connectionString, options);
});

builder.Services.AddScoped<WorkspaceFilter>();

builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserAccountService, UserAccountService>();
builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();
builder.Services.AddScoped<IWorkspaceAccessService, WorkspaceAccessService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IBoardService, BoardService>();
builder.Services.AddScoped<IStatusService, StatusService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IResetTokenService, ResetTokenService>();
builder.Services.AddScoped<IMailingService, MailingService>();
builder.Services.AddScoped<IAttachmentService, AttachmentService>();
builder.Services.AddScoped<IBoardAccessService, BoardAccessService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserLookupService, UserLookupService>();

builder.Services.AddScoped<IRefreshTokenLookupService, RefreshTokenLookupService>();

builder.Services.AddMemoryCache();
builder.Services.AddSignalR();
builder.Services.AddHttpClient();

builder.Services.AddSingleton<IEmailHttpClient, EmailHttpClient>();
builder.Services.AddSingleton<IBlobClientFactory, BlobClientFactory>();
builder.Services.AddSingleton<IHashingService, Sha256HashingService>();
builder.Services.AddHostedService<BoardCleanupBackgroundService>();

builder.Services.AddScoped<DatabaseSeeder>();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

var app = builder.Build();

var migrate = builder.Configuration.GetValue<bool>("DatabaseOptions:MigrateDatabaseOnStartup");


using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();

if (migrate)
{
    context.Database.Migrate();
}

await seeder.SeedIfNeededAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var supportedCultures = new[] { "en-US", "uk" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("en-US")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

app.UseCors("AllowReactApp");

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHub<BoardHub>("/hub/board");

app.Run();

public partial class Program
{
}