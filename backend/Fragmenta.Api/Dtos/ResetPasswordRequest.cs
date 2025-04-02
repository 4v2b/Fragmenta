using System.ComponentModel.DataAnnotations;

namespace Fragmenta.Api.Dtos
{
    public class ResetPasswordRequest
    {
        public required string Token { get; set; }

        public required long UserId { get; set; }

        [MinLength(8)]
        [MaxLength(64)]
        [Password(ErrorMessage = "auth.errors.passwordTooWeak")]
        public required string NewPassword { get; set; }
    }
}
