using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fragmenta.Tests.IntegrationTests;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public static string UserId { get; set; } = "1";
    public static string Email { get; set; } = "test@example.com";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if there's a custom user ID in the request header
        if (Context.Request.Headers.TryGetValue("Test-UserId", out var userIdValues))
        {
            UserId = userIdValues.FirstOrDefault() ?? UserId;
        }

        if (Context.Request.Headers.TryGetValue("Test-Email", out var emailValues))
        {
            Email = emailValues.FirstOrDefault() ?? Email;
        }

        var claims = new[] {
            new Claim(ClaimTypes.NameIdentifier, UserId),
            new Claim(ClaimTypes.Email, Email)
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}