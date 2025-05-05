using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
using Fragmenta.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fragmenta.Api.Controllers
{
    #pragma warning disable CS1591
    [Authorize]
    [ApiController]
    [Route("api/workspaces")]
    public class WorkspaceController : ControllerBase
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

        /// <summary>
        /// Returns workspaces available to the user
        /// </summary>
        /// <response code="200">Returns workspace</response>
        /// <response code="401">If user is unauthorized</response>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromServices] IWorkspaceService workspaceService)
        {
            var id = GetAuthenticatedUserId();

            if (id != null)
            {
                var workspaces = await workspaceService.GetAllAsync(id.Value);

                return Ok(workspaces);
            }
            return Unauthorized("User was not found");
        }

        /// <summary>
        /// Creates a new workspace with set owner
        /// </summary>
        /// <response code="201">Returns created workspace</response>
        /// <response code="401">If user is unauthorized</response>
        /// <response code="400">If request contains invalid data</response>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrUpdateWorkspaceRequest request, [FromServices] IWorkspaceService workspaceService)
        {
            var id = GetAuthenticatedUserId();

            if (id != null)
            {
                var result = await workspaceService.CreateAsync(request.Name, id.Value);

                if (result != null)
                {
                    return CreatedAtAction(nameof(Create), result);
                }

                return BadRequest();
            }
            return Unauthorized("User was not found");
        }

        /// <summary>
        /// Creates a new workspace with set owner
        /// </summary>
        /// <param name="id">Workspace id</param>
        /// <response code="200">Returns updated workspace</response>
        /// <response code="401">If user is unauthorized</response>
        /// <response code="400">If request contains invalid data</response>
        /// <response code="403">If user has no permission for the action</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] CreateOrUpdateWorkspaceRequest request, [FromServices] IWorkspaceService workspaceService, [FromServices] IWorkspaceAccessService accessService)
        {
            var userId = GetAuthenticatedUserId();

            if (userId != null)
            {
                var role = await accessService.GetRoleAsync(id, userId.Value);

                if (role == null || !AccessCheck.CanUpdateWorkspace(role.Value))
                {
                    return Forbid();
                }

                var result = await workspaceService.UpdateAsync(request.Name, id);

                if (result != null)
                {
                    return Ok(result);
                }

                return BadRequest("result is null");
            }
            return Unauthorized("User was not found");
        }

        /// <summary>
        /// Deletes a workspace
        /// </summary>
        /// <param name="id">Workspace id</param>
        /// <response code="204">Deletion is successful</response>
        /// <response code="401">If user is unauthorized</response>
        /// <response code="403">If user has no permission for the action</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id, [FromServices] IWorkspaceService workspaceService, [FromServices] IWorkspaceAccessService accessService)
        {
            var userId = GetAuthenticatedUserId();

            if (userId != null)
            {
                var role = await accessService.GetRoleAsync(id, userId.Value);

                if(role == null)
                {
                    return Forbid();
                }

                if (AccessCheck.CanDeleteWorkspace(role.Value) && await workspaceService.DeleteAsync(id))
                {
                    return NoContent();
                }

                return Forbid();
            }
            return Unauthorized("User was not found");
        }
    }
}