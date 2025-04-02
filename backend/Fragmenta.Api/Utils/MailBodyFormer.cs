using System.Net;

namespace Fragmenta.Api.Utils
{
    public static class MailBodyFormer
    {
        public static string CreateResetPasswordTextBody(string baseUrl, string token, long userId)
        {
            string encodedToken = WebUtility.UrlEncode(token);
            return $"{baseUrl}/reset-password?token={encodedToken}&userId={userId}";
        }

        public static string CreateResetPasswordHtmlBody(string baseUrl, string token, long userId)
        {
            string encodedToken = WebUtility.UrlEncode(token);
            return $"{baseUrl}/reset-password?token={encodedToken}&userId={userId}";
        }
    }
}
