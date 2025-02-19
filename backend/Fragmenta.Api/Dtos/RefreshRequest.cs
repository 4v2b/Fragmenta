using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Fragmenta.Api.Dtos
{
    public class RefreshRequest
    {
        public required string RefreshToken { get; set; }
    }
}
