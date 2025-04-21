using Fragmenta.Api.Dtos;
using Fragmenta.Api.Enums;

namespace Fragmenta.Api.Contracts
{
    public interface IWorkspaceAccessService
    {
        Task<Role?> GetRoleAsync(long workspaceId, long userId);

        Task<bool> DeleteMemberAsync(long workspaceId, long userId);

        Task<MemberDto?> AddMemberAsync(long workspaceId, long userId);

        Task<List<MemberDto>> AddMembersAsync(long workspaceId, long[] userIds);

        Task<List<MemberDto>> GetMembersAsync(long workspaceId);

        Task<bool> RevokeAdminPermissionAsync(long workspaceId, long adminId);

        Task<bool> GrantAdminPermissionAsync(long workspaceId, long memberId);
    }
}
