using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
using Fragmenta.Api.Middleware;
using Fragmenta.Api.Utils;
using Fragmenta.Dal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Role = Fragmenta.Api.Enums.Role;

namespace Fragmenta.Api.Controllers
{
#pragma warning disable CS1591
    [Authorize]
    [ApiController]
    [ServiceFilter(typeof(WorkspaceFilter))]
    [Route("api/members")]
    public class AccessController : ControllerBase
    {
        private long? GetAuthenticatedUserId()
        {
            var idString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(idString, out var id))
                return id;

            return null;
        }

        /// <summary>
        /// Returns all members of the workspace
        /// </summary>
        /// <param name="id">Workspace id</param>
        /// <response code="200">Deletion is successful</response>
        /// <response code="401">If user is unauthorized</response>
        /// <response code="403">If user has no permission for the action</response>
        [HttpGet]
        public async Task<IActionResult> GetMembers([FromServices] IWorkspaceAccessService accessService)
        {
            var userId = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && userId != null)
            {
                var role = await accessService.GetRoleAsync(workspaceId, userId.Value);

                if (role == null)
                    return Forbid();

                List<MemberDto> members = await accessService.GetMembersAsync(workspaceId);

                return Ok(members);
            }
            return Unauthorized("User was not found");
        }

        /// <summary>
        /// Adds user the workspace as a member
        /// </summary>
        /// <response code="201">Returns added member</response>
        /// <response code="400">If member cannot be added to workspace</response>
        /// <response code="401">If user is unauthorized</response>
        /// <response code="403">If user has no permission for the action</response>
        [HttpPost]
        public async Task<IActionResult> AddMembers([FromBody] MemberRequest request, [FromServices] IWorkspaceAccessService accessService)
        {
            var userId = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && userId != null)
            {
                var role = await accessService.GetRoleAsync(workspaceId, userId.Value);

                if (role == null || !AccessCheck.CanAddMember(role.Value))
                    return Forbid();

                var addedMembers = await accessService.AddMembersAsync(workspaceId, request.UsersId);

                if (addedMembers == null)
                    return BadRequest();

                return CreatedAtAction(nameof(AddMembers), addedMembers);
            }

            return Unauthorized("User was not found");
        }

        /// <summary>
        /// Adds user the workspace as a member
        /// </summary>
        /// <response code="204">Deletion is successful</response>
        /// <response code="400">If no member found for give workspace</response>
        /// <response code="401">If user is unauthorized</response>
        /// <response code="403">If user has no permission for the action</response>
        [HttpDelete("{memberId}")]
        public async Task<IActionResult> RemoveMember(long memberId, [FromServices] IWorkspaceAccessService accessService)
        {
            var actorId = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && actorId != null)
            {
                var actorRole = await accessService.GetRoleAsync(workspaceId, actorId.Value);

                var memberRole = await accessService.GetRoleAsync(workspaceId, memberId);

                if (memberRole == null)
                    return BadRequest();

                if (actorRole == null 
                        || (!AccessCheck.CanDeleteMember(actorRole.Value, memberRole.Value) && actorId != memberId)
                        || (actorId == memberId && actorRole.Value == Role.Owner))
                    return Forbid();

                if (!await accessService.DeleteMemberAsync(workspaceId, memberId))
                    return BadRequest();

                return NoContent();
            }
            return Unauthorized("User was not found");
        }

        /// <summary>
        /// Revokes admin permissions of a user the workspace
        /// </summary>
        /// <response code="204">Deletion is successful</response>
        /// <response code="400">If no member found for give workspace</response>
        /// <response code="401">If user is unauthorized</response>
        /// <response code="403">If user has no permission for the action</response>
        [HttpPost("/members/{memberId}/revoke")]
        public async Task<IActionResult> RevokeAdmin(long memberId, [FromServices] IWorkspaceAccessService accessService)
        {
            var actorId = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && actorId != null)
            {
                var actorRole = await accessService.GetRoleAsync(workspaceId, actorId.Value);

                var memberRole = await accessService.GetRoleAsync(workspaceId, memberId);

                if (memberRole == null)
                    return BadRequest();

                if (actorRole == null || !AccessCheck.CanRevokeAdminPermission(actorRole.Value, memberRole.Value))
                    return Forbid();

                if (!await accessService.RevokeAdminPermissionAsync(workspaceId, memberId))
                    return BadRequest();

                return NoContent();
            }
            return Unauthorized("User was not found");
        }

        /// <summary>
        /// Grants admin permissions of a user the workspace
        /// </summary>
        /// <response code="204">Deletion is successful</response>
        /// <response code="400">If no member found for give workspace</response>
        /// <response code="401">If user is unauthorized</response>
        /// <response code="403">If user has no permission for the action</response>
        [HttpPost("/members/{memberId}/grant")]
        public async Task<IActionResult> GrantAdmin(long memberId, [FromServices] IWorkspaceAccessService accessService)
        {
            var actorId = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && actorId != null)
            {
                var actorRole = await accessService.GetRoleAsync(workspaceId, actorId.Value);

                var memberRole = await accessService.GetRoleAsync(workspaceId, memberId);

                if (memberRole == null)
                    return BadRequest();

                if (actorRole == null || !AccessCheck.CanGrantAdminPermission(actorRole.Value, memberRole.Value))
                    return Forbid();

                if (! await accessService.GrantAdminPermissionAsync(workspaceId, memberId))
                    return BadRequest();

                return NoContent();
            }
            return Unauthorized("User was not found");
        }
    }
}
