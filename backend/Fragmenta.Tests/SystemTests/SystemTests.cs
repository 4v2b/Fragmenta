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
            Headless = true
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

        //await _page.GotoAsync(UrlBase + "/login");
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

        var email = "test3@example.com";
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
        Assert.NotEqual(error, timer);
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

        var email = "test2@example.com";
        await _page.FillAsync("//input[contains(@class, 'chakra-input css-eiee9d') and @type='email']", email);

        var button = "//div[contains(@class, 'main-content')]//button[@type='submit']";
        await _page.ClickAsync(button);
        await _page.ClickAsync(button);
        await _page.ClickAsync(button);

        var errorElement = "//p[contains(@class, 'css-zofl1m')]";
        var error = await _page.TextContentAsync(errorElement);

        Assert.True(string.IsNullOrEmpty(error));
    }

    [Fact]
    public async Task ForgotPassword_ShowTimer_WhenLocked()
    {
        await LogoutAsync();

        await _page.GotoAsync(UrlBase + "/forgot-password");

        var email = "test2@example.com";
        await _page.FillAsync("//input[contains(@class, 'chakra-input css-eiee9d') and @type='email']", email);

        var button = "//div[contains(@class, 'main-content')]//button[@type='submit']";
        await _page.ClickAsync(button);
        await _page.ClickAsync(button);
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

        Assert.Fail();
        // TODO Seed token into database to use for test
        // Navigate to login page
        await _page.GotoAsync(UrlBase + "/reset-password");

        //TODO Login with new password
    }

    [Fact]
    public async Task ResetPassword_ShowError_WhenPasswordsDontMatch()
    {
        Assert.Fail();
    }

    [Fact]
    public async Task ResetPassword_ShowError_WhenEmptyPassword()
    {
        Assert.Fail();
    }

    [Fact]
    public async Task ResetPassword_ShowError_WhenPasswordTooWeak()
    {
        Assert.Fail();

        // Navigate to login page
        await _page.GotoAsync(UrlBase + "/reset-password");
    }

    [Fact]
    public async Task ResetPassword_ShowError_WhenTokenInvalid()
    {
        Assert.Fail();
        // Navigate to login page
        await _page.GotoAsync(UrlBase + "/reset-password");
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
    public async Task LeaveWorkspace_ButtonInactive_IfHasOwnerRole()
    {
        await _page.GotoAsync(UrlBase + "/me");

        Assert.Fail();
    }

    [Fact]
    public async Task LeaveWorkspace_Success_IfDoesntHaveOwnerRole()
    {
        await _page.GotoAsync(UrlBase + "/me");

        Assert.Fail();
    }

    [Fact]
    public async Task LeaveWorkspace_NoAssigneeForTask_WhenUserLeft()
    {
        await _page.GotoAsync(UrlBase + "/me");

        Assert.Fail();
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
        Assert.Fail();
    }

    [Fact]
    public async Task RevokeAdminRoleOfAdmin_Success_IfOwnerOfWorkspace()
    {
        Assert.Fail();
    }

    [Fact]
    public async Task DeleteWorkpace_Success_IfNoActiveBoardsExist()
    {
        Assert.Fail();
    }

    [Fact]
    public async Task DeleteWorkpace_ShowError_IfActiveBoardsExist()
    {
        Assert.Fail();
    }

    [Fact]
    public async Task ViewBoard_ArchivedBoardsHidden_WhenMemberRole()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test8@example.com", "Password1234");
        
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
        
        var n2 = await _page.Locator(archivedBoard).CountAsync();
        
        Assert.True(n2 < n1);
    }

    [Fact]
    public async Task ViewBoards_ShowsTimerUntilDeleting_WhenBoardArchived()
    {
        Assert.Fail();
    }
    
    [Fact]
    public async Task AddGuest_Unavailable_WhenActorHasMemberRole()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test7@example.com", "Password1234");
        
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
        
        var n2 = await _page.Locator(archivedBoard).CountAsync();
        
        Assert.True(n2 < n1);
    }

    [Fact]
    public async Task AllowedFiles_CanAccess_WhenAdminRole()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        
        var openBoard = "(//button[contains(@class, 'chakra-card__body css-7b8ujn') and .//button[contains(@class, 'chakra-button css-o4f00r')]]//p)";
        await _page.ClickAsync(openBoard);

        var types = "//button[contains(@class, 'allowedType')]";
        
        Assert.True(await _page.ElementExists(types));
    }

    [Fact]
    public async Task AllowedFiles_Unavailable_WhenMemberRole()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test4@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        
        var openBoard = "(//button[contains(@class, 'chakra-card__body css-7b8ujn') and .//button[contains(@class, 'chakra-button css-o4f00r')]]//p)";
        
        await _page.ClickAsync(openBoard);

        var types = "//button[contains(@class, 'allowedType')]";
        
        Assert.False(await _page.ElementExists(types));
    }

    [Fact]
    public async Task AllowedFiles_DontChange_WhenCancel()
    {
        Assert.Fail();
        
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test1@example.com", "Password1234");
        
        var workspaceSelect = "//button[@id='select::r5::trigger']";
        await _page.ClickAsync(workspaceSelect);
        
        var workspace = "//div[@data-part='item']";
        await _page.ClickAsync(workspace);
        
        var openBoard = "(//button[contains(@class, 'chakra-card__body css-7b8ujn') and .//button[contains(@class, 'chakra-button css-o4f00r')]]//p)";
        await _page.ClickAsync(openBoard);

        var types = "//button[contains(@class, 'allowedType')]";

        await _page.ClickAsync(types);

        var cancel = "//button[contains(@class, 'chakra-button css-fw2fpl')]";
        
        await _page.ClickAsync(cancel);

    }

    [Fact]
    public async Task AllowedFiles_SelectSubitems_WhenParentItemChecked()
    {
        var parents =
            "//div[contains(@class, 'chakra-collapsible__root css-0')]/*[contains(@class, 'chakra-checkbox__root css-1rjggtr')]";
        
        var children =
            "//div[contains(@class, 'chakra-collapsible__content css-kjmtd3')]//*[contains(@class, 'chakra-checkbox__root css-1rjggtr')]";
        
        Assert.Fail();
        
    }

    [Fact]
    public async Task AllowedFiles_UnselectSubitems_WhenParentItemUnchecked()
    {
        Assert.Fail();
    }

    [Fact]
    public async Task AllowedFiles_Change_WhenSave()
    {
        Assert.Fail();
    }


    [Fact]
    public async Task ViewBoard_Success_WhenAllowedAsGuest()
    {
        Assert.Fail();
    }

    [Fact]
    public async Task CreateStatus_Unavailable_WhenGuest()
    {
        Assert.Fail();
    }

    [Fact]
    public async Task CreateStatus_ShowError_WhenEmptyName()
    {
        Assert.Fail();
    }

    [Fact]
    public async Task CreateStatus_Success_WhenEnteredCorrectName()
    {
        var createStatus =
            "//div[contains(@class, 'css-69i1ev')]//button[contains(@class, 'chakra-button chakra-dialog__trigger css-n9m7sp')]";
    }

    [Fact]
    public async Task CreateStatus_NoIndicator_WhenLimitNotSet()
    {
        Assert.Fail();
    }

    [Fact]
    public async Task CreateStatus_ShowIndicator_WhenSetLimit()
    {
        Assert.Fail();
    }

    [Fact]
    public async Task MoveTask_Success_WhenNoAssigneeOnTask()
    {
        Assert.Fail();
    }

    [Fact]
    public async Task MoveTask_NoResult_WhenAssigneeDifferentUser()
    {
        Assert.Fail();
    }

    [Fact]
    public async Task MoveTask_Success_WhenAssignedToCurrentUser()
    {
        Assert.Fail();
    }

    [Fact]
    public async Task CreateTask_EmptyList_WhenUserNotFound()
    {
        Assert.Fail();
    }

    [Fact]
    public async Task CreateTask_EmptyList_WhenUserNotMemberOfWorkspace()
    {
        Assert.Fail();
    }

    [Fact]
    public async Task CreateTask_AssigneeSet_WhenUserMemberOfWorkspace()
    {
        Assert.Fail();
    }

    [Fact]
    public async Task CreateTask_AssigneeUnset_WhenClickedRemoveButton()
    {
        Assert.Fail();
    }

    [Fact]
    public async Task UploadFile_NotAllowed_WhenGuestRole()
    {
        Assert.Fail();
    }

    [Fact]
    public async Task UploadFile_ShowError_WhenFileTooBig()
    {
        Assert.Fail();
    }

    [Fact]
    public async Task UploadFile_ShowError_WhenFileHasNotAllowedExtension()
    {
        Assert.Fail();
    }

    [Fact]
    public async Task UploadFile_Success_WhenFileIsLessThan10MbAndHasAllowedExtension()
    {
        Assert.Fail();
    }

    [Fact]
    public async Task DownloadFile_Success_WhenClickOnLoadButton()
    {
        Assert.Fail();
    }

    [Fact]
    public async Task RemoveAttachment_Success_WhenMemberRole()
    {
        Assert.Fail();
    }

    [Fact]
    public async Task RemoveAttachment_Unavailable_WhenGuestRole()
    {
        Assert.Fail();
    }

    [Fact]
    public async Task SetDueDate_Unavailable_WhenGuestRole()
    {
        Assert.Fail();
    }

    [Fact]
    public async Task SetDueDate_NotSelect_WhenDatePassed()
    {
        Assert.Fail();
    }

    [Fact]
    public async Task SetDueDate_NotSet_WhenUnchecked()
    {
        Assert.Fail();
    }

    [Fact]
    public async Task SetDueDate_Success_WhenFutureDateAndChecked()
    {
        Assert.Fail();
    }

    [Fact]
    public async Task SelectTag_SelectedExisting_WhenExists()
    {
        Assert.Fail();
    }

    [Fact]
    public async Task SelectTag_CreatedNew_WhenDoestExists()
    {
        Assert.Fail();
    }

    [Fact]
    public async Task RemoveTag_RemovedFromSuggestions_WhenDoestExistOnDifferentTasks()
    {
        Assert.Fail();
    }

    [Fact]
    public async Task RemoveTag_StaysInSuggestions_WhenExistOnDifferentTasks()
    {
        Assert.Fail();
    }

    [Fact]
    public async Task DragTask_RepositionInStatus_WhenMoveHorizontally()
    {
        Assert.Fail();
    }

    [Fact]
    public async Task DragTask_ChangeStatus_WhenMoveVertically()
    {
        Assert.Fail();
    }

    [Fact]
    public async Task DragStatus_ChangePosition_WhenMoveHorizontally()
    {
        Assert.Fail();
    }

    [Fact]
    public async Task MembersDisplay_OwnerHasDifferentStyle()
    {
        Assert.Fail();
    }

    [Fact]
    public async Task MembersDisplay_AdminAndMemberHaveSameStyle()
    {
        Assert.Fail();
    }
    
    [Fact]
    public async Task DeleteAccount_Success_IfNoWorkspacesExist()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test10@example.com", "Password1234");
        await _page.ClickAsync("//button[contains(@class, 'tome')]");
       
        var button = "//button[contains(@class ,'chakra-button chakra-dialog__trigger css-zgepf1')]";

        await _page.ClickAsync(button);

        var dialog = "//div[contains(@class, 'chakra-dialog__content css-oedudq')]";
        
        var delete = "//button[contains(@class ,'chakra-button css-1uynepj')]";
        
        await _page.ClickAsync(delete);

        Assert.False(await _page.ElementExists("//button[contains(@class, 'chakra-button css-10ap741')]"));
    }
    
    [Fact]
    public async Task DeleteAccount_Canceled_WhenClickedCancelButton()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test10@example.com", "Password1234");
        await _page.ClickAsync("//button[contains(@class, 'tome')]");
       
        var button = "//button[contains(@class ,'chakra-button chakra-dialog__trigger css-zgepf1')]";

        await _page.ClickAsync(button);
        
        var cancel = "//button[contains(@class ,'chakra-button css-y5du55')]";
        
        await _page.ClickAsync(cancel);

        Assert.True(await _page.ElementExists("//button[contains(@class, 'chakra-button css-10ap741')]"));
    }

    [Fact]
    public async Task DeleteAccount_AskConfirmation_WhenClicked()
    {
        await LogoutAsync();
        await _page.GotoAsync(UrlBase + "/login");
        await LoginAsync("test10@example.com", "Password1234");
        await _page.ClickAsync("//button[contains(@class, 'tome')]");

        var button = "//button[contains(@class ,'chakra-button chakra-dialog__trigger css-zgepf1')]";

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

        var button = "//button[contains(@class ,'chakra-button chakra-dialog__trigger css-zgepf1')]";

        await Task.Delay(100);
        
        bool isDisabled = await _page.Locator(button).IsDisabledAsync();
        
        Assert.True(isDisabled);
        
    }
}