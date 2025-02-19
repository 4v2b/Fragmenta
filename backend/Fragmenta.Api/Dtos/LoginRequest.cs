using System.ComponentModel.DataAnnotations;

namespace Fragmenta.Api.Dtos
{
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = nameof(Resources.ValidationMessages.Required))]
        public required string Password { get; set; }
    }
}