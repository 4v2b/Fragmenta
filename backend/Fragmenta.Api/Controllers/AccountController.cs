using System.Security.Claims;
using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
using Fragmenta.Api.Middleware;
using Fragmenta.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fragmenta.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/me")]
public class AccountController : ControllerBase
{
    private long? GetAuthenticatedUserId()
    {
        var idString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (int.TryParse(idString, out var id))
        {
            return id;
        }

        return null;
    }

    [HttpGet]
    public async Task<IActionResult> Me([FromServices] IUserLookupService lookoutService)
    {
        var id = GetAuthenticatedUserId();

        if (id != null)
        {
            var user = await lookoutService.GetUserInfoAsync(id.Value);

            if (user is null)
            {
                return NotFound();
            }
            
            return Ok(user);
        }
        return Unauthorized("User was not found");
    }
    
    [HttpDelete]
    public async Task<IActionResult> DeleteAccount([FromQuery] string password, [FromServices] IUserAccountService accountService)
    {
        var id = GetAuthenticatedUserId();

        if (id != null)
        {
            var result = await accountService.DeleteAsync(password, id.Value);

            if (result)
            {
                return NoContent();
            }
            
            return Forbid();
        }
        return Unauthorized("User was not found");
    }

    [HttpPost("name")]
    public async Task<IActionResult> ChangeName([FromQuery] string newName, [FromServices] IUserAccountService accountService)
    {
        var id = GetAuthenticatedUserId();

        if (id != null)
        {
            var result = await accountService.ChangeNameAsync(newName, id.Value);

            if (result)
            {
                return NoContent();
            }
            
            return BadRequest();
        }
        return Unauthorized("User was not found");
    }
    
    [HttpPost("password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, [FromServices] IUserAccountService accountService)
    {
        var id = GetAuthenticatedUserId();

        if (id != null)
        {
            var result = await accountService.ChangePasswordAsync(request.NewPassword, request.OldPassword, id.Value);

            if (result)
            {
                return NoContent();
            }
            
            return BadRequest();
        }
        return Unauthorized("User was not found");
    }
   
}