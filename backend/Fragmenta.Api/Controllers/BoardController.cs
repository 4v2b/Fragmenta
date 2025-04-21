using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
using Fragmenta.Api.Enums;
using Fragmenta.Api.Middleware;
using Fragmenta.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fragmenta.Api.Controllers
{
    [Authorize]
    [ApiController]
    [ServiceFilter(typeof(WorkspaceFilter))]
    [Route("api/boards")]
    public class BoardController : ControllerBase
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
        public async Task<IActionResult> GetBoards([FromServices] IBoardService boardService, [FromServices] IWorkspaceAccessService accessService)
        {
            var id = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && id != null)
            {
                var role = await accessService.GetRoleAsync(workspaceId, id.Value);

                if (role == null)
                {
                    return Forbid();
                }

                if (role == Role.Guest)
                {
                    return Ok(await boardService.GetGuestBoardsAsync(workspaceId, id.Value));
                }

                return Ok(await boardService.GetBoardsAsync(workspaceId));
            }

            return Unauthorized("User was not found");
        }

        [HttpGet("{boardId}")]
        public async Task<IActionResult> GetBoard(long boardId, [FromServices] IBoardAccessService boardAccessService, [FromServices] IBoardService boardService, [FromServices] IWorkspaceAccessService accessService)
        {
            var id = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && id != null)
            {
                var role = await accessService.GetRoleAsync(workspaceId, id.Value);

                if (role == null || (role == Role.Guest && !await boardAccessService.CanViewBoardAsync(boardId, id.Value)))
                {
                    return Forbid();
                }

                var result = await boardService.GetBoardAsync(boardId);
                
                if(result == null)
                {
                    return BadRequest();
                }

                return Ok(result);
            }

            return Unauthorized("User was not found");
        }

        [HttpPost]
        public async Task<IActionResult> CreateBoard([FromBody] CreateBoardRequest request, [FromServices] IBoardService boardService, [FromServices] IWorkspaceAccessService accessService)
        {
            var id = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && id != null)
            {
                var role = await accessService.GetRoleAsync(workspaceId, id.Value);

                if (role == null || !AccessCheck.CanCreateBoard(role.Value))
                {
                    return Forbid();
                }

                var result = await boardService.CreateBoardAsync(workspaceId, request);

                if (result != null)
                {
                    return CreatedAtAction(nameof(CreateBoard), result);
                }

                return BadRequest();
            }
            return Unauthorized("User was not found");
        }

        [HttpPut("{boardId}")]
        public async Task<IActionResult> UpdateBoard(long boardId, [FromBody] UpdateBoardRequest request, [FromServices] IBoardService boardService, [FromServices] IWorkspaceAccessService accessService)
        {
            var id = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && id != null)
            {
                var role = await accessService.GetRoleAsync(workspaceId, id.Value);

                if (role == null || !AccessCheck.CanUpdateBoard(role.Value))
                {
                    return Forbid();
                }

                var result = await boardService.UpdateBoardAsync(boardId, request);

                if (result != null)
                {
                    return Ok(result);
                }

                return BadRequest();
            }

            return Unauthorized("User was not found");
        }
        
        [HttpDelete("{boardId:long}")]
        public async Task<IActionResult> DeleteBoard(long boardId,[FromServices] IBoardService boardService, [FromServices] IWorkspaceAccessService accessService)
        {
            var id = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && id != null)
            {
                var role = await accessService.GetRoleAsync(workspaceId, id.Value);

                if (role == null || !AccessCheck.CanUpdateBoard(role.Value))
                {
                    return Forbid();
                }

                var result = await boardService.DeleteBoardAsync(boardId);

                if (result)
                {
                    return NoContent();
                }

                return BadRequest();
            }

            return Unauthorized("User was not found");
        }

        [HttpPost("{boardId:long}/guests")]
        public async Task<IActionResult> AddGuests(long boardId, [FromBody] AddGuestsRequest request, [FromServices] IBoardAccessService boardAccessService, [FromServices] IWorkspaceAccessService accessService)
        {
            var id = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && id != null)
            {
                var role = await accessService.GetRoleAsync(workspaceId, id.Value);

                if (role == null || !AccessCheck.CanManageGuests(role.Value))
                {
                    return Forbid();
                }

                var result = await boardAccessService.AddGuestsAsync(boardId, request.UsersId);

                if (result.Count > 0)
                {
                    return CreatedAtAction(nameof(CreateBoard), result);
                }

                return BadRequest();
            }

            return Unauthorized("User was not found");
        }
        
        [HttpGet("{boardId:long}/guests")]
        public async Task<IActionResult> GetGuests(long boardId, [FromServices] IBoardAccessService boardAccessService, [FromServices] IWorkspaceAccessService accessService)
        {
            var id = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && id != null)
            {
                var role = await accessService.GetRoleAsync(workspaceId, id.Value);

                if (role == null || (role == Role.Guest && !await boardAccessService.CanViewBoardAsync(boardId, id.Value)))
                {
                    return Forbid();
                }

                var guests = await boardAccessService.GetGuestsAsync(boardId);

                return Ok(guests);
            }

            return Unauthorized("User was not found");
        }

        [HttpDelete("{boardId:long}/guests/{guestId:long}")]
        public async Task<IActionResult> RemoveGuests(long boardId, long guestId, [FromServices] IBoardAccessService boardAccessService, [FromServices] IWorkspaceAccessService accessService)
        {
            var id = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && id != null)
            {
                var role = await accessService.GetRoleAsync(workspaceId, id.Value);

                if (role == null || !AccessCheck.CanManageGuests(role.Value))
                {
                    return Forbid();
                }

                var result = await boardAccessService.RemoveGuestAsync(boardId, guestId);

                if (result)
                {
                    return NoContent();
                }

                return BadRequest();
            }

            return Unauthorized("User was not found");
        }
    }
}
