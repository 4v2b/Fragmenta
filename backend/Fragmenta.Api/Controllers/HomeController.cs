using Fragmenta.Api.Contracts;
using Fragmenta.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fragmenta.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/users")]
    public class HomeController : ControllerBase
    {
        /// <summary>
        /// Searches for users with given emails
        /// </summary>
        /// <response code="200">List of ids and emails of found users</response>
        [HttpGet("lookup")]
        public IActionResult UserLookup([FromQuery]string email, [FromServices] IUserLookupService userService)
        {
            var users = userService.FindByEmail(email);

            return Ok(users);
        }
    }
}
