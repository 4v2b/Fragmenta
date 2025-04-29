using Microsoft.Playwright;

namespace Fragmenta.Tests.SystemTests;

public class LoginTests : IAsyncLifetime
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

    private async Task LoginAsync(string email, string password)
    {
        await _page.FillAsync("//input[contains(@class, 'chakra-input css-eiee9d') and @type='email']", email);
        await _page.FillAsync("//input[contains(@class, 'chakra-input css-eiee9d') and @type='password']", password);
        
        // Click login button
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
        // Navigate to login page
        await _page.GotoAsync( UrlBase + "/login");
        
        // Fill credentials
        await LoginAsync("test1@example.com", "Password1234");
        
        // Verify successful login
        Assert.Equal("http://localhost:5173/", _page.Url);
        
        //var userName = await _page.Locator(".user-name").TextContentAsync();
        //Assert.Contains("testuser1", userName);
    }
    
    [Fact]
    public async Task Login_ContainsEmailAndPasswordFields_WhenNavigated()
    {
        // Navigate to login page
        await _page.GotoAsync( UrlBase + "/login");
        
        // Fill credentials
        
        // TODO Check if fields exist
            
        await _page.FillAsync("//input[contains(@class, 'chakra-input css-eiee9d') and @type='email']", "");
        await _page.FillAsync("//input[contains(@class, 'chakra-input css-eiee9d') and @type='password']", "");
    }
    
    [Fact]
    public async Task Login_ShowError_WhenEmailEmpty()
    {Assert.Fail();
        await _page.GotoAsync(UrlBase +"/login");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task Login_ShowError_WhenPasswordEmpty()
    {Assert.Fail();
        await _page.GotoAsync(UrlBase +"/login");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task Login_ShowError_IfEmailHasIncorrectFormat()
    {
        // Navigate to login page
        await _page.GotoAsync( UrlBase + "/login");
        
        // Fill credentials
        await LoginAsync("test1@example.com", "Password1234");
        
        // Verify successful login
        Assert.Equal("http://localhost:5173/", _page.Url);
    }
    
    [Fact]
    public async Task Login_ShowError_EmailDoesntBelongToUser()
    {
        // Navigate to login page
        await _page.GotoAsync( UrlBase + "/login");
        
        // Fill credentials
        await LoginAsync("test1@example.com", "Password1234");
        
        // Verify successful login
        Assert.Equal("http://localhost:5173/", _page.Url);
    }
    
    
    
    [Fact]
    public async Task Login_ShowError_IfPasswordInvalid()
    {
        // Navigate to login page
        await _page.GotoAsync( UrlBase + "/login");
        
        // Fill credentials
        await LoginAsync("test1@example.com", "Password1234");
        
        // Verify successful login
        Assert.Equal("http://localhost:5173/", _page.Url);
    }
    
    [Fact]
    public async Task Login_ShowError_IfTooManyUnsuccessfullTries()
    {
        // Navigate to login page
        await _page.GotoAsync( UrlBase + "/login");
        
        // Fill credentials
        await LoginAsync("test1@example.com", "Password1234");
        
        // Verify successful login
        Assert.Equal("http://localhost:5173/", _page.Url);
    }
    
    [Fact]
    public async Task Login_ShowTimer_WhenLocked()
    {
        // Navigate to login page
        await _page.GotoAsync( UrlBase + "/login");
        
        // Fill credentials
        await LoginAsync("test1@example.com", "Password1234");
        
        // Verify successful login
        Assert.Equal("http://localhost:5173/", _page.Url);
    }
    
    [Fact]
    public async Task ShouldShowErrorOnInvalidCredentials()
    {
        await _page.GotoAsync("https://your-app-url/login");
        await _page.FillAsync("#email", "wrong@example.com");
        await _page.FillAsync("#password", "WrongPassword");
        await _page.ClickAsync("button[type='submit']");
        
        var isVisible = await _page.Locator(".error-message").IsVisibleAsync();
        Assert.True(isVisible);
        
        var errorText = await _page.Locator(".error-message").TextContentAsync();
        Assert.Contains("Invalid credentials", errorText);
    }
    
    [Fact]
    public async Task ForgotPassword_Navigate_WhenLabelClicked()
    {Assert.Fail();
        await _page.GotoAsync(UrlBase +"/login");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task ForgotPassword_SendLetter_WhenEmailValid()
    {Assert.Fail();
        await _page.GotoAsync(UrlBase +"/login");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task ForgotPassword_ShowError_WhenEmailInvalid()
    {Assert.Fail();
        await _page.GotoAsync(UrlBase +"/login");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task ForgotPassword_ShowError_WhenFieldEmpty()
    {Assert.Fail();
        await _page.GotoAsync(UrlBase +"/login");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task ForgotPassword_ShowError_WhenEmailSentThreeTimes()
    {Assert.Fail();
        await _page.GotoAsync(UrlBase +"/login");
       
        
    }
    
    [Fact]
    public async Task ForgotPassword_ShowTimer_WhenLocked()
    {Assert.Fail();
        // Navigate to login page
        await _page.GotoAsync( UrlBase + "/forgot-password");
        
        
    }
    
    [Fact]
    public async Task ResetPassword_NavigateToLogin_WhenPasswordSuccessfullyReset()
    {
        Assert.Fail();
        // TODO Seed token into database to use for test
        // Navigate to login page
        await _page.GotoAsync( UrlBase + "/reset-password");
        
        //TODO Login with new password
       
    }
    
    [Fact]
    public async Task ResetPassword_ShowError_WhenPasswordsDontMatch()
    {
        Assert.Fail();

        // Navigate to login page
        await _page.GotoAsync( UrlBase + "/reset-password");
    }
    
    [Fact]
    public async Task ResetPassword_ShowError_WhenEmptyPassword()
    {
        Assert.Fail();

        // Navigate to login page
        await _page.GotoAsync( UrlBase + "/reset-password");
    }
    
    [Fact]
    public async Task ResetPassword_ShowError_WhenPasswordTooWeak()
    {
        Assert.Fail();

        // Navigate to login page
        await _page.GotoAsync( UrlBase + "/reset-password");
    }
    
    [Fact]
    public async Task ResetPassword_ShowError_WhenTokenInvalid()
    {
        Assert.Fail();
        // Navigate to login page
        await _page.GotoAsync( UrlBase + "/reset-password");
    }
    
        
    [Fact]
    public async Task Login_Navigate_WhenClickedButtonOnRegisterPage()
    {
        Assert.Fail();
        // Navigate to login page
        await _page.GotoAsync( UrlBase + "/reset-password");
    }
    
    [Fact]
    public async Task Register_Navigate_WhenClickedButtonOnLoginPage()
    {
        Assert.Fail();
        // Navigate to login page
        await _page.GotoAsync( UrlBase + "/reset-password");
    }
    
    [Fact]
    public async Task Register_ShowError_WhenEmptyName()
    {
        Assert.Fail();
        // Navigate to login page
        await _page.GotoAsync( UrlBase + "/reset-password");
    }
    
    [Fact]
    public async Task Register_ShowError_WhenPasswordTooWeak()
    {
        Assert.Fail();
        // Navigate to login page
        await _page.GotoAsync( UrlBase + "/reset-password");
    }
    
    [Fact]
    public async Task Register_ShowError_WhenPasswordsDontMatch()
    {
        Assert.Fail();
        // Navigate to login page
        await _page.GotoAsync( UrlBase + "/reset-password");
    }
    
    [Fact]
    public async Task Register_ShowError_WhenEmailWithInvalidFormat()
    {
        Assert.Fail();
        // Navigate to login page
        await _page.GotoAsync( UrlBase + "/reset-password");
    }
    
    [Fact]
    public async Task Register_ShowError_WhenUserExists()
    {
        Assert.Fail();
        // Navigate to login page
        await _page.GotoAsync( UrlBase + "/reset-password");
    }
    
    [Fact]
    public async Task Register_Successfully_WhenCredentialValid()
    {
        Assert.Fail();
        // Navigate to login page
        await _page.GotoAsync( UrlBase + "/reset-password");
    }

    [Fact]
    public async Task LeaveWorkspace_ButtonInactive_IfHasOwnerRole()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task LeaveWorkspace_Success_IfDoesntHaveOwnerRole()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task LeaveWorkspace_NoAssigneeForTask_WhenUserLeft()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task CreateWorkspace_Success_WhenUniqueName()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task CreateWorkspace_ShowError_WhenNamesDuplicate()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task CreateWorkspace_ShowError_WhenNameEmpty()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    
    [Fact]
    public async Task GrantAdminRoleToMember_Success_IfOwnerOfWorkspace()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task RevokeAdminRoleOfAdmin_Success_IfOwnerOfWorkspace()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }

    [Fact]
    public async Task DeleteWorkpace_Success_IfNoActiveBoardsExist()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task DeleteWorkpace_ShowError_IfActiveBoardsExist()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task AddMember_Success_IfUserWasntMember()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task AddMember_ShowEmptyPrompt_IfUserNotFoundByEmail()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task RemoveMember_Success_IfUserWasRemoved()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task RemoveMember_Blocked_IfUserHasOwnerOrAdminRole()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task CreateBoard_Success_WhenUniqueName()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task CreateBoard_Success_WhenEmptyName()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task CreateBoard_ShowError_WhenNamesDuplicate()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task ViewBoard_ArchivedBoardsHidden_WhenMemberRole()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task ViewBoard_ArchivedBoardsVisible_WhenMemberRole()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task ArchiveBoard_Success_WhenAdminRole()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task DeleteBoard_Success_WhenAdminRole()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task ViewBoards_ShowsTimerUntilDeleting_WhenBoardArchived()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task AddGuest_Success_WhenUserIsNotAGuestOnBoard()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task AddGuest_EmptyPrompt_WhenUserIsAlreadyAGuestOnBoard()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task AddGuest_Unavailable_WhenActorHasMemberRole()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task RemoveGuest_Success_WhenUserIsAlreadyAGuestOnBoard()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task RemoveGuest_Unavailable_WhenActorHasMemberRole()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task RestoreBoard_Success_WhenAdminRole()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task AllowedFiles_Success_WhenAdminRole()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task AllowedFiles_Unavailable_WhenMemberRole()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task AllowedFiles_DontChange_WhenCancel()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task AllowedFiles_SelectSubitems_WhenParentItemChecked()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task AllowedFiles_UnselectSubitems_WhenParentItemUnchecked()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task AllowedFiles_Change_WhenSave()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    
    [Fact]
    public async Task ViewBoard_Success_WhenAllowedAsGuest()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task ViewBoard_ShowErrorMessage_WhenNotAllowedAsGuest()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task CreateStatus_Unavailable_WhenGuest()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task CreateStatus_ShowError_WhenEmptyName()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task CreateStatus_Success_WhenEnteredCorrectName()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task CreateStatus_NoIndicator_WhenLimitNotSet()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task CreateStatus_ShowIndicator_WhenSetLimit()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task MoveTask_Success_WhenNoAssigneeOnTask()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task MoveTask_NoResult_WhenAssigneeDifferentUser()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task MoveTask_Success_WhenAssignedToCurrentUser()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task CreateTask_EmptyList_WhenUserNotFound()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task CreateTask_EmptyList_WhenUserNotMemberOfWorkspace()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task CreateTask_AssigneeSet_WhenUserMemberOfWorkspace()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task CreateTask_AssigneeUnset_WhenClickedRemoveButton()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task UploadFile_NotAllowed_WhenGuestRole()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task UploadFile_ShowError_WhenFileTooBig()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task UploadFile_ShowError_WhenFileHasNotAllowedExtension()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }

    [Fact]
    public async Task UploadFile_Success_WhenFileIsLessThan10MbAndHasAllowedExtension()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task DownloadFile_Success_WhenClickOnLoadButton()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task RemoveAttachment_Success_WhenMemberRole()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task RemoveAttachment_Unavailable_WhenGuestRole()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task SetDueDate_Unavailable_WhenGuestRole()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task SetDueDate_NotSelect_WhenDatePassed()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task SetDueDate_NotSet_WhenUnchecked()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task SetDueDate_Success_WhenFutureDateAndChecked()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task SelectTag_SelectedExisting_WhenExists()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task SelectTag_CreatedNew_WhenDoestExists()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task RemoveTag_RemovedFromSuggestions_WhenDoestExistOnDifferentTasks()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task RemoveTag_StaysInSuggestions_WhenExistOnDifferentTasks()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task DragTask_RepositionInStatus_WhenMoveHorizontally()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task DragTask_ChangeStatus_WhenMoveVertically()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task DragStatus_ChangePosition_WhenMoveHorizontally()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task ChangeLanguage_SetToUkrainian_WhenClickedButton()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task ChangeLanguage_SetToEnglish_WhenClickedButton()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task MembersDisplay_OwnerHasIcon()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task MembersDisplay_OwnerHasDifferentStyle()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task MembersDisplay_AdminAndMemberHaveSameStyle()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task ChangeLanguage_SaveSettings_WhenPageReloaded()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task DeleteAccount_Success_IfNoWorkspacesExist()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task DeleteAccount_AskConfirmation_WhenSelected()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
    
    [Fact]
    public async Task DeleteAccount_ShowsError_IfWorkspacesExist()
    {
        await _page.GotoAsync(UrlBase +"/me");
       
        Assert.Fail();
    }
}