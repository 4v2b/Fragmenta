using Fragmenta.Api.Enums;

namespace Fragmenta.Api.Dtos
{
    public class AuthResult
    {
        public bool IsSuccess { get; }
        public UserDto? User { get; }
        public bool IsLocked { get; }
        public DateTime? LockedUntil { get; }
        public AuthErrorType? Error { get; set; }

        private AuthResult(bool isSuccess, UserDto? user, bool isLocked, DateTime? lockedUntil, AuthErrorType? error)
        {
            IsSuccess = isSuccess;
            User = user;
            IsLocked = isLocked;
            LockedUntil = lockedUntil;
            Error = error;
        }

        public static AuthResult Success(UserDto user) => new(true, user, false, null, null);
        public static AuthResult Failed(AuthErrorType error) => new(false, null, false, null, error);
        public static AuthResult Locked(DateTime until) => new(false, null, true, until, AuthErrorType.AccessLocked);
    }
}
