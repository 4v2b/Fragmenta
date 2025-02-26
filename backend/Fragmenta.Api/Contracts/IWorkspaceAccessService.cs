using Fragmenta.Api.Dtos;
using Fragmenta.Api.Enums;

namespace Fragmenta.Api.Contracts
{
    public interface IWorkspaceAccessService
    {
        Role? GetRole(long workspaceId, long userId);

        bool DeleteMember(long workspaceId, long userId);

        MemberDto? AddMember(long workspaceId, long userId);

        List<MemberDto> AddMembers(long workspaceId, long[] userIds);

        List<MemberDto> GetMembers(long workspaceId);

        bool RevokeAdminPermission(long workspaceId, long adminId);

        bool GrantAdminPermission(long workspaceId, long memberId);
    }
}
