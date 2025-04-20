using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
using Fragmenta.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;

namespace Fragmenta.Api.Controllers
{
    // TODO Make refresh token renew instead of generating one every time

    [ApiController]
    [Route("/api")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        /// <summary>
        /// Generates a new access token based on refresh token
        /// </summary>
        /// <response code="200">Returns new access token</response>
        /// <response code="401">If access token is invalid or expired</response>
        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Refresh([FromBody] RefreshRequest model, [FromServices] IRefreshTokenLookupService lookupService, [FromServices] IRefreshTokenService refreshService, [FromServices] IJwtService jwtService)
        {
            var user = lookupService.GetUserByToken(model.RefreshToken);

            if (user == null)
            {
                return Unauthorized();
            }

            return refreshService.VerifyToken(model.RefreshToken, user.Id) switch
            {
                Enums.RefreshTokenStatus.Valid
                    => Ok(jwtService.GenerateToken(new UserDto() { Email = user.Email, Id = user.Id })),
                _ => Unauthorized()
            };
        }

        /// <summary>
        /// Logs in a user into system
        /// </summary>
        /// <response code="200">Returns new access and refresh tokens</response>
        /// <response code="400">If an error happened during request</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        // TODO Simultaneous login from different devices
        public IActionResult Login(
            [FromBody] LoginRequest model,
            [FromServices] IJwtService jwtService,
            [FromServices] IAuthService authService,
            [FromServices] IRefreshTokenService refreshService
        )
        {
            try
            {
                var result = authService.Authorize(model);

                if (result.IsLocked)
                    return StatusCode(423, new { message = "auth.errors.lockout", lockoutUntil = result.LockedUntil });

                if (!result.IsSuccess || result.User is null)
                    return BadRequest(new
                    {
                        message = result.Error switch
                        {
                            Enums.AuthErrorType.PasswordInvalid => "auth.errors.passwordInvalid",
                            Enums.AuthErrorType.UserNonExistent => "auth.errors.userDoesntExist",
                            _ => "auth.errors.unknown"
                        }
                    });

                refreshService.RevokeTokens(result.User.Id);

                var refreshToken = refreshService.GenerateToken(result.User.Id);

                if (refreshToken == null) return BadRequest("Failed to generate refresh token");

                return Ok(new TokenResponse
                {
                    AccessToken = jwtService.GenerateToken(result.User),
                    RefreshToken = refreshToken
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <response code="200">Returns new access and refresh tokens</response>
        /// <response code="400">If an error happened during adding a user to database</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Register(
            [FromBody] RegisterRequest model,
            [FromServices] IJwtService jwtService,
            [FromServices] IAuthService authService,
            [FromServices] IRefreshTokenService refreshService
        )
        {
            try
            {
                var result = authService.Register(model);

                if (result is { IsSuccess: true, User: not null })
                {
                    var refreshToken = refreshService.GenerateToken(result.User.Id);

                    if (refreshToken != null)
                    {
                        var response = new TokenResponse()
                        {
                            AccessToken = jwtService.GenerateToken(result.User),
                            RefreshToken = refreshToken
                        };

                        return Ok(response);
                    }
                }

                return BadRequest(new
                {
                    message = result.Error switch
                    {
                        Enums.AuthErrorType.UserExists => "auth.errors.userExists",
                        _ => "auth.errors.unknown"
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ForgotPassword(
            [FromQuery][EmailAddress(ErrorMessage = "auth.errors.emailInvalid")] string email,
            [FromServices] IUserLookupService lookupService,
            [FromServices] IResetTokenService resetService,
            [FromServices] IMailingService mailingService
        )
        {
            try
            {
                var userId = lookupService.FindSingleByEmail(email);

                if (userId.HasValue)
                {
                    var token = resetService.GenerateToken(userId.Value);

                    var content = MailBodyFormer.CreateResetPasswordTextBody("http://localhost:5173", token, userId.Value);

                    var result = await mailingService.SendEmailAsync(email, content);

                    if (result.IsLocked)
                        return StatusCode(423, new { message = "auth.errors.lockout", lockoutUntil = result.LockedUntil });

                    if (!result.IsSuccess)
                        return BadRequest(new
                        {
                            message = result.ErrorType switch
                            {
                                Enums.EmailSendErrorType.SendingError => "auth.errors.cannotSendEmail",
                                _ => "auth.errors.unknown"
                            }
                        });

                    if (result.IsSuccess)
                        return Ok();

                }

                return BadRequest(new
                {
                    message = "auth.errors.userDoesntExist"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ResetPassword(
            [FromBody] ResetPasswordRequest request,
            [FromServices] IUserAccountService userService,
            [FromServices] IResetTokenService resetService
        )
        {
            try
            {
                //var decodedToken = WebUtility.UrlDecode(request.Token);

                if (!resetService.VerifyAndDestroyToken(request.Token, request.UserId))
                {
                    return BadRequest(new
                    {
                        message = "auth.errors.invalidResetToken"
                    });
                }

                if (!userService.ResetPassword(request.NewPassword, request.UserId))
                {
                    return BadRequest(new
                    {
                        message = "auth.errors.userDoesntExist"
                    });
                }

                return Ok();

            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
