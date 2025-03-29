using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Fragmenta.Api.Dtos
{
    public class PasswordAttribute : ValidationAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        public string ErrorMessage { get; set; }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var request = (RegisterRequest)validationContext.ObjectInstance;

            int count = 0;
            if (Regex.IsMatch(request.Password, "[a-z]")) count++;
            if (Regex.IsMatch(request.Password, "[A-Z]")) count++;
            if (Regex.IsMatch(request.Password, "[0-9]")) count++;
            if (Regex.IsMatch(request.Password, "[@#$%^&*!?]")) count++;

            if(count >= 3)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage);
        }
    }
}
