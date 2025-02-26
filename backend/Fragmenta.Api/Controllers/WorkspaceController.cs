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
        /// Returns workspaces avialable to the user
        /// </summary>
        /// <response code="200">Returns workspace</response>
        /// <response code="401">If user is unauthorized</response>
        [HttpGet]
        public IActionResult GetAll([FromServices] IWorkspaceService workspaceService)
        {
            var id = GetAuthenticatedUserId();

            if (id != null)
            {
                List<WorkspaceRoleDto> workspaces = workspaceService.GetAll(id.Value);

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
        public IActionResult Create([FromBody] CreateOrUpdateWorkspaceRequest request, [FromServices] IWorkspaceService workspaceService)
        {
            var id = GetAuthenticatedUserId();

            if (id != null)
            {
                var result = workspaceService.Create(request.Name, id.Value);

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
        public IActionResult Update(long id, [FromBody] CreateOrUpdateWorkspaceRequest request, [FromServices] IWorkspaceService workspaceService, [FromServices] IWorkspaceAccessService accessService)
        {
            var userId = GetAuthenticatedUserId();

            if (userId != null)
            {
                var role = accessService.GetRole(id, userId.Value);

                if (role == null || !AccessCheck.CanUpdateWorkspace(role.Value))
                {
                    return Forbid();
                }

                var result = workspaceService.Update(request.Name, userId.Value);

                if (result != null)
                {
                    return Ok(result);
                }

                return BadRequest();
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
        public IActionResult Delete(long id, [FromServices] IWorkspaceService workspaceService, [FromServices] IWorkspaceAccessService accessService)
        {
            var userId = GetAuthenticatedUserId();

            if (userId != null)
            {
                var role = accessService.GetRole(id, userId.Value);

                if(role == null)
                {
                    return Forbid();
                }

                if (AccessCheck.CanDeleteWorkspace(role.Value) && workspaceService.Delete(id))
                {
                    return NoContent();
                }

                return Forbid();
            }
            return Unauthorized("User was not found");
        }
    }
}