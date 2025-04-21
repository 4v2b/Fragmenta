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
    [Route("api/")]
    public class AttachmentController : ControllerBase
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

        [HttpGet("attachment-types")]
        public async Task<IActionResult> GetAttachmentTypes([FromServices] IAttachmentService attachmentService )
        {
            var id = GetAuthenticatedUserId();

            if (id != null)
            {
                var result = await attachmentService.GetAllTypesAsync();

                return Ok(result);
            }

            return Unauthorized("User was not found");
        }

        //[HttpGet("{taskId}")]
        //public IActionResult GetTask([FromQuery] long taskId, [FromServices] ITaskService taskService, [FromServices] IWorkspaceAccessService accessService)
        //{
        //    var id = GetAuthenticatedUserId();

        //    if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && id != null)
        //    {
        //        var role = accessService.GetRole(workspaceId, id.Value);

        //        if (role == null || !AccessCheck.CanManageBoardContent(role.Value))
        //        {
        //            return Forbid();
        //        }

        //        var result = taskService.GetTask(taskId);

        //        return Ok(result);
        //    }

        //    return Unauthorized("User was not found");
        //}

        //[HttpPost]
        //public IActionResult CreateTask([FromQuery] long statusId, [FromBody] CreateOrUpdateTaskRequest request, [FromServices] ITaskService taskService, [FromServices] IWorkspaceAccessService accessService)
        //{
        //    var id = GetAuthenticatedUserId();

        //    if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && id != null)
        //    {
        //        var role = accessService.GetRole(workspaceId, id.Value);

        //        if (role == null || !AccessCheck.CanManageBoardContent(role.Value))
        //        {
        //            return Forbid();
        //        }

        //        var result = taskService.CreateTask(statusId, request);

        //        if (result != null)
        //        {
        //            return CreatedAtAction(nameof(CreateTask), result);
        //        }

        //        return BadRequest();
        //    }

        //    return Unauthorized("User was not found");
        //}

        //[HttpPost("/reorder")]
        //public IActionResult Reorder([FromBody] List<ShallowUpdateTaskRequest> request, [FromServices] ITaskService taskService, [FromServices] IWorkspaceAccessService accessService)
        //{
        //    var id = GetAuthenticatedUserId();

        //    if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && id != null)
        //    {
        //        var role = accessService.GetRole(workspaceId, id.Value);

        //        if (role == null || !AccessCheck.CanManageBoardContent(role.Value))
        //        {
        //            return Forbid();
        //        }

        //        taskService.ShallowUpdate(request);

        //        return Ok();
        //    }

        //    return Unauthorized("User was not found");
        //}

        //[HttpPut("{taskId}")]
        //public IActionResult UpdateTask(long taskId, [FromBody] UpdateTaskRequest request, [FromServices] ITaskService taskService, [FromServices] IWorkspaceAccessService accessService)
        //{
        //    var id = GetAuthenticatedUserId();

        //    if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && id != null)
        //    {
        //        var role = accessService.GetRole(workspaceId, id.Value);

        //        if (role == null || !AccessCheck.CanManageStatuses(role.Value))
        //        {
        //            return Forbid();
        //        }

        //        var result = taskService.UpdateTask(taskId, request);

        //        if (result)
        //        {
        //            return Ok();
        //        }

        //        return BadRequest();
        //    }

        //    return Unauthorized("User was not found");
        //}

        //[HttpDelete("{taskId}")]
        //public IActionResult DeleteTask(long taskId, [FromServices] ITaskService taskService, [FromServices] IWorkspaceAccessService accessService)
        //{
        //    var id = GetAuthenticatedUserId();

        //    if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && id != null)
        //    {
        //        var role = accessService.GetRole(workspaceId, id.Value);

        //        if (role == null || !AccessCheck.CanManageStatuses(role.Value))
        //        {
        //            return Forbid();
        //        }

        //        var result = taskService.DeleteTask(taskId);

        //        if (result)
        //        {
        //            return NoContent();
        //        }

        //        return BadRequest("Cannot delete status with dependent tasks");
        //    }

        //    return Unauthorized("User was not found");
        //}
    }
}
