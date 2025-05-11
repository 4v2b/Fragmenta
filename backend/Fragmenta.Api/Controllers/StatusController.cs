using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
using Fragmenta.Api.Middleware;
using Fragmenta.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Fragmenta.Api.Controllers
{
    [Authorize]
    [ApiController]
    [ServiceFilter(typeof(WorkspaceFilter))]
    [Route("api/statuses")]
    public class StatusController(IHubContext<BoardHub> hubContext)  : ControllerBase
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

        [HttpPost]
        public async Task<IActionResult> CreateStatus([FromQuery] long boardId, [FromBody] CreateOrUpdateStatusRequest request,[FromServices] IStatusService statusService, [FromServices] IWorkspaceAccessService accessService)
        {
            var id = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && id != null)
            {
                var role = await accessService.GetRoleAsync(workspaceId, id.Value);

                if (role == null || !AccessCheck.CanManageStatuses(role.Value))
                {
                    return Forbid();
                }

                var result = await statusService.CreateStatusAsync(boardId, request);

                if(result != null)
                {
                    if (Request.Headers.TryGetValue("X-Board-Id", out var hubId) 
                        && !string.IsNullOrWhiteSpace(hubId))
                    {
                        await hubContext.Clients.Group($"{hubId}")
                            .SendAsync("StatusCreated", result);
                    }
                    
                    return CreatedAtAction(nameof(CreateStatus), result);
                }

                return BadRequest();
            }

            return Unauthorized("User was not found");
        }

        [HttpPut("{statusId}")]
        public async Task<IActionResult> UpdateStatus(long statusId, [FromBody] CreateOrUpdateStatusRequest request, [FromServices] IStatusService statusService, [FromServices] IWorkspaceAccessService accessService)
        {
            var id = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && id != null)
            {
                var role = await accessService.GetRoleAsync(workspaceId, id.Value);

                if (role == null || !AccessCheck.CanManageStatuses(role.Value))
                {
                    return Forbid();
                }

                var result = await statusService.UpdateStatusAsync(statusId, request);

                if (result != null)
                {
                    if (Request.Headers.TryGetValue("X-Board-Id", out var boardId) 
                        && !string.IsNullOrWhiteSpace(boardId))
                    {
                        await hubContext.Clients.Group($"{boardId}")
                            .SendAsync("StatusUpdated", result);
                    }
                    
                    return Ok(result);
                }

                return BadRequest();
            }

            return Unauthorized("User was not found");
        }

        [HttpDelete("{statusId}")]
        public async Task<IActionResult> DeleteStatus(long statusId, [FromServices] IStatusService statusService, [FromServices] IWorkspaceAccessService accessService)
        {
            var id = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && id != null)
            {
                var role = await accessService.GetRoleAsync(workspaceId, id.Value);

                if (role == null || !AccessCheck.CanManageStatuses(role.Value))
                {
                    return Forbid();
                }

                var result = await statusService.DeleteStatusAsync(statusId);

                if (result)
                {
                    if (Request.Headers.TryGetValue("X-Board-Id", out var boardId) 
                        && !string.IsNullOrWhiteSpace(boardId))
                    {
                        await hubContext.Clients.Group($"{boardId}")
                            .SendAsync("StatusDeleted", statusId);
                    }
                    
                    return NoContent();
                }

                return BadRequest("Cannot delete status with dependent tasks");
            }

            return Unauthorized("User was not found");
        }
    }
}
