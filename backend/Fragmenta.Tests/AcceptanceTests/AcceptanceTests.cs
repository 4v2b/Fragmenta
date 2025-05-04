using Microsoft.Playwright;

namespace Fragmenta.Tests.AcceptanceTests;

public class AcceptanceTests : IAsyncLifetime
{
    private IPlaywright _playwright;
    private IBrowser _browser;
    private IPage _page;
    private static readonly string UrlBase = "http://localhost:5173";

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false,
            SlowMo = 500
        });
        _page = await _browser.NewPageAsync();
    }
    
    public async Task DisposeAsync()
    {
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    private async Task LogoutAsync()
    {
        var logout = "//button[contains(@class, 'chakra-button css-10ap741')]";

        if (await _page.ElementExists(logout))
        {
            await _page.ClickAsync(logout);
        }
    }

    private async Task LoginAsync(string email, string password)
    {
        await _page.FillAsync("//input[contains(@class, 'chakra-input css-eiee9d') and @type='email']", email);
        await _page.FillAsync("//input[contains(@class, 'chakra-input css-eiee9d') and @type='password']", password);

        await _page.ClickAsync("//div[contains(@class, 'css-3fisqh')]//button[@type='submit']");
    }

    private async Task RegisterAsync(string name, string email, string password, string repeatPassword)
    {
        await _page.FillAsync("//input[contains(@class, 'chakra-input css-eiee9d') and @type='email']", email);
        await _page.FillAsync("//input[contains(@class, 'chakra-input css-eiee9d') and @type='password']", password);
        await _page.FillAsync("//input[not(@type)]", name);
        await _page.FillAsync("//p[contains(text(), 'Repeat') or contains(text(), 'Повтор')]/following-sibling::input",
            repeatPassword);

        await _page.ClickAsync("//div[contains(@class, 'css-3fisqh')]//button[@type='submit']");
    }

    
    
}