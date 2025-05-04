using Microsoft.Playwright;

namespace Fragmenta.Tests.SystemTests;

public class SystemTests : IAsyncLifetime
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
            Headless = true,
            SlowMo = 200
        });
        _page = await _browser.NewPageAsync();
    }

    private async Task LogoutAsync()
    {
        //await _page.GotoAsync(UrlBase + "/login");
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

    public async Task DisposeAsync()
    {
        await _browser.CloseAsync();
        _playwright.Dispose();
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
    public async Task Login_ContainsEmailAndPasswordFields_WhenNavigated()
    {
        await LogoutAsync();

        await _page.GotoAsync(UrlBase + "/login");

        var email = await _page.ElementExists("//input[contains(@class, 'chakra-input css-eiee9d') and @type='email']");
        var password =
            await _page.ElementExists("//input[contains(@class, 'chakra-input css-eiee9d') and @type='password']");

        Assert.True(email);
        Assert.True(password);
    }
    

    [Fact]
    public async Task Login_ShowError_IfTooManyUnsuccessfullTries()
    {
        await LogoutAsync();

        await _page.GotoAsync(UrlBase + "/login");

        var email = "test8@example.com";
        var password = "Password4321";

        await LoginAsync(email, password);
        await LoginAsync(email, password);
        await LoginAsync(email, password);

        var errorElement = "//p[contains(@class, 'css-zofl1m')]";

        var error = await _page.TextContentAsync(errorElement);

        Assert.False(string.IsNullOrEmpty(error));
    }

    [Fact]
    public async Task Login_ShowTimer_WhenLocked()
    {
        await LogoutAsync();

        await _page.GotoAsync(UrlBase + "/login");

        var email = "test4@example.com";
        var password = "Password4321";

        await LoginAsync(email, password);
        await LoginAsync(email, password);
        await LoginAsync(email, password);

        var errorElement = "//p[contains(@class, 'css-zofl1m')]";

        var errors = await _page.GetAllTextContents(errorElement);

        var error = errors.FirstOrDefault();
        var timer = errors.LastOrDefault();

        Assert.False(string.IsNullOrEmpty(error));
        Assert.False(string.IsNullOrEmpty(timer));
    }

    [Fact]
    public async Task ForgotPassword_Navigate_WhenLabelClicked()
    {
        await LogoutAsync();

        await _page.GotoAsync(UrlBase + "/login");

        var link = "//a[contains(@href, 'forgot-password')]";

        await _page.ClickAsync(link);

        Assert.Equal(UrlBase + "/forgot-password", _page.Url);
    }

    [Fact]
    public async Task ForgotPassword_ShowError_WhenEmailSentThreeTimes()
    {
        await LogoutAsync();

        await _page.GotoAsync(UrlBase + "/forgot-password");

        var email = "vitalijber2004@gmail.com";
        await _page.FillAsync("//input[contains(@class, 'chakra-input css-eiee9d') and @type='email']", email);

        var button = "//div[contains(@class, 'main-content')]//button[@type='submit']";
        await _page.ClickAsync(button);
        await Task.Delay(500);
        await _page.ClickAsync(button);
        await Task.Delay(500);
        await _page.ClickAsync(button);

        var errorElement = "//p[contains(@class, 'css-zofl1m')]";
        var error = await _page.TextContentAsync(errorElement);

        Assert.False(string.IsNullOrEmpty(error));
    }

    [Fact]
    public async Task ForgotPassword_ShowTimer_WhenLocked()
    {
        await LogoutAsync();

        await _page.GotoAsync(UrlBase + "/forgot-password");

        var email = "vitalijber2004@gmail.com";
        await _page.FillAsync("//input[contains(@class, 'chakra-input css-eiee9d') and @type='email']", email);

        var button = "//div[contains(@class, 'main-content')]//button[@type='submit']";
        await _page.ClickAsync(button);
        await Task.Delay(500);
        await _page.ClickAsync(button);
        await Task.Delay(500);
        await _page.ClickAsync(button);

        var errorElement = "//p[contains(@class, 'css-zofl1m')]";
        var errors = await _page.GetAllTextContents(errorElement);

        var error = errors.FirstOrDefault();
        var timer = errors.LastOrDefault();

        Assert.False(string.IsNullOrEmpty(error));
        Assert.False(string.IsNullOrEmpty(timer));
        Assert.NotEqual(error, timer);
    }

    [Fact]
    public async Task ResetPassword_NavigateToLogin_WhenPasswordSuccessfullyReset()
    {
        await LogoutAsync();

        await _page.GotoAsync(UrlBase + "/reset-password?token=reset-token-9999&userId=9999");

        var password = "Password1234";

        await _page.FillAsync("//input[contains(@class, 'enter-password')]", password);
        await _page.FillAsync("//input[contains(@class, 'repeat-password')]", password);
        
        await _page.ClickAsync("//button[contains(@class, 'change-password')]");

        await Task.Delay(5100);
        
        Assert.True(_page.Url.Contains("/login"));
    }

    [Fact]
    public async Task ResetPassword_ShowError_WhenPasswordsDontMatch()
    {
        await LogoutAsync();

        await _page.GotoAsync(UrlBase + "/reset-password?token=reset-token-10000&userId=10000");

        var password = "Password1234";

        await _page.FillAsync("//input[contains(@class, 'enter-password')]", password);
        await _page.FillAsync("//input[contains(@class, 'repeat-password')]", password + "5");
        
        await _page.ClickAsync("//button[contains(@class, 'change-password')]");

        var error = "//p[contains(@class, 'reset-error')]";
        
        Assert.True(await _page.ElementExists(error));
    }

    [Fact]
    public async Task ResetPassword_ShowError_WhenEmptyPassword()
    {
        await LogoutAsync();

        await _page.GotoAsync(UrlBase + "/reset-password?token=reset-token-10000&userId=10000");
        
        await _page.ClickAsync("//button[contains(@class, 'change-password')]");

        var error = "//p[contains(@class, 'reset-error')]";
        
        Assert.True(await _page.ElementExists(error));
    }

    [Fact]
    public async Task ResetPassword_ShowError_WhenPasswordTooWeak()
    {
        await LogoutAsync();

        await _page.GotoAsync(UrlBase + "/reset-password?token=reset-token-10000&userId=10000");

        var password = "password";

        await _page.FillAsync("//input[contains(@class, 'enter-password')]", password);
        await _page.FillAsync("//input[contains(@class, 'repeat-password')]", password);
        
        await _page.ClickAsync("//button[contains(@class, 'change-password')]");

        var error = "//p[contains(@class, 'reset-error')]";
        
        Assert.True(await _page.ElementExists(error));
    }

    [Fact]
    public async Task ResetPassword_ShowError_WhenTokenInvalid()
    {
        await LogoutAsync();

        await _page.GotoAsync(UrlBase + "/reset-password?token=invalid-token&userId=10000");

        var password = "Password1234";

        await _page.FillAsync("//input[contains(@class, 'enter-password')]", password);
        await _page.FillAsync("//input[contains(@class, 'repeat-password')]", password);
        
        await _page.ClickAsync("//button[contains(@class, 'change-password')]");

        await Task.Delay(500);
        
        var error = "//p[contains(@class, 'reset-error')]";
        
        Assert.True(await _page.ElementExists(error));
    }


    [Fact]
    public async Task Login_Navigate_WhenClickedButtonOnRegisterPage()
    {
        await LogoutAsync();

        await _page.GotoAsync(UrlBase + "/register");

        var link = "//a[contains(@href, 'login')]";
        await _page.ClickAsync(link);
        Assert.Equal(_page.Url, UrlBase + "/login");
    }

    [Fact]
    public async Task Register_Navigate_WhenClickedButtonOnLoginPage()
    {
        await LogoutAsync();

        await _page.GotoAsync(UrlBase + "/login");

        var link = "//a[contains(@href, 'register')]";
        await _page.ClickAsync(link);
        Assert.Equal(_page.Url, UrlBase + "/register");
    }

    [Fact]
    public async Task LeaveWorkspace_Unavailable_IfHasOwnerRole()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        
        var button = "//button[contains(@class ,'leave-workspace')]";
        
        Assert.False(await _page.ElementExists(button));
    }
    
    
    [Fact]
    public async Task LeaveWorkspace_Success_IfHasAdminRole()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test14@example.com", "Password1234");
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        
        var button = "//button[contains(@class ,'leave-workspace')]";

        await _page.ClickAsync(button);
        
        var confirm = "//button[contains(@class ,'alert-confirm')]";
        
        await _page.ClickAsync(confirm);
        await Task.Delay(100);
        Assert.False(_page.Url.Contains("/workspaces/"));
    }
    
    [Fact]
    public async Task LeaveWorkspace_Success_IfHasMemberRole()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test13@example.com", "Password1234");
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        
        var button = "//button[contains(@class ,'leave-workspace')]";

        await _page.ClickAsync(button);
        
        var confirm = "//button[contains(@class ,'alert-confirm')]";
        
        await _page.ClickAsync(confirm);
        await Task.Delay(100);

        Assert.False(_page.Url.Contains("/workspaces/"));
    }
    
    [Fact]
    public async Task LeaveWorkspace_Cancel_WhenClickCancelButton()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test2@example.com", "Password1234");
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        
        var button = "//button[contains(@class ,'leave-workspace')]";

        await _page.ClickAsync(button);
        
        var cancel = "//button[contains(@class ,'alert-cancel')]";
        
        await _page.ClickAsync(cancel);

        Assert.True(_page.Url.Contains("/workspaces/"));
    }

    [Fact]
    public async Task CreateWorkspace_Success_WhenUniqueName()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");

        var button = "//button[contains(@class, 'chakra-button chakra-dialog__trigger css-md70yr')]";
        await _page.ClickAsync(button);

        var input = "//input[contains(@class, 'chakra-input css-eiee9d')]";
        await _page.FillAsync(input, Guid.NewGuid().ToString());

        var submit = "//button[contains(@class, 'chakra-button chakra-dialog__trigger css-n9m7sp')]";
        await _page.ClickAsync(submit);

        var errorElement = "//span[contains(@class, 'chakra-field__errorText css-118wl8')]";

        Assert.False(await _page.ElementExists(errorElement));
    }

    [Fact]
    public async Task CreateWorkspace_ShowError_WhenNamesDuplicate()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");

        var button = "//button[contains(@class, 'chakra-button chakra-dialog__trigger css-md70yr')]";
        await _page.ClickAsync(button);

        var input = "//input[contains(@class, 'chakra-input css-eiee9d')]";
        await _page.FillAsync(input, "Workspace 2");

        var submit = "//button[contains(@class, 'chakra-button css-166vzny')]";
        await _page.ClickAsync(submit);

        var errorElement = "//span[contains(@class, 'chakra-field__errorText css-118wl8')]";
        string? error = null;

        if (await _page.ElementExists(errorElement))
        {
            error = await _page.TextContentAsync(errorElement);
        }

        Assert.False(string.IsNullOrEmpty(error));
    }

    [Fact]
    public async Task CreateWorkspace_ShowError_WhenNameEmpty()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");

        var button = "//button[contains(@class, 'chakra-button chakra-dialog__trigger css-md70yr')]";
        await _page.ClickAsync(button);

        var submit = "//button[contains(@class, 'chakra-button css-166vzny')]";
        await _page.ClickAsync(submit);

        var errorElement = "//span[contains(@class, 'chakra-field__errorText css-118wl8')]";
        string? error = null;

        if (await _page.ElementExists(errorElement))
        {
            error = await _page.TextContentAsync(errorElement);
        }

        Assert.False(string.IsNullOrEmpty(error));
    }

    [Fact]
    public async Task CreateWorkspace_ShowDialog_WhenClickedButton()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");

        var button = "//button[contains(@class, 'chakra-button chakra-dialog__trigger css-md70yr')]";
        await _page.ClickAsync(button);

        var submit = "//button[contains(@class, 'chakra-button css-166vzny')]";

        Assert.True(await _page.ElementExists(submit));
    }

    [Fact]
    public async Task CreateWorkspace_Canceled_WhenClickedCloseButton()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");

        var button = "//button[contains(@class, 'chakra-button chakra-dialog__trigger css-md70yr')]";
        await _page.ClickAsync(button);

        var cancel = "//button[contains(@class, 'chakra-button css-y5du55')]";
        await _page.ClickAsync(cancel);

        var overlay = "//div[@data-aria-hidden]";

        Assert.False(await _page.ElementExists(overlay));
    }

    [Fact]
    public async Task GrantAdminRoleToMember_Success_IfOwnerOfWorkspace()
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

        var userToAdmin = "test8@example.com";
        var button = $"//tr[contains(.//td[contains(@class, 'email-cell')]/text(), '{userToAdmin}') ]//button[contains(@class, 'grant')]";
        
        await _page.ClickAsync(button);
        
        var isAdmin = $"//tr[contains(.//td[contains(@class, 'email-cell') and contains(@class, 'admin')]/text(), '{userToAdmin}') ]";
        
        var confirm = "//button[contains(@class, 'chakra-button css-aoq1vx')]";
        await _page.ClickAsync(confirm);

        await Task.Delay(500);

        Assert.True(await _page.ElementExists(isAdmin));
    }
    
    [Fact]
    public async Task GrantAdminRoleToMember_Cancel_IfClickedCancelButton()
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

        var userToAdmin = "test3@example.com";
        var button = $"//tr[contains(.//td[contains(@class, 'email-cell')]/text(), '{userToAdmin}') ]//button[contains(@class, 'grant')]";
        
        await _page.ClickAsync(button);
        
        var isAdmin = $"//tr[contains(.//td[contains(@class, 'email-cell') and contains(@class, 'admin')]/text(), '{userToAdmin}') ]";
        
        var cancel = "//button[contains(@class, 'chakra-button css-y5du55')]";
        await _page.ClickAsync(cancel);

        Assert.False(await _page.ElementExists(isAdmin));
    }

    [Fact]
    public async Task RevokeAdminRoleOfAdmin_Success_IfOwnerOfWorkspace()
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

        var userToMember = "test5@example.com";
        var button = $"//tr[contains(.//td[contains(@class, 'email-cell')]/text(), '{userToMember}') ]//button[contains(@class, 'revoke')]";
        
        await _page.ClickAsync(button);
        
        var isMember = $"//tr[contains(.//td[contains(@class, 'email-cell') and contains(@class, 'member')]/text(), '{userToMember}') ]";
        
        var confirm = "//button[contains(@class, 'chakra-button css-14str5r')]";
        await _page.ClickAsync(confirm);

        await Task.Delay(500);

        Assert.True(await _page.ElementExists(isMember));
    }
    
    [Fact]
    public async Task RevokeAdminRoleToMember_Cancel_IfClickedCancelButton()
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

        var userToAdmin = "test2@example.com";
        var button = $"//tr[contains(.//td[contains(@class, 'email-cell')]/text(), '{userToAdmin}') ]//button[contains(@class, 'revoke')]";
        
        await _page.ClickAsync(button);
        
        var isMember = $"//tr[contains(.//td[contains(@class, 'email-cell') and contains(@class, 'member')]/text(), '{userToAdmin}') ]";
        
        var cancel = "//button[contains(@class, 'chakra-button css-y5du55')]";
        await _page.ClickAsync(cancel);

        Assert.False(await _page.ElementExists(isMember));
    }

    [Fact]
    public async Task DeleteWorkspace_Success_IfNoActiveBoardsExist()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test15@example.com", "Password1234");
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        
        var button = "//button[contains(@class ,'delete-workspace')]";

        await _page.ClickAsync(button);
        
        var confirm = "//button[contains(@class ,'alert-confirm')]";
        
        await _page.ClickAsync(confirm);

        await Task.Delay(100);

        Assert.False(_page.Url.Contains("/workspaces/"));
    }

    [Fact]
    public async Task DeleteWorkspace_Disabled_IfActiveBoardsExist()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        
        var button = "//button[contains(@class ,'delete-workspace')]";

        bool isDisabled = await _page.Locator(button).IsDisabledAsync();

        Assert.True(isDisabled);
    }

    [Fact]
    public async Task ViewBoard_ArchivedBoardsHidden_WhenMemberRole()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test3@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);

        var archivedBoard =
            "//div[contains(@class, 'chakra-stack css-1m0tjh1')]//button[contains(@class, 'chakra-button css-10ap741')]";

        Assert.False(await _page.ElementExists(archivedBoard));
    }

    [Fact]
    public async Task ViewBoard_ArchivedBoardsVisible_WhenOwnerRole()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);

        var archivedBoard =
            "//div[contains(@class, 'chakra-stack css-1m0tjh1')]//button[contains(@class, 'chakra-button css-10ap741')]";

        Assert.True(await _page.ElementExists(archivedBoard));
    }

    [Fact]
    public async Task ArchiveBoard_Success_WhenOwnerRole()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);

        var archivedBoard =
            "//div[contains(@class, 'chakra-stack css-1m0tjh1')]//button[contains(@class, 'chakra-button css-10ap741')]";

        var n1 = await _page.Locator(archivedBoard).CountAsync();
        
        
        var archiveBoard =
            "(//div[contains(@class, 'chakra-stack css-1m0tjh1') and .//button[contains(@class, 'chakra-button css-o4f00r')]]//button[contains(@class, 'chakra-button css-o4f00r')])[last()]";

        await _page.ClickAsync(archiveBoard);
        
        var n2 = await _page.Locator(archivedBoard).CountAsync();
        
        Assert.True(n2 > n1);
    }

    [Fact]
    public async Task DeleteBoard_Success_WhenOwnerRole()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);

        var archivedBoard =
            "//div[contains(@class, 'chakra-stack css-1m0tjh1')]//button[contains(@class, 'chakra-button css-10ap741')]";

        if (!await _page.ElementExists(archivedBoard))
        {
            Assert.Fail();
        }
        
        var n1 = await _page.Locator(archivedBoard).CountAsync();
        
        await _page.ClickAsync(archivedBoard);

        await Task.Delay(500);
        
        var n2 = await _page.Locator(archivedBoard).CountAsync();
        
        Assert.True(n2 < n1);
    }
    
    [Fact]
    public async Task AddGuest_Unavailable_WhenActorHasMemberRole()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test3@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        
        var openBoard = "//div[contains(@class, 'chakra-card__root css-10r8yuk')]//p";
        await _page.ClickAsync(openBoard);

        var guestList = "//button[contains(@class, 'chakra-button chakra-drawer__trigger css-xvlchq')]";
        await _page.ClickAsync(guestList);
        
        int count1 = await _page.Locator("//tr").CountAsync();
        
        var search = "//input[@class=\"chakra-input css-eiee9d\"]";
        await _page.FillAsync(search, "test");
        await _page.ClickAsync("//ul[@class=\"chakra-list__root css-1yzzf3n\"]/li");

        var add = "//button[@class=\"chakra-button css-166vzny\"]";
        await _page.ClickAsync(add);
        
        var e = await _page.ContentAsync();

        int count2 = await _page.Locator("//tr").CountAsync();
        
        Assert.True(count2 == count1);
    }

    [Fact]
    public async Task RestoreBoard_Success_WhenOwnerRole()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);

        var archivedBoard =
            "//div[contains(@class, 'chakra-stack css-1m0tjh1')]//button[contains(@class, 'chakra-button css-12vy4dw')]";

        if (!await _page.ElementExists(archivedBoard))
        {
            Assert.Fail();
        }
        
        var n1 = await _page.Locator(archivedBoard).CountAsync();
        
        await _page.ClickAsync(archivedBoard);
        
        await Task.Delay(500);
        
        var n2 = await _page.Locator(archivedBoard).CountAsync();
        
        Assert.True(n2 < n1);
    }

    [Fact]
    public async Task AllowedFiles_CanAccess_WhenAdminRole()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test2@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        
        var openBoard = "//div[contains(@class, 'chakra-card__body css-7b8ujn') and .//button[contains(@class, 'chakra-button css-o4f00r')]]//p";
        await _page.ClickAsync(openBoard);

        var types = "//button[contains(@class, 'allowedType')]";
        
        Assert.True(await _page.ElementExists(types));
    }

    [Fact]
    public async Task AllowedFiles_Unavailable_WhenMemberRole()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test3@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        
        var openBoard = "//div[contains(@class, 'chakra-card__body css-7b8ujn')]//p";
        
        await _page.ClickAsync(openBoard);

        var types = "//button[contains(@class, 'allowedType')]";
        
        Assert.False(await _page.ElementExists(types));
    }

    [Fact]
    public async Task AllowedFiles_ChangeAmount_WhenSaved()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        var openBoard = "//div[contains(@class, 'chakra-card__body css-7b8ujn')]//p";
        await _page.ClickAsync(openBoard);
        var types = "//button[contains(@class, 'allowedType')]";
        await _page.ClickAsync(types);
        var checkedBoxes = "//div[@data-scope=\"checkbox\" and @data-state='checked']";
        var checkedTypesPrev = await _page.Locator(checkedBoxes).CountAsync();
        
        var parent =
            "//div[contains(@class, 'chakra-collapsible__root css-0')]/*[contains(@class, 'chakra-checkbox__root css-1rjggtr')]";
        await _page.ClickAsync(parent);
        var submit = "//button[contains(@class, 'submit-allowed-files')]";
        
        await _page.ClickAsync(submit);
        await _page.ClickAsync(types);
        var checkedTypesCurrent = await _page.Locator(checkedBoxes).CountAsync();
        Assert.True(checkedTypesCurrent > checkedTypesPrev);
    }


    [Fact]
    public async Task ViewBoard_Success_WhenAllowedAsGuest()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test9@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        
        var openBoard = "//div[contains(@class, 'chakra-card__body css-7b8ujn')]//p";
        
        await _page.ClickAsync(openBoard);
        
        Assert.True(_page.Url.Contains("/boards/"));
    }

    [Fact]
    public async Task CreateStatus_Unavailable_WhenGuest()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test9@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        
        var openBoard = "//div[contains(@class, 'chakra-card__body css-7b8ujn')]//p";
        
        await _page.ClickAsync(openBoard);

        var createStatus =
            "//button[contains(@class, 'status-dialog')]";
        
        Assert.False(await _page.ElementExists(createStatus));
    }

    [Fact]
    public async Task CreateStatus_ShowError_WhenEmptyName()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test3@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        
        var openBoard = "//div[contains(@class, 'chakra-card__body css-7b8ujn')]//p";
        
        await _page.ClickAsync(openBoard);

        var statusDialog =
            "//button[contains(@class, 'status-dialog')]";

        await _page.ClickAsync(statusDialog);

        var createStatus = "//button[contains(@class, 'create-status')]";
        
        await _page.ClickAsync(createStatus);

        var error = "//span[contains(@class, 'error')]";
        
        Assert.True(await _page.ElementExists(error));
    }

    [Fact]
    public async Task CreateStatus_Success_WhenEnteredName()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test3@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        
        var openBoard = "//div[contains(@class, 'chakra-card__body css-7b8ujn')]//p";
        
        await _page.ClickAsync(openBoard);

        var statusDialog =
            "//button[contains(@class, 'status-dialog')]";

        await _page.ClickAsync(statusDialog);

        var statusName = $"Status {Random.Shared.Next(1000)}";
        await _page.FillAsync("//input[contains(@class, 'status-name')]", statusName);
        
        var createStatus = "//button[contains(@class, 'create-status')]";
        
        await _page.ClickAsync(createStatus);

        var newStatus = $"//*[contains(@class, 'status-heading') and contains(text(),'{statusName}')]"; 
        
        Assert.True(await _page.ElementExists(newStatus));
    }

    [Fact]
    public async Task CreateStatus_ShowIndicator_WhenSetLimit()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test3@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        
        var openBoard = "//div[contains(@class, 'chakra-card__body css-7b8ujn')]//p";
        
        await _page.ClickAsync(openBoard);

        var statusDialog =
            "//button[contains(@class, 'status-dialog')]";

        await _page.ClickAsync(statusDialog);

        var statusName = $"Status {Random.Shared.Next(1000)}";
        await _page.FillAsync("//input[contains(@class, 'status-name')]", statusName);

        await _page.ClickAsync("//*[contains(@class, 'set-limit-check')]");
        
        var createStatus = "//button[contains(@class, 'create-status')]";
        await _page.ClickAsync(createStatus);

        var newStatusWithBadge = $"//*[../*[contains(@class, 'status-heading') and contains(text(),'{statusName}')] and contains(@class, 'task-limit-badge')]"; 
        
        Assert.True(await _page.ElementExists(newStatusWithBadge));
    }

    [Fact]
    public async Task CreateTask_AssigneeEmptyList_WhenUserNotFound()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        var openBoard = "//div[contains(@class, 'chakra-card__body css-7b8ujn')]//p";
        await _page.ClickAsync(openBoard);

        var dialogButton =
            "//div[contains(@class, 'status') and .//*[contains(@class, 'status-heading') and contains(text(), 'Status 1')]]//*[contains(@class, 'add-task')]";
        await _page.ClickAsync(dialogButton);

        var taskName = $"New task {Random.Shared.Next(1000)}";
        await _page.FillAsync("//input[contains(@class, 'title')]", taskName);
        
        var assignee = "nonexisting";
        await _page.FillAsync("//input[contains(@class, 'select-member')]", assignee);

        var suggestedItem = "//*[contains(@class, 'suggested-member')]";
        Assert.False(await _page.ElementExists(suggestedItem));
    }

    [Fact]
    public async Task CreateTask_AssigneeSet_WhenUserMemberOfWorkspace()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        var openBoard = "//div[contains(@class, 'chakra-card__body css-7b8ujn')]//p";
        await _page.ClickAsync(openBoard);

        var dialogButton =
            "//div[contains(@class, 'status') and .//*[contains(@class, 'status-heading') and contains(text(), 'Status 1')]]//*[contains(@class, 'add-task')]";
        await _page.ClickAsync(dialogButton);

        var taskName = $"New task {Random.Shared.Next(1000)}";
        await _page.FillAsync("//input[contains(@class, 'title')]", taskName);
        
        var assignee = "test1";
        await _page.FillAsync("//input[contains(@class, 'select-member')]", assignee);

        var suggestedItem = "//*[contains(@class, 'suggested-member')]";
        await _page.ClickAsync(suggestedItem);
        await _page.ClickAsync("//button[contains(@class, 'create-task')]");

        var user = "testuser1";
        
        await Task.Delay(100);
        Assert.True(await _page.ElementExists($"//div[contains(@class, 'assignee')]//span[contains(text(), '{user}')]"));
    }

    [Fact]
    public async Task UploadFile_NotAllowed_WhenGuestRole()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test9@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        
        var openBoard = "//div[contains(@class, 'chakra-card__body css-7b8ujn')]//p";
        
        await _page.ClickAsync(openBoard);

        var task = "//*[contains(@class, 'task-open')]";
        
        await _page.ClickAsync(task);
        
        var upload = "//button[contains(@class, 'upload-file')]";

        Assert.False(await _page.ElementExists(upload));
    }

    [Fact]
    public async Task UploadFile_ShowError_WhenFileTooBig()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test3@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        
        var openBoard = "//div[contains(@class, 'chakra-card__body css-7b8ujn')]//p";
        
        await _page.ClickAsync(openBoard);

        var task = "//*[contains(@class, 'task-open')]";

        await _page.ClickAsync(task);
        
        string projectDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
        string fullPath = Path.Combine(projectDir, "TestData", "dictionary.txt");
        
        await _page.SetInputFilesAsync("input[type='file']", [ fullPath ]);

        var errorToast = "//div[contains(@class, 'chakra-toast')]";

        await Task.Delay(500);
        
        Assert.True(await _page.ElementExists(errorToast));
    }

    [Fact]
    public async Task UploadFile_ShowError_WhenFileHasNotAllowedExtension()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test3@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        
        var openBoard = "//div[contains(@class, 'chakra-card__body css-7b8ujn')]//p";
        
        await _page.ClickAsync(openBoard);

        var task = "//*[contains(@class, 'task-open')]";

        await _page.ClickAsync(task);
        
        string projectDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
        string fullPath = Path.Combine(projectDir, "TestData", "file.js");
        
        await _page.SetInputFilesAsync("input[type='file']", [ fullPath ]);

        var errorToast = "//div[contains(@class, 'chakra-toast')]";

        await Task.Delay(500);
        
        Assert.True(await _page.ElementExists(errorToast));
    }

    [Fact]
    public async Task UploadFile_Success_WhenFileIsLessThan10MbAndHasAllowedExtension()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test3@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        
        var openBoard = "//div[contains(@class, 'chakra-card__body css-7b8ujn')]//p";
        
        await _page.ClickAsync(openBoard);

        var task = "//*[contains(@class, 'task-open')]";
        
        await _page.ClickAsync(task);
        
        var attachment = "//div[contains(@class, 'attachment-tile')]";
        var prevCount = await _page.Locator(attachment).CountAsync();
        
        string projectDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
        string fullPath = Path.Combine(projectDir, "TestData", "test.txt");
        
        await _page.SetInputFilesAsync("input[type='file']", [ fullPath ]);
        var upload = "//button[contains(@class, 'upload-file')]";
        await Task.Delay(100);
        await _page.ClickAsync(upload);
        await Task.Delay(2000);
        
        var newCount = await _page.Locator(attachment).CountAsync();
        Assert.True(newCount > prevCount);
    }

    [Fact]
    public async Task SetDueDate_Success_WhenChecked()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        var openBoard = "//div[contains(@class, 'chakra-card__body css-7b8ujn')]//p";
        await _page.ClickAsync(openBoard);

        var dialogButton =
            "//div[contains(@class, 'status') and .//*[contains(@class, 'status-heading') and contains(text(), 'Status 1')]]//*[contains(@class, 'add-task')]";
        await _page.ClickAsync(dialogButton);

        var taskName = $"New task {Random.Shared.Next(1000)}";
        await _page.FillAsync("//input[contains(@class, 'title')]", taskName);
        
        await _page.ClickAsync("//*[contains(@class, 'due-date-check')]");
        await _page.ClickAsync("//button[contains(@class, 'create-task')]");
        await Task.Delay(100);
        Assert.True(await _page.ElementExists($"//div[contains(@class, 'due-date')]//span[contains(text(), '{DateTime.Today.Day}')]"));
    }
    
    [Fact]
    public async Task SetDueDate_SetTodayDefault_WhenSelectedPastDate()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        var openBoard = "//div[contains(@class, 'chakra-card__body css-7b8ujn')]//p";
        await _page.ClickAsync(openBoard);

        var dialogButton =
            "//div[contains(@class, 'status') and .//*[contains(@class, 'status-heading') and contains(text(), 'Status 1')]]//*[contains(@class, 'add-task')]";
        await _page.ClickAsync(dialogButton);

        var taskName = $"New task {Random.Shared.Next(1000)}";
        await _page.FillAsync("//input[contains(@class, 'title')]", taskName);
        
        await _page.ClickAsync("//*[contains(@class, 'due-date-check')]");

        var day = DateTime.Today.AddDays(-1).Day;

        var dayInput = "//input[contains(@name, 'day')]";
        
        var actualValueElement = await _page.QuerySelectorAsync(dayInput);
        await _page.FillAsync(dayInput, day.ToString());
        string value = await actualValueElement.GetAttributeAsync("value");
        Assert.True(value == DateTime.Today.Day.ToString());
        
    }


    /*[Fact]
    public async Task SelectTag_SelectedExisting_WhenExists()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        var openBoard = "//div[contains(@class, 'chakra-card__body css-7b8ujn')]//p";
        await _page.ClickAsync(openBoard);

        var dialogButton =
            "//div[contains(@class, 'status') and .//*[contains(@class, 'status-heading') and contains(text(), 'Status 1')]]//*[contains(@class, 'add-task')]";
        await _page.ClickAsync(dialogButton);

        var taskName = $"New task {Random.Shared.Next(1000)}";
        await _page.FillAsync("//input[contains(@class, 'title')]", taskName);
        
        var assignee = "test1";
        await _page.FillAsync("//input[contains(@class, 'select-member')]", assignee);

        var suggestedItem = "//*[contains(@class, 'suggested-member')]";
        await _page.ClickAsync(suggestedItem);
        await _page.ClickAsync("//button[contains(@class, 'create-task')]");
    }*/

    [Fact]
    public async Task SelectTag_CreatedNew_WhenDoestExists()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        var openBoard = "//div[contains(@class, 'chakra-card__body css-7b8ujn')]//p";
        await _page.ClickAsync(openBoard);

        var dialogButton =
            "//div[contains(@class, 'status') and .//*[contains(@class, 'status-heading') and contains(text(), 'Status 1')]]//*[contains(@class, 'add-task')]";
        await _page.ClickAsync(dialogButton);

        var taskName = $"New task {Random.Shared.Next(1000)}";
        await _page.FillAsync("//input[contains(@class, 'title')]", taskName);
        var newTag = $"test-{Random.Shared.Next(1000)}";
        
        await _page.FillAsync("//input[contains(@class, 'input-tag ')]", newTag);
        await _page.ClickAsync("//*[contains(@class, 'create-tag')]");
        
        await _page.ClickAsync("//button[contains(@class, 'create-task')]");
        
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test2@example.com", "Password1234");
        await _page.ClickAsync(workspaceSelect);
        await _page.ClickAsync(workspace);
        await _page.ClickAsync(openBoard);
        await _page.ClickAsync(dialogButton);
        await _page.FillAsync("//input[contains(@class, 'input-tag ')]", newTag);
        
        var result = await _page.ElementExists($"//*[contains(@class, 'suggested-tag') and contains(.//text(), '{newTag}')]");
        Assert.True(result);
    }

    [Fact]
    public async Task RemoveTag_RemovedFromSuggestions_WhenDoestExistOnDifferentTasks()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        var openBoard = "//div[contains(@class, 'chakra-card__body css-7b8ujn')]//p";
        await _page.ClickAsync(openBoard);
        var dialogButton =
            "//div[contains(@class, 'status') and .//*[contains(@class, 'status-heading') and contains(text(), 'Status 1')]]//*[contains(@class, 'add-task')]";
        await _page.ClickAsync(dialogButton);
        var taskName = $"New task {Random.Shared.Next(1000)}";
        await _page.FillAsync("//input[contains(@class, 'title')]", taskName);
        var newTag = "test";
        await _page.FillAsync("//input[contains(@class, 'input-tag ')]", newTag);
        await _page.ClickAsync("//*[contains(@class, 'create-tag')]");
        await _page.ClickAsync("//*[contains(@class, 'remove-tag')]");
        await _page.FillAsync("//input[contains(@class, 'input-tag ')]", newTag);
        
        var result = await _page.ElementExists($"//*[contains(@class, 'suggested-tag') and contains(.//text(), '{newTag}')]");
        Assert.False(result);
    }

    [Fact]
    public async Task RemoveTag_StaysInSuggestions_WhenExistOnDifferentTasks()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        var openBoard = "//div[contains(@class, 'chakra-card__body css-7b8ujn')]//p";
        await _page.ClickAsync(openBoard);
        var dialogButton =
            "//div[contains(@class, 'status') and .//*[contains(@class, 'status-heading') and contains(text(), 'Status 1')]]//*[contains(@class, 'add-task')]";
        await _page.ClickAsync(dialogButton);
        var taskName = $"New task {Random.Shared.Next(1000)}";
        await _page.FillAsync("//input[contains(@class, 'title')]", taskName);
        var tag = "QA";
        await _page.FillAsync("//input[contains(@class, 'input-tag ')]", tag);
        await _page.ClickAsync($"//*[contains(@class, 'suggested-tag') and contains(.//text(), '{tag}')]");
        
        await _page.ClickAsync("//*[contains(@class, 'remove-tag')]");
        await _page.FillAsync("//input[contains(@class, 'input-tag ')]", tag);
        
        var result = await _page.ElementExists($"//*[contains(@class, 'suggested-tag') and contains(.//text(), '{tag}')]");
        Assert.True(result);
    }

    [Fact]
    public async Task DragTask_RepositionInStatus_WhenMoveVertically()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        
        var openBoard = "//div[contains(@class, 'chakra-card__body css-7b8ujn')]//p";
        
        await _page.ClickAsync(openBoard);

        var statusName = "Status 2";
        
        var status =
            $"//div[contains(@class, 'status') and .//*[contains(@class, 'status-heading') and contains(text(), '{statusName}')]]";

        var items = status + "//*[contains(@class, 'task-open')]";
        
        var taskHandles = await _page.QuerySelectorAllAsync(items);
        var initialOrder = await Task.WhenAll(taskHandles.Select(h => h.TextContentAsync()));
        
        var targetPosition = await _page.QuerySelectorAsync("(" + status + "//div[contains(@class, 'task-drag-handle')])[last()]");
        var sourceHandle = await _page.QuerySelectorAsync(status+ "//div[contains(@class, 'task-drag-handle')]");

        await DragDropToAsync(sourceHandle, targetPosition);
        
        await Task.Delay(100);
        
        var taskHandles1 = await _page.QuerySelectorAllAsync(items);
        var newOrder = await Task.WhenAll(taskHandles1.Select(h => h.TextContentAsync()));
        
        Assert.NotEqual(initialOrder, newOrder);
    }

    [Fact]
    public async Task DragTask_ChangeStatus_WhenMoveHorizontally()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        
        var openBoard = "//div[contains(@class, 'chakra-card__body css-7b8ujn')]//p";
        
        await _page.ClickAsync(openBoard);

        var statusName = "Status 2";
        
        var status =
            $"//div[contains(@class, 'status') and .//*[contains(@class, 'status-heading') and contains(text(), '{statusName}')]]";

        var items = status + "//*[contains(@class, 'task-open')]";
        
        var taskHandles = await _page.QuerySelectorAllAsync(items);
        var initialOrder = await Task.WhenAll(taskHandles.Select(h => h.TextContentAsync()));
        
        var targetPosition = await _page.QuerySelectorAsync($"//div[contains(@class, 'status') and .//*[contains(@class, 'status-heading') and contains(text(), 'Status 1')]]//*[contains(@class, 'add-task')]");
        var sourceHandle = await _page.QuerySelectorAsync(status+ "//div[contains(@class, 'task-drag-handle')]");

        await DragDropToAsync(sourceHandle, targetPosition);
        
        await Task.Delay(100);
        
        var taskHandles1 = await _page.QuerySelectorAllAsync(items);
        var newOrder = await Task.WhenAll(taskHandles1.Select(h => h.TextContentAsync()));
        
        Assert.NotEqual(initialOrder, newOrder);
        Assert.True(initialOrder.Length > newOrder.Length);
    }
    
    [Fact]
    public async Task DragTask_Blocked_WhenNotAssignee()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        
        var openBoard = "//div[contains(@class, 'chakra-card__body css-7b8ujn')]//p";
        
        await _page.ClickAsync(openBoard);

        var statusName = "Status 1";
        
        var status =
            $"//div[contains(@class, 'status') and .//*[contains(@class, 'status-heading') and contains(text(), '{statusName}')]]";

        var items = status + "//*[contains(@class, 'task-open')]";

        var targetStatus = "Status 2";

        var targetStatusXPath =
            $"//div[contains(@class, 'status') and .//*[contains(@class, 'status-heading') and contains(text(), '{targetStatus}')]]";

        var assignedTaskHandle =
            "//div[contains(@class, 'head') and ..//div[contains(@class, 'assignee')]]//div[contains(@class, 'task-drag-handle')]";
        
        var taskHandles = await _page.QuerySelectorAllAsync(items);
        var initialOrder = await Task.WhenAll(taskHandles.Select(h => h.TextContentAsync()));
        
        var targetPosition = await _page.QuerySelectorAsync(targetStatusXPath + "//*[contains(@class, 'add-task')]");
        var sourceHandle = await _page.QuerySelectorAsync(status + assignedTaskHandle);

        await DragDropToAsync(sourceHandle, targetPosition);
        
        await Task.Delay(100);
        
        var taskHandles1 = await _page.QuerySelectorAllAsync(items);
        var newOrder = await Task.WhenAll(taskHandles1.Select(h => h.TextContentAsync()));
        
        Assert.Equal(initialOrder, newOrder);
    }
    
    [Fact]
    public async Task DragTask_Allowed_WhenAssignee()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test2@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        
        var openBoard = "//div[contains(@class, 'chakra-card__body css-7b8ujn')]//p";
        
        await _page.ClickAsync(openBoard);

        var statusName = "Status 1";
        
        var status =
            $"//div[contains(@class, 'status') and .//*[contains(@class, 'status-heading') and contains(text(), '{statusName}')]]";

        var items = status + "//*[contains(@class, 'task-open')]";

        var targetStatus = "Status 2";

        var targetStatusXPath =
            $"//div[contains(@class, 'status') and .//*[contains(@class, 'status-heading') and contains(text(), '{targetStatus}')]]";

        var assignedTaskHandle =
            "//div[contains(@class, 'head') and ..//div[contains(@class, 'assignee')]]//div[contains(@class, 'task-drag-handle')]";
        
        var taskHandles = await _page.QuerySelectorAllAsync(items);
        var initialOrder = await Task.WhenAll(taskHandles.Select(h => h.TextContentAsync()));
        
        var targetPosition = await _page.QuerySelectorAsync(targetStatusXPath + "//*[contains(@class, 'add-task')]");
        var sourceHandle = await _page.QuerySelectorAsync(status + assignedTaskHandle);

        await DragDropToAsync(sourceHandle, targetPosition);
        
        await Task.Delay(100);
        
        var taskHandles1 = await _page.QuerySelectorAllAsync(items);
        var newOrder = await Task.WhenAll(taskHandles1.Select(h => h.TextContentAsync()));
        
        Assert.NotEqual(initialOrder, newOrder);
    }

    private async Task DragDropToAsync(IElementHandle source, IElementHandle target)
    {
        var sourceBound = await source.BoundingBoxAsync();
        var targetBound = await target.BoundingBoxAsync();

        // Perform drag and drop with mouse actions
        await _page.Mouse.MoveAsync(sourceBound.X + sourceBound.Width / 2, sourceBound.Y + sourceBound.Height / 2, new()
        {
            Steps = 10
        });
        await _page.Mouse.DownAsync();
        await Task.Delay(100);
        await _page.Mouse.MoveAsync(targetBound.X + targetBound.Width / 2, targetBound.Y + targetBound.Height / 2);
        await Task.Delay(100);
        await _page.Mouse.UpAsync();
        await Task.Delay(100);
    }

    [Fact]
    public async Task DragStatus_ChangePosition_WhenMoveHorizontally()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        
        var openBoard = "//div[contains(@class, 'chakra-card__body css-7b8ujn')]//p";
        
        await _page.ClickAsync(openBoard);
        
        var initialOrder = await _page.EvaluateAsync<string[]>("() => Array.from(document.querySelectorAll('.status-heading')).map(col => col.innerText)");
        
        var  targetPosition= await _page.QuerySelectorAsync("(//*[contains(@class, 'column-drag-handle')])[last()]");
        var sourceHandle = await _page.QuerySelectorAsync("//*[contains(@class, 'column-drag-handle')]");

        await Task.Delay(100);
        
        await DragDropToAsync(sourceHandle, targetPosition);
        
        var newOrder = await _page.EvaluateAsync<string[]>("() => Array.from(document.querySelectorAll('.status-heading')).map(col => col.innerText)");
        
        Assert.NotEqual(initialOrder, newOrder);
    }

    [Fact]
    public async Task MembersPage_OwnerLabelHasDifferentStyle()
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

        var styledText = "//p[contains(@class, 'css')]";
        
        var memberRow = "//tr[.//td[contains(@class, 'email-cell') and contains(@class, 'member')]]";
        var ownerRow = "//tr[.//td[contains(@class, 'email-cell') and contains(@class, 'owner')]]";
        
        Assert.True(await _page.ElementExists(ownerRow + styledText));
        Assert.False(await _page.ElementExists(memberRow + styledText));
    }
    
    [Fact]
    public async Task DeleteAccount_ConfirmDisabled_WhenPasswordEmpty()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test10@example.com", "Password1234");
        await _page.ClickAsync("//button[contains(@class, 'tome')]");
       
        var button = "//button[contains(@class ,'delete-acc')]";

        await _page.ClickAsync(button);
        
        var delete = "//button[contains(@class ,'alert-confirm')]";
        
        bool isDisabled = await _page.Locator(delete).IsDisabledAsync();

        Assert.True(isDisabled);
    }
    
    [Fact]
    public async Task DeleteAccount_Success_WhenPasswordValid()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test4@example.com", "Password1234");
        await _page.ClickAsync("//button[contains(@class, 'tome')]");
       
        var button = "//button[contains(@class ,'delete-acc')]";
        await _page.ClickAsync(button);
        var error = "//span[contains(@class, 'error')]";

        var password = "Password1234";
        await _page.FillAsync("//input[contains(@class, 'password')]", password);
        
        var delete = "//button[contains(@class ,'alert-confirm')]";
        await _page.ClickAsync(delete);

        await Task.Delay(500);
        
        Assert.True(_page.Url.Contains("login"));
    }
    
    [Fact]
    public async Task DeleteAccount_ShowError_WhenPasswordInvalid()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test10@example.com", "Password1234");
        await _page.ClickAsync("//button[contains(@class, 'tome')]");
       
        var button = "//button[contains(@class ,'delete-acc')]";
        await _page.ClickAsync(button);
        var error = "//span[contains(@class, 'error')]";
        await _page.FillAsync("//input[contains(@class, 'password')]", "password");
        
        var delete = "//button[contains(@class ,'alert-confirm')]";
        await _page.ClickAsync(delete);

        Assert.True(await _page.ElementExists(error));
    }
    
    [Fact]
    public async Task DeleteAccount_Canceled_WhenClickedCancelButton()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test10@example.com", "Password1234");
        await _page.ClickAsync("//button[contains(@class, 'tome')]");
       
        var button = "//button[contains(@class ,'delete-acc')]";

        await _page.ClickAsync(button);
        
        var cancel = "//button[contains(@class ,'alert-cancel')]";
        
        await _page.ClickAsync(cancel);
        
        Assert.True(_page.Url.Contains("/me"));
    }

    [Fact]
    public async Task DeleteAccount_AskConfirmation_WhenClicked()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test10@example.com", "Password1234");
        await _page.ClickAsync("//button[contains(@class, 'tome')]");

        var button = "//button[contains(@class ,'delete-acc')]";

        await _page.ClickAsync(button);

        var dialog = "//div[contains(@class, 'chakra-dialog__content css-oedudq')]";
        
        Assert.True(await _page.ElementExists(dialog));
    }

    [Fact]
    public async Task DeleteAccount_Locked_IfWorkspacesExist()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");
        await _page.ClickAsync("//button[contains(@class, 'tome')]");

        var button = "//button[contains(@class ,'delete-acc')]";

        await Task.Delay(100);
        
        bool isDisabled = await _page.Locator(button).IsDisabledAsync();
        
        Assert.True(isDisabled);
    }
    
    [Fact]
    public async Task AllButtonHaveLabels_WhenNavigatedToMainPage()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");
        var buttons = await _page.QuerySelectorAllAsync("button");
        var buttonsWithoutText = new List<string>();
    
        foreach (var button in buttons)
        {
            var text = await button.TextContentAsync();
            var ariaLabel = await button.GetAttributeAsync("aria-label");
            var title = await button.GetAttributeAsync("title");
            
            if (string.IsNullOrWhiteSpace(text) && 
                string.IsNullOrWhiteSpace(ariaLabel) && 
                string.IsNullOrWhiteSpace(title))
            {
                var id = await button.GetAttributeAsync("id") ?? "unknown";
                var classes = await button.GetAttributeAsync("class") ?? "no-class";
                buttonsWithoutText.Add($"Button {id} ({classes})");
            }
        }
    
        Assert.Empty(buttonsWithoutText);
    }
    
    [Fact]
    public async Task AllButtonHaveLabels_WhenNavigatedToWorkspacePage()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        
        var buttons = await _page.QuerySelectorAllAsync("button");
        var buttonsWithoutText = new List<string>();
    
        foreach (var button in buttons)
        {
            var text = await button.TextContentAsync();
            var ariaLabel = await button.GetAttributeAsync("aria-label");
            var title = await button.GetAttributeAsync("title");
            
            if (string.IsNullOrWhiteSpace(text) && 
                string.IsNullOrWhiteSpace(ariaLabel) && 
                string.IsNullOrWhiteSpace(title))
            {
                var id = await button.GetAttributeAsync("id") ?? "unknown";
                var classes = await button.GetAttributeAsync("class") ?? "no-class";
                buttonsWithoutText.Add($"Button {id} ({classes})");
            }
        }
    
        Assert.Empty(buttonsWithoutText);
    }
    
    
    // Start of acceptance tests

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

        var email = "test_nonexisting@example.com";
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

        var email = "test123@example.com";
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