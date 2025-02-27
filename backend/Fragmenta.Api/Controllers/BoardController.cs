using Azure.Core;
using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
using Fragmenta.Api.Enums;
using Fragmenta.Api.Services;
using Fragmenta.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fragmenta.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/workspaces")]
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

        [HttpGet("{workspaceId}/boards")]
        public IActionResult GetBoards(long workspaceId, [FromServices] IBoardService boardService, [FromServices] IWorkspaceAccessService accessService)
        {
            var id = GetAuthenticatedUserId();

            if (id != null)
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

        [HttpPost("{workspaceId}/boards")]
        public IActionResult CreateBoard(long workspaceId, [FromBody] CreateBoardRequest request, [FromServices] IBoardService boardService, [FromServices] IWorkspaceAccessService accessService)
        {
            var id = GetAuthenticatedUserId();

            if (id != null)
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
    }
}
