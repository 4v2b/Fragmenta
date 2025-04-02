using Fragmenta.Api.Enums;

namespace Fragmenta.Api.Dtos
{
    public class EmailSendResult
    {

        public bool IsSuccess { get; }
        public bool IsLocked { get; }
        public DateTime? LockedUntil { get; }
        public EmailSendErrorType? ErrorType { get; init; }

        private EmailSendResult(bool isSuccess, bool isLocked, DateTime? lockedUntil, EmailSendErrorType? errorType)
        {
            IsSuccess = isSuccess;
            IsLocked = isLocked;
            LockedUntil = lockedUntil;
            ErrorType = errorType;
        }

        public static EmailSendResult SuccessResult() => new(true, false, null, null);
        public static EmailSendResult RateLimited(DateTime lockedUntil) => new(false, true, lockedUntil, EmailSendErrorType.RateLimited);
        public static EmailSendResult Failure(EmailSendErrorType errorType) => new(false, false, null, errorType);
    }
}
