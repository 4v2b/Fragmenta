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
    [Route("api/statuses")]
    public class StatusController : ControllerBase
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

        /*[HttpGet]
        public IActionResult GetStatuses([FromQuery] long boardId, [FromServices] IStatusService statusService, [FromServices] IWorkspaceAccessService accessService)
        {
            var id = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && id != null)
            {
                var role = accessService.GetRole(workspaceId, id.Value);

                if (role == null || !AccessCheck.CanManageStatuses(role.Value))
                {
                    return Forbid();
                }

                var result = statusService.GetStatuses(boardId);

                return Ok(result);
            }

            return Unauthorized("User was not found");
        }*/

        [HttpPost]
        public IActionResult CreateStatus([FromQuery] long boardId, [FromBody] CreateOrUpdateStatusRequest request,[FromServices] IStatusService statusService, [FromServices] IWorkspaceAccessService accessService)
        {
            var id = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && id != null)
            {
                var role = accessService.GetRole(workspaceId, id.Value);

                if (role == null || !AccessCheck.CanManageStatuses(role.Value))
                {
                    return Forbid();
                }

                var result = statusService.CreateStatus(boardId, request);

                if(result != null)
                {
                    return CreatedAtAction(nameof(CreateStatus), result);
                }

                return BadRequest();
            }

            return Unauthorized("User was not found");
        }

        [HttpPut("{statusId}")]
        public IActionResult UpdateStatus(long statusId, [FromBody] CreateOrUpdateStatusRequest request, [FromServices] IStatusService statusService, [FromServices] IWorkspaceAccessService accessService)
        {
            var id = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && id != null)
            {
                var role = accessService.GetRole(workspaceId, id.Value);

                if (role == null || !AccessCheck.CanManageStatuses(role.Value))
                {
                    return Forbid();
                }

                var result = statusService.UpdateStatus(statusId, request);

                if (result != null)
                {
                    return Ok(result);
                }

                return BadRequest();
            }

            return Unauthorized("User was not found");
        }

        [HttpDelete("{statusId}")]
        public IActionResult DeleteStatus(long statusId, [FromServices] IStatusService statusService, [FromServices] IWorkspaceAccessService accessService)
        {
            var id = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && id != null)
            {
                var role = accessService.GetRole(workspaceId, id.Value);

                if (role == null || !AccessCheck.CanManageStatuses(role.Value))
                {
                    return Forbid();
                }

                var result = statusService.DeleteStatus(statusId);

                if (result)
                {
                    return NoContent();
                }

                return BadRequest("Cannot delete status with dependent tasks");
            }

            return Unauthorized("User was not found");
        }
    }
}
