using System.ComponentModel.DataAnnotations;

namespace Fragmenta.Api.Dtos
{
    public class RegisterRequest
    {
        [Required]
        [EmailAddress(ErrorMessage = "auth.errors.emailInvalid")]
        public string Email { get; set; }

        [Required]
        [Password(ErrorMessage = "auth.errors.passwordTooWeak")]
        [MinLength(8)]
        [MaxLength(64)]
        public required string Password { get; set; }

        [Required]
        public required string Name { get; set; }
    }
}
