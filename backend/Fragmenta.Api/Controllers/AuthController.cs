using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fragmenta.Api.Controllers
{
    // TODO Make refresh token renew instead of generating one every time

    [ApiController]
    [Route("/api/[action]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        /// <summary>
        /// Generates a new access token based on refresh token
        /// </summary>
        /// <response code="200">Returns new access token</response>
        /// <response code="401">If access token is invalid or expired</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Refresh([FromBody] RefreshRequest model, [FromServices] IRefreshTokenService refreshService, [FromServices] IJwtService jwtService)
        {
            var user = refreshService.GetUserByToken(model.RefreshToken);

            if (user == null)
            {
                return Unauthorized();
            }

            return refreshService.VerifyToken(model.RefreshToken, user.Id) switch
            {
                Enums.RefreshTokenStatus.Valid or Enums.RefreshTokenStatus.Expired
                    => Ok(jwtService.GenerateToken(new UserDto() { Email = user.Email, Id = user.Id })),
                _ => Unauthorized()
            };
        }

        /// <summary>
        /// Logs in a user into system
        /// </summary>
        /// <response code="200">Returns new access and refresh tokens</response>
        /// <response code="400">If an error happened during request</response>
        /// <response code="401">If user was not found or could not generate a refresh token</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        // TODO Simultaneous login from different devices
        public IActionResult Login(
            [FromBody] LoginRequest model,
            [FromServices] IJwtService jwtService,
            [FromServices] IUserService userService,
            [FromServices] IRefreshTokenService refreshService
        )
        {
            try
            {
                var user = userService.Authorize(model);
                if (user == null) return Unauthorized();

                refreshService.RevokeTokens(user.Id);

                var refreshToken = refreshService.GenerateToken(user.Id);
                if (refreshToken == null) return BadRequest("Failed to generate refresh token");

                return Ok(new TokenResponse
                {
                    AccessToken = jwtService.GenerateToken(user),
                    RefreshToken = refreshToken
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <response code="200">Returns new access and refresh tokens</response>
        /// <response code="400">If an error happened during adding a user to database</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Register(
            [FromBody] RegisterRequest model,
            [FromServices] IJwtService jwtService,
            [FromServices] IUserService userService,
            [FromServices] IRefreshTokenService refreshService
        )
        {
            try
            {
                var user = userService.Register(model);

                if (user != null)
                {
                    var refreshToken = refreshService.GenerateToken(user.Id);

                    if (refreshToken != null)
                    {
                        var response = new TokenResponse()
                        {
                            AccessToken = jwtService.GenerateToken(user),
                            RefreshToken = refreshToken
                        };

                        return Ok(response);
                    }
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
