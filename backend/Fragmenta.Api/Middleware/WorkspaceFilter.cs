using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Fragmenta.Api.Middleware
{
    public class WorkspaceFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue("X-Workspace-Id", out var workspaceId))
            {
                context.Result = new BadRequestObjectResult("Missing X-Workspace-Id header.");
                return;
            }

            context.HttpContext.Items["WorkspaceId"] = workspaceId.ToString();
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
