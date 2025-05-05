using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
using Fragmenta.Api.Middleware;
using Fragmenta.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Fragmenta.Api.Enums;

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
        public async Task<IActionResult> GetTags([FromQuery] long boardId, [FromServices] ITagService tagService, [FromServices] IBoardAccessService boardAccessService, [FromServices] IWorkspaceAccessService accessService)
        {
            var id = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && id != null)
            {
                var role = await accessService.GetRoleAsync(workspaceId, id.Value);

                if (role == null || (role == Role.Guest && !await boardAccessService.CanViewBoardAsync(boardId, id.Value)))
                {
                    return Forbid();
                }

                var result = await tagService.GetTagsAsync(boardId);

                return Ok(result);
            }

            return Unauthorized("User was not found");
        }

        [HttpPost]
        public async Task<IActionResult> CreateTag([FromQuery] string name, [FromQuery] long boardId, [FromServices] ITagService tagService, [FromServices] IWorkspaceAccessService accessService)
        {
            var id = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && id != null)
            {
                var role = await accessService.GetRoleAsync(workspaceId, id.Value);

                if (role == null || !AccessCheck.CanManageBoardContent(role.Value))
                {
                    return Forbid();
                }

                var result = await tagService.CreateTagAsync(name, boardId);

                if (result != null)
                {
                    return CreatedAtAction(nameof(CreateTag), result);
                }

                return BadRequest();
            }

            return Unauthorized("User was not found");
        }

        [HttpDelete("{tagId}")]
        public async Task<IActionResult> DeleteTag(long tagId, [FromServices] ITagService tagService, [FromServices] IWorkspaceAccessService accessService)
        {
            var id = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && id != null)
            {
                var role = await accessService.GetRoleAsync(workspaceId, id.Value);

                if (role == null || !AccessCheck.CanManageStatuses(role.Value))
                {
                    return Forbid();
                }

                var result = await tagService.DeleteTagAsync(tagId);

                if (result)
                {
                    return NoContent();
                }

                return NotFound();
            }

            return Unauthorized("User was not found");
        }
    }
}
