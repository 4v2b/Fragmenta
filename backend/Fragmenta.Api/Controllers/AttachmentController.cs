using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
using Fragmenta.Api.Middleware;
using Fragmenta.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.StaticFiles;

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
        public async Task<IActionResult> GetAttachmentTypes([FromServices] IAttachmentService attachmentService)
        {
            var id = GetAuthenticatedUserId();

            if (id != null)
            {
                var result = await attachmentService.GetAllTypesAsync();

                return Ok(result);
            }

            return Unauthorized("User was not found");
        }

        // Get attachments for a specific task
        [HttpGet("attachments")]
        public async Task<IActionResult> GetAttachments([FromQuery]long taskId,
            [FromServices] IAttachmentService attachmentService, [FromServices] IWorkspaceAccessService accessService)
        {
            var userId = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && userId != null)
            {
                var role = await accessService.GetRoleAsync(workspaceId, userId.Value);

                if (role == null || !AccessCheck.CanManageBoardContent(role.Value))
                {
                    return Forbid();
                }

                var attachments = await attachmentService.GetAttachmentPreviewsAsync(taskId);
                return Ok(attachments);
            }

            return Unauthorized("User was not found");
        }

// Upload attachment
        [HttpPost("attachments")]
        public async Task<IActionResult> UploadAttachment([FromQuery] long taskId, IFormFile file,
            [FromServices] IAttachmentService attachmentService, [FromServices] IWorkspaceAccessService accessService)
        {
            var userId = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && userId != null)
            {
                var role = await accessService.GetRoleAsync(workspaceId, userId.Value);

                if (role == null || !AccessCheck.CanManageBoardContent(role.Value))
                {
                    return Forbid();
                }

                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file was uploaded");
                }

                try
                {
                    var attachment = await attachmentService.UploadAttachmentAsync(file, taskId);

                    return CreatedAtAction(nameof(UploadAttachment), attachment);
                }
                catch (InvalidOperationException ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            return Unauthorized("User was not found");
        }

        // Download attachment
        [HttpGet("attachments/{attachmentId}")]
        public async Task<IActionResult> DownloadAttachment(long attachmentId,
            [FromServices] IAttachmentService attachmentService, [FromServices] IWorkspaceAccessService accessService)
        {
            var userId = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && userId != null)
            {
                var role = await accessService.GetRoleAsync(workspaceId, userId.Value);

                if (role == null || !AccessCheck.CanManageBoardContent(role.Value))
                {
                    return Forbid();
                }

                try
                {
                   
                    
                    var stream = await attachmentService.DownloadAttachmentAsync(attachmentId);
                    var attachmentInfo = await attachmentService.GetAttachmentPreviewAsync(attachmentId);

                    if (attachmentInfo == null)
                        return NotFound("Attachment not found");
                    
                    var provider = new FileExtensionContentTypeProvider();
                    if (!provider.TryGetContentType(attachmentInfo.OriginalName, out var contentType))
                    {
                        contentType = "application/octet-stream";
                    }
                    Response.Headers.Append("Access-Control-Expose-Headers", "Content-Disposition");
                    return File(stream, contentType, attachmentInfo.OriginalName);
                }
                catch (InvalidOperationException ex)
                {
                    return NotFound(ex.Message);
                }
            }

            return Unauthorized("User was not found");
        }

// Delete attachment
        [HttpDelete("attachments/{attachmentId}")]
        public async Task<IActionResult> DeleteAttachment(long attachmentId,
            [FromServices] IAttachmentService attachmentService, [FromServices] IWorkspaceAccessService accessService)
        {
            var userId = GetAuthenticatedUserId();

            if (long.TryParse(HttpContext.Items["WorkspaceId"]?.ToString(), out long workspaceId) && userId != null)
            {
                var role = await accessService.GetRoleAsync(workspaceId, userId.Value);

                if (role == null || !AccessCheck.CanManageBoardContent(role.Value))
                {
                    return Forbid();
                }

                var result = await attachmentService.DeleteAttachmentAsync(attachmentId);

                if (result)
                {
                    return NoContent();
                }

                return NotFound("Attachment not found");
            }

            return Unauthorized("User was not found");
        }
    }
}