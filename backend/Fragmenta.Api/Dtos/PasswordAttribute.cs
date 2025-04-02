using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Fragmenta.Api.Dtos
{
    public class PasswordAttribute : ValidationAttribute
    {
        public string ErrorMessage { get; set; } = "Password does not meet complexity requirements.";

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not string password)
            {
                return new ValidationResult("Invalid password format.");
            }

            int count = 0;
            if (Regex.IsMatch(password, "[a-z]")) count++;
            if (Regex.IsMatch(password, "[A-Z]")) count++;
            if (Regex.IsMatch(password, "[0-9]")) count++;
            if (Regex.IsMatch(password, "[@#$%^&*!?]")) count++;

            return count >= 3 ? ValidationResult.Success : new ValidationResult(ErrorMessage);
        }
    }
}