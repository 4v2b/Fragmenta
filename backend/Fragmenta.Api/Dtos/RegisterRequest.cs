using System.ComponentModel.DataAnnotations;

namespace Fragmenta.Api.Dtos
{
    public class RegisterRequest
    {
        [Required]
        [MaxLength(100)]
        [EmailAddress(ErrorMessage = "auth.errors.emailInvalid")]
        public string Email { get; set; }

        [Required]
        [Password(ErrorMessage = "auth.errors.passwordTooWeak")]
        [MinLength(8)]
        [MaxLength(64)]
        public required string Password { get; set; }

        [Required]
        [MinLength(1)]
        [MaxLength(100)]
        public required string Name { get; set; }
    }
}
