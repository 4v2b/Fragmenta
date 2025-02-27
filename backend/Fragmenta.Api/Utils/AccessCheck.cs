using Fragmenta.Api.Enums;

namespace Fragmenta.Api.Utils
{
    #pragma warning disable CS1591
    public static class AccessCheck
    {
        public static bool CanDeleteMember(Role actorRole, Role targetRole)
        {
            if (actorRole == Role.Owner && targetRole != Role.Owner)
                return true;

            if (actorRole == Role.Admin && (targetRole == Role.Member || targetRole == Role.Guest))
                return true;

            return false;
        }

        public static bool CanCreateBoard(Role actorRole) =>
            actorRole == Role.Owner || actorRole == Role.Admin;

        public static bool CanAddMember(Role actorRole) =>
            actorRole == Role.Owner || actorRole == Role.Admin;

        public static bool CanDeleteWorkspace(Role actorRole) => actorRole == Role.Owner;

        public static bool CanUpdateWorkspace(Role actorRole) => actorRole == Role.Owner;

        public static bool CanRevokeAdminPermission(Role actorRole, Role memberRole) => actorRole == Role.Owner && memberRole == Role.Admin ;

        public static bool CanGrantAdminPermission(Role actorRole, Role memberRole) => actorRole == Role.Owner && memberRole == Role.Member;

    }
}
