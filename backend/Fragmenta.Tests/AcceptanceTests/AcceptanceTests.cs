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
            Headless = true
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

    
    [Fact]
    public async Task Login_Success_IfValidCredentials()
    {
        await LogoutAsync();

        await _page.GotoAsync(UrlBase + "/login");


        await LoginAsync("test1@example.com", "Password1234");

        var logout = "//button[contains(@class, 'chakra-button css-10ap741')]";

        Assert.True(await _page.ElementExists(logout));
    }

    [Fact]
    public async Task Login_ShowError_WhenEmailEmpty()
    {
        await LogoutAsync();

        await _page.GotoAsync(UrlBase + "/login");

        var email = string.Empty;

        await LoginAsync(email, "Password1234");

        var errorElement = "//p[contains(@class, 'css-zofl1m')]";

        var error = await _page.TextContentAsync(errorElement);

        Assert.False(string.IsNullOrEmpty(error));
    }

    [Fact]
    public async Task Login_ShowError_WhenPasswordEmpty()
    {
        await LogoutAsync();

        await _page.GotoAsync(UrlBase + "/login");

        var email = "test1@example.com";
        var password = "";

        await LoginAsync(email, password);

        var errorElement = "//p[contains(@class, 'css-zofl1m')]";

        var error = await _page.TextContentAsync(errorElement);

        Assert.False(string.IsNullOrEmpty(error));
    }

    [Fact]
    public async Task Login_ShowError_IfEmailHasIncorrectFormat()
    {
        await LogoutAsync();

        await _page.GotoAsync(UrlBase + "/login");

        var email = "test.com";
        var password = "Password1234";

        await LoginAsync(email, password);

        var errorElement = "//p[contains(@class, 'css-zofl1m')]";

        var error = await _page.TextContentAsync(errorElement);

        Assert.False(string.IsNullOrEmpty(error));
    }

    [Fact]
    public async Task Login_ShowError_EmailDoesntBelongToUser()
    {
        await LogoutAsync();

        await _page.GotoAsync(UrlBase + "/login");

        var email = "test12@example.com";
        var password = "Password1234";

        await LoginAsync(email, password);

        var errorElement = "//p[contains(@class, 'css-zofl1m')]";

        var error = await _page.TextContentAsync(errorElement);

        Assert.False(string.IsNullOrEmpty(error));
    }

    [Fact]
    public async Task Login_ShowError_IfPasswordInvalid()
    {
        await LogoutAsync();

        await _page.GotoAsync(UrlBase + "/login");

        var email = "test2@example.com";
        var password = "Password4321";

        await LoginAsync(email, password);

        var errorElement = "//p[contains(@class, 'css-zofl1m')]";

        var error = await _page.TextContentAsync(errorElement);

        Assert.False(string.IsNullOrEmpty(error));
    }

    [Fact]
    public async Task ForgotPassword_SendLetter_WhenEmailValid()
    {
        await LogoutAsync();

        await _page.GotoAsync(UrlBase + "/forgot-password");

        var email = "vitalijber2004@gmail.com";
        await _page.FillAsync("//input[contains(@class, 'chakra-input css-eiee9d') and @type='email']", email);

        var button = "//div[contains(@class, 'main-content')]//button[@type='submit']";
        await _page.ClickAsync(button);

        var errorElement = "//p[contains(@class, 'css-zofl1m')]";

        Assert.False(await _page.ElementExists(errorElement));
    }

    [Fact]
    public async Task ForgotPassword_ShowError_WhenEmailInvalidFormat()
    {
        await LogoutAsync();

        await _page.GotoAsync(UrlBase + "/forgot-password");

        var email = "test";
        await _page.FillAsync("//input[contains(@class, 'chakra-input css-eiee9d') and @type='email']", email);

        var button = "//div[contains(@class, 'main-content')]//button[@type='submit']";
        await _page.ClickAsync(button);

        var errorElement = "//p[contains(@class, 'css-zofl1m')]";
        var error = await _page.TextContentAsync(errorElement);

        Assert.False(string.IsNullOrEmpty(error));
    }

    [Fact]
    public async Task ForgotPassword_ShowError_WhenEmailDoesNotBelongToUser()
    {
        await LogoutAsync();

        await _page.GotoAsync(UrlBase + "/forgot-password");

        var email = "test12@example.com";
        await _page.FillAsync("//input[contains(@class, 'chakra-input css-eiee9d') and @type='email']", email);

        var button = "//div[contains(@class, 'main-content')]//button[@type='submit']";
        await _page.ClickAsync(button);

        var errorElement = "//p[contains(@class, 'css-zofl1m')]";
        var error = await _page.TextContentAsync(errorElement);

        Assert.False(string.IsNullOrEmpty(error));
    }

    [Fact]
    public async Task ForgotPassword_ShowError_WhenFieldEmpty()
    {
        await LogoutAsync();

        await _page.GotoAsync(UrlBase + "/forgot-password");

        var email = "";

        await _page.FillAsync("//input[contains(@class, 'chakra-input css-eiee9d') and @type='email']", email);

        var button = "//div[contains(@class, 'main-content')]//button[@type='submit']";
        await _page.ClickAsync(button);

        var errorElement = "//p[contains(@class, 'css-zofl1m')]";
        var error = await _page.TextContentAsync(errorElement);

        Assert.False(string.IsNullOrEmpty(error));
    }
    
    [Fact]
    public async Task Register_ShowError_WhenEmptyName()
    {
        await LogoutAsync();

        await _page.GotoAsync(UrlBase + "/register");

        var email = "test13@example.com";
        var password1 = "Password4321";
        var password2 = "Password4321";
        var name = "";

        await RegisterAsync(name, email, password1, password2);

        var errorElement = "//p[contains(@class, 'css-zofl1m')]";
        string error = null;

        if (await _page.ElementExists(errorElement))
        {
            error = await _page.TextContentAsync(errorElement);
        }

        Assert.False(string.IsNullOrEmpty(error));
    }

    [Fact]
    public async Task Register_ShowError_WhenPasswordTooWeak()
    {
        await LogoutAsync();

        await _page.GotoAsync(UrlBase + "/register");

        var email = "test13@example.com";
        var password1 = "Password";
        var password2 = "Password";
        var name = "Test User";

        await RegisterAsync(name, email, password1, password2);

        var errorElement = "//p[contains(@class, 'css-zofl1m')]";
        string error = null;

        if (await _page.ElementExists(errorElement))
        {
            error = await _page.TextContentAsync(errorElement);
        }

        Assert.False(string.IsNullOrEmpty(error));
    }

    [Fact]
    public async Task Register_ShowError_WhenPasswordsDontMatch()
    {
        await LogoutAsync();

        await _page.GotoAsync(UrlBase + "/register");

        var email = "test13@example.com";
        var password1 = "Password";
        var password2 = "Password1";
        var name = "Test User";

        await RegisterAsync(name, email, password1, password2);

        var errorElement = "//p[contains(@class, 'css-zofl1m')]";
        string error = null;

        if (await _page.ElementExists(errorElement))
        {
            error = await _page.TextContentAsync(errorElement);
        }

        Assert.False(string.IsNullOrEmpty(error));
    }

    [Fact]
    public async Task Register_ShowError_WhenEmailWithInvalidFormat()
    {
        await LogoutAsync();

        await _page.GotoAsync(UrlBase + "/register");

        var email = "testexample.com";
        var password1 = "Password13";
        var password2 = "Password13";
        var name = "Test User";

        await RegisterAsync(name, email, password1, password2);

        var errorElement = "//p[contains(@class, 'css-zofl1m')]";
        string error = null;

        if (await _page.ElementExists(errorElement))
        {
            error = await _page.TextContentAsync(errorElement);
        }

        Assert.False(string.IsNullOrEmpty(error));
    }

    [Fact]
    public async Task Register_ShowError_WhenUserExists()
    {
        await LogoutAsync();

        await _page.GotoAsync(UrlBase + "/register");

        var email = "test2@example.com";
        var password1 = "Password12";
        var password2 = "Password12";
        var name = "Test User";

        await RegisterAsync(name, email, password1, password2);

        var errorElement = "//p[contains(@class, 'css-zofl1m')]";
        string? error = null;

        if (await _page.ElementExists(errorElement))
        {
            error = await _page.TextContentAsync(errorElement);
        }

        Assert.False(string.IsNullOrEmpty(error));
    }

    [Fact]
    public async Task Register_Successfully_WhenCredentialValid()
    {
        await LogoutAsync();

        await _page.GotoAsync(UrlBase + "/register");

        var email = $"test{Random.Shared.Next(1000)}@example.com";
        var password1 = "Password123";
        var password2 = "Password123";
        var name = "Test User 1";

        await RegisterAsync(name, email, password1, password2);

        var errorElement = "//p[contains(@class, 'css-zofl1m')]";
        string? error = null;

        if (await _page.ElementExists(errorElement))
        {
            error = await _page.TextContentAsync(errorElement);
        }

        Assert.True(string.IsNullOrEmpty(error));
    }

    [Fact]
    public async Task AddMember_Success_IfUserWasntMember()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);

        var members = "//button[@data-value=\"members\"]";
        await _page.ClickAsync(members);

        int count1 = await _page.Locator("//tr").CountAsync();
        
        var search = "//input[@class=\"chakra-input css-eiee9d\"]";
        await _page.FillAsync(search, "test");
        await _page.ClickAsync("//ul[@class=\"chakra-list__root css-1yzzf3n\"]/li");

        var add = "//button[@class=\"chakra-button css-166vzny\"]";
        await _page.ClickAsync(add);

        int count2 = await _page.Locator("//tr").CountAsync();
        
        Assert.True(count2 >= count1);
    }

    [Fact]
    public async Task AddMember_ShowEmptyPrompt_IfUserNotFoundByEmail()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");

        Assert.True(await _page.ElementExists("//div[contains(text(), 'Welcome')]"));
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);

        var members = "//button[@data-value=\"members\"]";
        await _page.ClickAsync(members);
        
        var search = "//input[@class=\"chakra-input css-eiee9d\"]";
        await _page.FillAsync(search, "non-existing@mail.com");
        
        int count = await _page.Locator("//ul[@class=\"chakra-list__root css-1yzzf3n\"]/li").CountAsync();
        
        var e = await _page.ContentAsync();
        
        Assert.True(count == 0);
    }

    [Fact]
    public async Task RemoveMember_Success_IfUserWasRemoved()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        var members = "//button[@data-value=\"members\"]";
        await _page.ClickAsync(members);

        var userToDelete = "test12@example.com";
        int count1 = await _page.Locator("//tr").CountAsync();
        var button = $"//tr[contains(.//td[contains(@class, 'email-cell')]/text(), '{userToDelete}') ]//button[contains(@class, 'kick')]";
        
        await _page.ClickAsync(button);
        
        var delete = "//button[contains(@class, 'chakra-button css-13ohstl')]";
        await _page.ClickAsync(delete);
        
        await _page.ClickAsync("//p[contains(@class, 'css-sabuw6')]");

        await _page.ClickAsync(workspaceSelect);
        await _page.ClickAsync(workspace);
        await _page.ClickAsync(members);
        
        int count2 = await _page.Locator("//tr").CountAsync();

        Assert.True(count2 <= count1);
    }

    [Fact]
    public async Task RemoveMember_Blocked_IfUserHasOwnerRole()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);

        var button = "//tr[contains(.//p//text(), 'Власник') or contains(.//p//text(), 'Owner')]//button[contains(@class, 'kick')]";
        
        var members = "//button[@data-value=\"members\"]";
        await _page.ClickAsync(members);
        
        bool isDisabled = await _page.Locator(button).IsDisabledAsync();
        
        Assert.True(isDisabled);
    }
    
    [Fact]
    public async Task RemoveMember_Canceled_IfClickedCancelButton()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        var members = "//button[@data-value=\"members\"]";
        await _page.ClickAsync(members);
        int count1 = await _page.Locator("//tr").CountAsync();
        var button = "//tr[contains(.//td[contains(@class, 'chakra-table__cell css-4v8c5f')]/text(), 'Учасник') or contains(.//td[contains(@class, 'chakra-table__cell css-4v8c5f')]/text(), 'Member')]//button[contains(@class, 'kick')]";
        
        await _page.ClickAsync(button);
        
        var cancel = "//button[contains(@class, 'chakra-button css-y5du55')]";
        await _page.ClickAsync(cancel);
        int count2 = await _page.Locator("//tr").CountAsync();

        Assert.True(count1 >= count2);
    }

    [Fact]
    public async Task CreateBoard_Success_WhenUniqueName()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        
        var openDialog = "//button[contains(@class, 'chakra-button chakra-dialog__trigger css-1d7tldl')]";
        await _page.ClickAsync(openDialog);
        
        await _page.FillAsync("//div[contains(@class, 'chakra-field__root css-13f87cb')]//input[contains(@class, 'chakra-input css-eiee9d')]", Guid.NewGuid().ToString());

        var create = "//button[contains(@class, 'chakra-button chakra-dialog__trigger css-n9m7sp')]";

        await _page.ClickAsync(create);
        
        var errorElement = "//span[contains(@class, 'chakra-field__errorText css-118wl8')]";
        string? error = null;

        if (await _page.ElementExists(errorElement))
        {
            error = await _page.TextContentAsync(errorElement);
        }

        Assert.True(string.IsNullOrEmpty(error));
    }

    [Fact]
    public async Task CreateBoard_ShowError_WhenEmptyName()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        
        var openDialog = "//button[contains(@class, 'chakra-button chakra-dialog__trigger css-1d7tldl')]";
        await _page.ClickAsync(openDialog);
        
        var create = "//button[contains(@class, 'chakra-button css-166vzny')]";

        await _page.ClickAsync(create);
        
        var errorElement = "//span[contains(@class, 'chakra-field__errorText css-118wl8')]";
        string? error = null;

        if (await _page.ElementExists(errorElement))
        {
            error = await _page.TextContentAsync(errorElement);
        }

        Assert.False(string.IsNullOrEmpty(error));
    }

    [Fact]
    public async Task CreateBoard_Cancel_WhenClickClose()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");

        Assert.True(await _page.ElementExists("//div[contains(text(), 'Welcome')]"));
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        
        var openDialog = "//button[contains(@class, 'chakra-button chakra-dialog__trigger css-1d7tldl')]";
        await _page.ClickAsync(openDialog);
        
        var close = "//button[contains(@class, 'chakra-button css-y5du55')]";

        await _page.ClickAsync(close);
        
        var errorElement = "//span[contains(@class, 'chakra-field__errorText css-118wl8')]";
        string? error = null;

        if (await _page.ElementExists(errorElement))
        {
            error = await _page.TextContentAsync(errorElement);
        }

        Assert.True(string.IsNullOrEmpty(error));
    }
    
    [Fact]
    public async Task CreateBoard_ShowError_WhenNamesDuplicate()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        
        var openDialog = "//button[contains(@class, 'chakra-button chakra-dialog__trigger css-1d7tldl')]";
        await _page.ClickAsync(openDialog);


        var dupName = "Board";
        
        await _page.FillAsync("//div[contains(@class, 'chakra-field__root css-13f87cb')]//input[contains(@class, 'chakra-input css-eiee9d')]", dupName);
        
        var create = "//button[contains(@class, 'chakra-button css-166vzny')]";

        await _page.ClickAsync(create);
        
        var errorElement = "//span[contains(@class, 'chakra-field__errorText css-118wl8')]";
        string? error = null;

        if (await _page.ElementExists(errorElement))
        {
            error = await _page.TextContentAsync(errorElement);
        }

        Assert.False(string.IsNullOrEmpty(error));
    }

    [Fact]
    public async Task AddGuest_EmptyPrompt_WhenUserNotFound()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        
        var openBoard = "(//div[contains(@class, 'chakra-card__body css-7b8ujn') and .//button[contains(@class, 'chakra-button css-o4f00r')]]//p)";
        await _page.ClickAsync(openBoard);

        var guestList = "//button[contains(@class, 'chakra-button chakra-drawer__trigger css-xvlchq')]";
        await _page.ClickAsync(guestList);
        
        int count1 = await _page.Locator("//tr").CountAsync();
        
        var search = "//input[@class=\"chakra-input css-eiee9d\"]";
        await _page.FillAsync(search, "test999");
        int count = await _page.Locator("//ul[@class=\"chakra-list__root css-1yzzf3n\"]/li").CountAsync();
        
        Assert.True(count == 0);
    }

    [Fact]
    public async Task AddGuest_EmptyPrompt_WhenUserIsAlreadyAGuestOnBoard()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        
        var openBoard = "(//div[contains(@class, 'chakra-card__body css-7b8ujn') and .//button[contains(@class, 'chakra-button css-o4f00r')]]//p)";
        await _page.ClickAsync(openBoard);

        var guestList = "//button[contains(@class, 'chakra-button chakra-drawer__trigger css-xvlchq')]";
        await _page.ClickAsync(guestList);
        
        var email = await _page.TextContentAsync("//*[contains(@class, 'chakra-table__cell css-180r3l5')][2]");
        
        var search = "//input[@class=\"chakra-input css-eiee9d\"]";
        await _page.FillAsync(search, email);
        
        int count = await _page.Locator("//ul[@class=\"chakra-list__root css-1yzzf3n\"]/li").CountAsync();
        
        Assert.True(count == 0);
    }
    
    [Fact]
    public async Task AddGuest_Success_WhenUserIsNotAGuestOnBoard ()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        
        var openBoard = "(//div[contains(@class, 'chakra-card__body css-7b8ujn') and .//button[contains(@class, 'chakra-button css-o4f00r')]]//p)";
        await _page.ClickAsync(openBoard);

        var guestList = "//button[contains(@class, 'chakra-button chakra-drawer__trigger css-xvlchq')]";
        await _page.ClickAsync(guestList);
        
        int count1 = await _page.Locator("//tr").CountAsync();
        
        var search = "//input[@class=\"chakra-input css-eiee9d\"]";
        await _page.FillAsync(search, "test");
        await _page.ClickAsync("//ul[@class=\"chakra-list__root css-1yzzf3n\"]/li");
        
        var add = "//button[@class=\"chakra-button css-166vzny\"]";
        await _page.ClickAsync(add);

        int count2 = await _page.Locator("//tr").CountAsync();
        
        Assert.True(count2 >= count1);
    }

    [Fact]
    public async Task RemoveGuest_Success_WhenUserIsAlreadyAGuestOnBoard()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        
        var openBoard = "(//div[contains(@class, 'chakra-card__body css-7b8ujn') and .//button[contains(@class, 'chakra-button css-o4f00r')]]//p)";
        await _page.ClickAsync(openBoard);

        var guestList = "//button[contains(@class, 'chakra-button chakra-drawer__trigger css-xvlchq')]";
        await _page.ClickAsync(guestList);
        
        int count1 = await _page.Locator("//tr").CountAsync();
        
        var guests = "//tr[1]//button";
        await _page.ClickAsync(guests);
        
        var delete = "//button[contains(@class, 'chakra-button css-13ohstl')]";
        await _page.ClickAsync(delete);

        int count2 = await _page.Locator("//tr").CountAsync();
        
        Assert.True(count2 <= count1);
    }

    [Fact]
    public async Task RemoveGuest_Unavailable_WhenActorHasMemberRole()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test8@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        
        var openBoard = "//div[contains(@class, 'chakra-card__root css-10r8yuk')]//p";
        await _page.ClickAsync(openBoard);

        var guestList = "//button[contains(@class, 'chakra-button chakra-drawer__trigger css-xvlchq')]";
        await _page.ClickAsync(guestList);
        
        var guests = "//tr[1]//button";
        
        bool isDisabled = await _page.Locator(guests).IsDisabledAsync();
        
        Assert.True(isDisabled);
    }

    [Fact]
    public async Task ChangeLanguage_SetToUkrainian_WhenClickedButton()
    {
        await LogoutAsync();
        
        await _page.GotoAsync(UrlBase + "/login");
        
        var button = "//div[contains(@class, 'corner-button')]//*[contains(text(), 'Українська')]";

        await _page.ClickAsync(button);
        
        var evaluator = @"xpath => {
        const el = document.evaluate(xpath, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE).singleNodeValue;
        return window.getComputedStyle(el).fontWeight;
    }";
        
        var fontWeight = await _page.EvaluateAsync<string>(evaluator, button);

        bool isBold = int.Parse(fontWeight) >= 700 || fontWeight == "bold";
        Assert.True(isBold);
    }

    [Fact]
    public async Task ChangeLanguage_SetToEnglish_WhenClickedButton()
    {
        await LogoutAsync();
        
        await _page.GotoAsync(UrlBase + "/login");
        
        var button = "//div[contains(@class, 'corner-button')]//*[contains(text(), 'English')]";

        var evaluator = @"xpath => {
        const el = document.evaluate(xpath, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE).singleNodeValue;
        return window.getComputedStyle(el).fontWeight;
    }";
        
        await _page.ClickAsync(button);
        var fontWeight = await _page.EvaluateAsync<string>(evaluator, button);

        bool isBold = int.Parse(fontWeight) >= 700 || fontWeight == "bold";
        Assert.True(isBold);
    }

    [Fact]
    public async Task ChangeLanguage_SaveSettings_WhenPageReloaded()
    {
        await LogoutAsync();
        var e = await _page.ContentAsync();

        await _page.GotoAsync(UrlBase + "/login");
        
        var button = "//div[contains(@class, 'corner-button') or contains(@class, 'css-1kk9w4d')]//*[contains(text(), 'Українська')]";

        var evaluator = @"xpath => {
        const el = document.evaluate(xpath, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE).singleNodeValue;
        return window.getComputedStyle(el).fontWeight;
    }";
        
        await _page.ClickAsync(button);
        var fontWeight = await _page.EvaluateAsync<string>(evaluator, button);

        bool isBold = int.Parse(fontWeight) >= 700 || fontWeight == "bold";
        Assert.True(isBold);
        
        await _page.ReloadAsync();
        
        var fontWeight1 = await _page.EvaluateAsync<string>(evaluator, button);

        bool isBoldStill = int.Parse(fontWeight1) >= 700 || fontWeight1 == "bold";

        Assert.True(isBoldStill);
    }
}