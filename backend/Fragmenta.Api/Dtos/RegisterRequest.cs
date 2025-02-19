using System.ComponentModel.DataAnnotations;

namespace Fragmenta.Api.Dtos
{
    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Password]
        [MinLength(8)]
        [MaxLength(64)]
        public required string Password { get; set; }

        [Required]
        public required string Name { get; set; }
    }
}
