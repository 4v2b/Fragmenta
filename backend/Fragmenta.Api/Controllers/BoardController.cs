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
        public IActionResult GetBoards([FromServices] IBoardService boardService, [FromServices] IWorkspaceAccessService accessService)
        {
            var id = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && id != null)
            {
                var role = accessService.GetRole(workspaceId, id.Value);

                if (role == null)
                {
                    return Forbid();
                }

                if (role == Role.Guest)
                {
                    return Ok(boardService.GetGuestBoards(workspaceId, id.Value));
                }

                return Ok(boardService.GetBoards(workspaceId));
            }

            return Unauthorized("User was not found");
        }

        [HttpGet("{boardId}")]
        public IActionResult GetBoard(long boardId,[FromServices] IBoardService boardService, [FromServices] IStatusService statusService, [FromServices] IWorkspaceAccessService accessService)
        {
            var id = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && id != null)
            {
                var role = accessService.GetRole(workspaceId, id.Value);

                if (role == null || (role == Role.Guest && !boardService.CanViewBoard(boardId, id.Value)))
                {
                    return Forbid();
                }

                var result = statusService.GetStatuses(boardId);
                
                if(result == null)
                {
                    return BadRequest();
                }

                return Ok(result);
            }

            return Unauthorized("User was not found");
        }

        [HttpPost]
        public IActionResult CreateBoard([FromBody] CreateBoardRequest request, [FromServices] IBoardService boardService, [FromServices] IWorkspaceAccessService accessService)
        {
            var id = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && id != null)
            {
                var role = accessService.GetRole(workspaceId, id.Value);

                if (role == null || !AccessCheck.CanCreateBoard(role.Value))
                {
                    return Forbid();
                }

                var result = boardService.CreateBoard(workspaceId, request);

                if (result != null)
                {
                    return CreatedAtAction(nameof(CreateBoard), result);
                }

                return BadRequest();
            }
            return Unauthorized("User was not found");
        }

        [HttpPut("{boardId}")]
        public IActionResult UpdateBoard(long boardId, [FromBody] UpdateBoardRequest request, [FromServices] IBoardService boardService, [FromServices] IWorkspaceAccessService accessService)
        {
            var id = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && id != null)
            {
                var role = accessService.GetRole(workspaceId, id.Value);

                if (role == null || !AccessCheck.CanUpdateBoard(role.Value))
                {
                    return Forbid();
                }

                var result = boardService.UpdateBoard(boardId, request);

                if (result != null)
                {
                    return Ok(result);
                }

                return BadRequest();
            }

            return Unauthorized("User was not found");
        }
        
        [HttpDelete("{boardId:long}")]
        public IActionResult DeleteBoard(long boardId,[FromServices] IBoardService boardService, [FromServices] IWorkspaceAccessService accessService)
        {
            var id = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && id != null)
            {
                var role = accessService.GetRole(workspaceId, id.Value);

                if (role == null || !AccessCheck.CanUpdateBoard(role.Value))
                {
                    return Forbid();
                }

                var result = boardService.DeleteBoard(boardId);

                if (result)
                {
                    return NoContent();
                }

                return BadRequest();
            }

            return Unauthorized("User was not found");
        }

        [HttpPost("{boardId:long}/guests")]
        public IActionResult AddGuests(long boardId, [FromBody] AddGuestsRequest request, [FromServices] IBoardService boardService, [FromServices] IWorkspaceAccessService accessService)
        {
            var id = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && id != null)
            {
                var role = accessService.GetRole(workspaceId, id.Value);

                if (role == null || !AccessCheck.CanManageGuests(role.Value))
                {
                    return Forbid();
                }

                var result = boardService.AddGuests(boardId, request.UsersId);

                if (result.Count > 0)
                {
                    return CreatedAtAction(nameof(CreateBoard), result);
                }

                return BadRequest();
            }

            return Unauthorized("User was not found");
        }
        
        [HttpGet("{boardId:long}/guests")]
        public IActionResult GetGuests(long boardId, [FromServices] IBoardService boardService, [FromServices] IWorkspaceAccessService accessService)
        {
            var id = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && id != null)
            {
                var role = accessService.GetRole(workspaceId, id.Value);

                if (role == null || (role == Role.Guest && !boardService.CanViewBoard(boardId, id.Value)))
                {
                    return Forbid();
                }

                var guests = boardService.GetGuests(boardId);

                return Ok(guests);
            }

            return Unauthorized("User was not found");
        }

        [HttpDelete("{boardId:long}/guests/{guestId:long}")]
        public IActionResult RemoveGuests(long boardId, long guestId, [FromServices] IBoardService boardService, [FromServices] IWorkspaceAccessService accessService)
        {
            var id = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && id != null)
            {
                var role = accessService.GetRole(workspaceId, id.Value);

                if (role == null || !AccessCheck.CanManageGuests(role.Value))
                {
                    return Forbid();
                }

                var result = boardService.RemoveGuest(boardId, guestId);

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
