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

        public MemberDto? AddMember(long workspaceId, long userId)
        {
            var access = _context.WorkspaceAccesses.Find(workspaceId, userId);

            if (access != null)
            {
                return null;
            }

            var workspace = _context.Workspaces.Find(workspaceId);

            var user = _context.Users.Find(userId);

            var role = _context.Roles.Find((long)Role.Member);

            if (user == null || role == null || workspace == null)
            {
                return null;
            }

            _context.WorkspaceAccesses.Add(new WorkspaceAccess() { JoinedAt = DateTime.UtcNow, Role = role, Workspace = workspace, User = user });
            _context.SaveChanges();

            return new MemberDto() { Id = user.Id, Email = user.Email, Role = Enum.GetName((Role)role.Id)!, Name = user.Name };
        }

        public List<MemberDto> AddMembers(long workspaceId, long[] userIds)
        {
            var workspace = _context.Workspaces.Find(workspaceId);

            var role = _context.Roles.Find((long)Role.Member);

            if (role == null || workspace == null)
            {
                return [];
            }

            List<WorkspaceAccess> accesses = new List<WorkspaceAccess>();

            foreach (long userId in userIds)
            {
                var user = _context.Users.Find(userId);

                if (user == null)
                    continue;

                accesses.Add(new WorkspaceAccess() { JoinedAt = DateTime.UtcNow, Role = role, Workspace = workspace, User = user });
            }

            _context.WorkspaceAccesses.AddRange(accesses);
            _context.SaveChanges();

            return accesses.Select(e => new MemberDto() { Id = e.UserId, Email = e.User.Email, Role = Enum.GetName((Role)role.Id)!, Name = e.User.Name }).ToList();
        }

        public bool DeleteMember(long workspaceId, long userId)
        {
            var access = _context.WorkspaceAccesses.Find(workspaceId, userId);

            if (access == null)
            {
                return false;
            }

            _context.Remove(access);
            _context.SaveChanges();

            return true;
        }

        public List<MemberDto> GetMembers(long workspaceId)
        {
            return _context.WorkspaceAccesses
                .Where(e => e.WorkspaceId == workspaceId)
                .Include(e => e.User)
                .Select(e => new MemberDto()
                {
                    Email = e.User.Email,
                    Id = e.UserId,
                    Name = e.User.Name,
                    Role = Enum.GetName((Role)e.RoleId)!
                }).ToList();
        }

        public Role? GetRole(long workspaceId, long userId)
        {
            var access = _context.WorkspaceAccesses.Find(workspaceId, userId);

            return access == null ? null : (Role)access.RoleId;
        }

        public bool GrantAdminPermission(long workspaceId, long memberId)
        {
            var access = _context.WorkspaceAccesses.Find(workspaceId, memberId);

            if (access == null)
            {
                return false;
            }

            var role = _context.Roles.Find((long)Role.Admin);

            if (role == null)
            {
                throw new InvalidOperationException("Cannot find admin role in the table");
            }

            access.Role = role;

            _context.SaveChanges();

            return true;
        }

        public bool RevokeAdminPermission(long workspaceId, long adminId)
        {
            var access = _context.WorkspaceAccesses.Find(workspaceId, adminId);

            if (access == null)
            {
                return false;
            }

            var role = _context.Roles.Find((long)Role.Member);

            if (role == null)
            {
                throw new InvalidOperationException("Cannot find member role in the table");
            }

            access.Role = role;

            _context.SaveChanges();

            return true;
        }
    }
}
