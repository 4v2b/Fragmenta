using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
using Fragmenta.Api.Enums;
using Role = Fragmenta.Api.Enums.Role;
using Fragmenta.Dal;
using Fragmenta.Dal.Models;
using Microsoft.EntityFrameworkCore;

namespace Fragmenta.Api.Services
{
    #pragma warning disable CS1591
    public class WorkspaceService : IWorkspaceService
    {
        private readonly ILogger<WorkspaceService> _logger;
        private readonly ApplicationContext _context;

        public WorkspaceService(ILogger<WorkspaceService> logger, ApplicationContext context)
        {
            _logger = logger;
            _context = context;
        }

        public WorkspaceDto? Create(string name, long ownerId)
        {
            var workspace = new Workspace() { Name = name };

            var user = _context.Users.Find(ownerId);

            var role = _context.Roles.Find((long)Role.Owner);

            if (user == null || role == null)
            {
                return null;
            }

            _context.Workspaces.Add(workspace);
            _context.SaveChanges();

            _context.WorkspaceAccesses.Add(new WorkspaceAccess() { JoinedAt = DateTime.UtcNow, Role = role, Workspace = workspace, User = user });
            _context.SaveChanges();

            return new WorkspaceDto() { Id = workspace.Id, Name = workspace.Name };

        }

        public bool Delete(long workspaceId)
        {
            var workspace = _context.Workspaces.Find(workspaceId);

            if(workspace == null)
            {
                return false;
            }

            _context.Workspaces.Remove(workspace);
            _context.SaveChanges();
            
            return true;
        }

        public List<WorkspaceRoleDto> GetAll(long userId)
        {
            return _context.WorkspaceAccesses
                .Where(e => e.UserId == userId)
                .Include(e => e.Workspace)
                .Select(e => new WorkspaceRoleDto() { Id = e.WorkspaceId, Name = e.Workspace.Name, Role = Enum.GetName((Role)e.RoleId)! })
                .ToList();
        }

        public WorkspaceDto? Update(string name, long workspaceId)
        {
            var workspace = _context.Workspaces.Find(workspaceId);

            if(workspace == null)
            {
                return null;
            }

            workspace.Name = name;

            _context.SaveChanges();

            return new WorkspaceDto() { Id = workspace.Id, Name = workspace.Name };
        }
    }
}
