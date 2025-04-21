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

        // TODO Need two saves???
        
        public async Task<WorkspaceDto?> CreateAsync(string name, long ownerId)
        {
            var workspace = new Workspace() { Name = name };

            var user = await _context.Users.FindAsync(ownerId);

            var role = await _context.Roles.FindAsync((long)Role.Owner);

            if (user == null || role == null)
            {
                return null;
            }

            _context.Workspaces.Add(workspace);
            
            await _context.SaveChangesAsync();

            _context.WorkspaceAccesses.Add(new WorkspaceAccess() { JoinedAt = DateTime.UtcNow, Role = role, Workspace = workspace, User = user });
            await _context.SaveChangesAsync();

            return new WorkspaceDto() { Id = workspace.Id, Name = workspace.Name };
        }

        public async Task<bool> DeleteAsync(long workspaceId)
        {
            var workspace = await _context.Workspaces.FindAsync(workspaceId);

            if(workspace == null)
            {
                return false;
            }

            _context.Workspaces.Remove(workspace);
            await _context.SaveChangesAsync();
            
            return true;
        }

        public async Task<List<WorkspaceRoleDto>> GetAllAsync(long userId)
        {
            return await _context.WorkspaceAccesses
                .Where(e => e.UserId == userId)
                .Include(e => e.Workspace)
                .Select(e => new WorkspaceRoleDto() { Id = e.WorkspaceId, Name = e.Workspace.Name, Role = Enum.GetName((Role)e.RoleId)! })
                .ToListAsync();
        }

        public async Task<WorkspaceDto?> UpdateAsync(string name, long workspaceId)
        {
            var workspace = await _context.Workspaces.FindAsync(workspaceId);

            if(workspace == null)
            {
                return null;
            }

            workspace.Name = name;

            await _context.SaveChangesAsync();

            return new WorkspaceDto() { Id = workspace.Id, Name = workspace.Name };
        }
    }
}
