using System.ComponentModel.DataAnnotations;

namespace Fragmenta.Api.Dtos;

public class ChangePasswordRequest
{
    public required string OldPassword { get; set; }
    
    [Password(ErrorMessage = "auth.errors.passwordTooWeak")]
    [MinLength(8)]
    [MaxLength(64)]
    public required string NewPassword { get; set; }
}