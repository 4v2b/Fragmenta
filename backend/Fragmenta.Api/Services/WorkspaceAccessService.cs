using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
using Fragmenta.Dal.Models;
using Fragmenta.Dal;
using Role = Fragmenta.Api.Enums.Role;
using Microsoft.EntityFrameworkCore;

namespace Fragmenta.Api.Services
{
    public class WorkspaceAccessService : IWorkspaceAccessService
    {
        private readonly ILogger<WorkspaceAccessService> _logger;
        private readonly ApplicationContext _context;

        public WorkspaceAccessService(ILogger<WorkspaceAccessService> logger, ApplicationContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<MemberDto?> AddMemberAsync(long workspaceId, long userId)
        {
            var access = await _context.WorkspaceAccesses.FindAsync(workspaceId, userId);

            if (access != null)
            {
                return null;
            }

            var workspace = await _context.Workspaces.FindAsync(workspaceId);

            var user = await _context.Users.FindAsync(userId);

            var role = await _context.Roles.FindAsync((long)Role.Member);

            if (user == null || role == null || workspace == null)
            {
                return null;
            }

            await _context.WorkspaceAccesses.AddAsync(new WorkspaceAccess() { JoinedAt = DateTime.UtcNow, Role = role, Workspace = workspace, User = user });
            await _context.SaveChangesAsync();

            return new MemberDto() { Id = user.Id, Email = user.Email, Role = Enum.GetName((Role)role.Id)!, Name = user.Name };
        }

        public async Task<List<MemberDto>> AddMembersAsync(long workspaceId, long[] userIds)
        {
            var workspace = await _context.Workspaces.FindAsync(workspaceId);

            var role = await _context.Roles.FindAsync((long)Role.Member);

            if (role == null || workspace == null)
            {
                return [];
            }

            List<WorkspaceAccess> accesses = new List<WorkspaceAccess>();

            foreach (long userId in userIds)
            {
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                    continue;

                accesses.Add(new WorkspaceAccess() { JoinedAt = DateTime.UtcNow, Role = role, Workspace = workspace, User = user });
            }

            await _context.WorkspaceAccesses.AddRangeAsync(accesses);
            await _context.SaveChangesAsync();

            return accesses.Select(e => new MemberDto() { Id = e.UserId, Email = e.User.Email, Role = Enum.GetName((Role)role.Id)!, Name = e.User.Name }).ToList();
        }

        public async Task<bool> DeleteMemberAsync(long workspaceId, long userId)
        {
            var access = await _context.WorkspaceAccesses.FindAsync(workspaceId, userId);

            if (access == null)
            {
                return false;
            }

            if (access.RoleId == (long)Role.Guest)
            {
                var boardAccesses = _context.BoardAccesses
                    .Include(e => e.Board)
                    .Where(e => e.UserId == userId && e.Board.WorkspaceId == workspaceId);

                _context.RemoveRange(boardAccesses);
                await _context.SaveChangesAsync();
            }
            else
            {
                await _context.Tasks.Where(t => t.AssigneeId == userId).ForEachAsync(t => t.Assignee = null);
            }

            _context.Remove(access);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<MemberDto>> GetMembersAsync(long workspaceId)
        {
            return await _context.WorkspaceAccesses
                .Where(e => e.WorkspaceId == workspaceId)
                .Include(e => e.User)
                .Select(e => new MemberDto()
                {
                    Email = e.User.Email,
                    Id = e.UserId,
                    Name = e.User.Name,
                    Role = Enum.GetName((Role)e.RoleId)!
                }).ToListAsync();
        }

        public async Task<Role?> GetRoleAsync(long workspaceId, long userId)
        {
            var access = await _context.WorkspaceAccesses.FindAsync(workspaceId, userId);

            return access == null ? null : (Role)access.RoleId;
        }

        public async Task<bool> GrantAdminPermissionAsync(long workspaceId, long memberId)
        {
            var access = await _context.WorkspaceAccesses.FindAsync(workspaceId, memberId);

            if (access == null)
            {
                return false;
            }

            var role = await _context.Roles.FindAsync((long)Role.Admin);

            if (role == null)
            {
                throw new InvalidOperationException("Cannot find admin role in the table");
            }

            access.Role = role;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RevokeAdminPermissionAsync(long workspaceId, long adminId)
        {
            var access = await _context.WorkspaceAccesses.FindAsync(workspaceId, adminId);

            if (access == null)
            {
                return false;
            }

            var role = await _context.Roles.FindAsync((long)Role.Member);

            if (role == null)
            {
                throw new InvalidOperationException("Cannot find member role in the table");
            }

            access.Role = role;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
