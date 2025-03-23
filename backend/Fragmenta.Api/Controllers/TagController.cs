using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
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
    [Route("api/tags")]
    public class TagController : ControllerBase
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
        public IActionResult GetTags([FromQuery] long boardId, [FromServices] ITagService tagService, [FromServices] IWorkspaceAccessService accessService)
        {
            var id = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && id != null)
            {
                var role = accessService.GetRole(workspaceId, id.Value);

                if (role == null || !AccessCheck.CanManageBoardContent(role.Value))
                {
                    return Forbid();
                }

                var result = tagService.GetTags(boardId);

                return Ok(result);
            }

            return Unauthorized("User was not found");
        }

        [HttpPost]
        public IActionResult CreateTag([FromQuery] string name, [FromQuery] long boardId, [FromServices] ITagService tagService, [FromServices] IWorkspaceAccessService accessService)
        {
            var id = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && id != null)
            {
                var role = accessService.GetRole(workspaceId, id.Value);

                if (role == null || !AccessCheck.CanManageBoardContent(role.Value))
                {
                    return Forbid();
                }

                var result = tagService.CreateTag(name, boardId);

                if (result != null)
                {
                    return CreatedAtAction(nameof(CreateTag), result);
                }

                return BadRequest();
            }

            return Unauthorized("User was not found");
        }

        [HttpDelete("{tagId}")]
        public IActionResult DeleteTask(long tagId, [FromServices] ITagService tagService, [FromServices] IWorkspaceAccessService accessService)
        {
            var id = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && id != null)
            {
                var role = accessService.GetRole(workspaceId, id.Value);

                if (role == null || !AccessCheck.CanManageStatuses(role.Value))
                {
                    return Forbid();
                }

                var result = tagService.DeleteTag(tagId);

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
