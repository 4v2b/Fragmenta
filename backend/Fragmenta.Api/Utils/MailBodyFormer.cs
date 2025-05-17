using System.Net;

namespace Fragmenta.Api.Utils
{
    public static class MailBodyFormer
    {
        public static string CreateResetPasswordTextBody(string baseUrl, string token, long userId, string culture)
        {
            string encodedToken = WebUtility.UrlEncode(token);
            string resetLink = $"{baseUrl}/reset-password?token={encodedToken}&userId={userId}";

            var (intro, instruction, linkText, outro) = culture switch
            {
                "uk" => (
                    "Вас запитали про скидання пароля.",
                    "Будь ласка, перейдіть за посиланням нижче, щоб змінити свій пароль:",
                    "Скинути пароль",
                    "Якщо ви не запитували це, просто проігноруйте цей лист."
                ),
                _ => (
                    "You requested a password reset.",
                    "Please click the link below to change your password:",
                    "Reset Password",
                    "If you didn’t request this, just ignore this email."
                )
            };

            return $@"
{intro}

{instruction}
{linkText}: {resetLink}

{outro}
";
        }
        
        }
}