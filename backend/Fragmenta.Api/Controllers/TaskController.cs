using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
using Fragmenta.Api.Middleware;
using Fragmenta.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Fragmenta.Api.Enums;
using Microsoft.AspNetCore.SignalR;

namespace Fragmenta.Api.Controllers
{
    [Authorize]
    [ApiController]
    [ServiceFilter(typeof(WorkspaceFilter))]
    [Route("api/tasks")]
    public class TaskController(IHubContext<BoardHub> hubContext) : ControllerBase
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
        public async Task<IActionResult> GetTasks([FromQuery] long boardId, [FromServices] IBoardAccessService boardAccessService, [FromServices] ITaskService taskService, [FromServices] IWorkspaceAccessService accessService)
        {
            var id = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && id != null)
            {
                var role = await accessService.GetRoleAsync(workspaceId, id.Value);

                if (role == null || (role == Role.Guest && !await boardAccessService.CanViewBoardAsync(boardId, id.Value)))
                {
                    return Forbid();
                }

                var result = await taskService.GetTasksAsync(boardId);

                return Ok(result);
            }

            return Unauthorized("User was not found");
        }

        [HttpGet("{taskId}")]
        public async Task<IActionResult> GetTask([FromQuery] long taskId, [FromServices] ITaskService taskService, [FromServices] IWorkspaceAccessService accessService)
        {
            var id = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && id != null)
            {
                var role = await accessService.GetRoleAsync(workspaceId, id.Value);

                if (role == null)
                {
                    return Forbid();
                }

                var result = await taskService.GetTaskAsync(taskId);

                return Ok(result);
            }

            return Unauthorized("User was not found");
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromQuery] long statusId, [FromBody] CreateOrUpdateTaskRequest request, [FromServices] ITaskService taskService, [FromServices] IWorkspaceAccessService accessService)
        {
            var id = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && id != null)
            {
                var role = await accessService.GetRoleAsync(workspaceId, id.Value);

                if (role == null || !AccessCheck.CanManageBoardContent(role.Value))
                {
                    return Forbid();
                }

                var result = await taskService.CreateTaskAsync(statusId, request);

                if (result != null)
                {
                    return CreatedAtAction(nameof(CreateTask), result);
                }

                return BadRequest();
            }

            return Unauthorized("User was not found");
        }

        [HttpPost("reorder")]
        public async Task<IActionResult> Reorder([FromBody] ShallowUpdateTaskRequest request, [FromServices] ITaskService taskService, [FromServices] IWorkspaceAccessService accessService)
        {
            var id = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out var workspaceId) && id != null)
            {
                var role = await accessService.GetRoleAsync(workspaceId, id.Value);

                if (role == null || !AccessCheck.CanManageBoardContent(role.Value))
                {
                    return Forbid();
                }

                await taskService.ShallowUpdateAsync(request);
                
                await hubContext.Clients
                    .Group(request.BoardId.ToString())
                    .SendAsync("TaskMoved", request);

                return NoContent();
            }

            return Unauthorized("User was not found");
        }

        [HttpPut("{taskId:long}")]
        public async Task<IActionResult> UpdateTask(long taskId, [FromBody] UpdateTaskRequest request, [FromServices] ITaskService taskService, [FromServices] IWorkspaceAccessService accessService)
        {
            var id = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out var workspaceId) && id != null)
            {
                var role = await accessService.GetRoleAsync(workspaceId, id.Value);

                if (role == null || !AccessCheck.CanManageBoardContent(role.Value))
                {
                    return Forbid();
                }

                var result = await taskService.UpdateTaskAsync(taskId, request);

                if (result)
                {
                    return NoContent();
                }

                return BadRequest();
            }

            return Unauthorized("User was not found");
        }

        [HttpDelete("{taskId:long}")]
        public async Task<IActionResult> DeleteTask(long taskId, [FromServices] ITaskService taskService, [FromServices] IWorkspaceAccessService accessService)
        {
            var id = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out var workspaceId) && id != null)
            {
                var role = await accessService.GetRoleAsync(workspaceId, id.Value);

                if (role == null || !AccessCheck.CanManageStatuses(role.Value))
                {
                    return Forbid();
                }

                var result = await taskService.DeleteTaskAsync(taskId);

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