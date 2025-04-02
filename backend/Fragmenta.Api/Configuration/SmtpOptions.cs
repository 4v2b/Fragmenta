namespace Fragmenta.Api.Configuration
{
    public class SmtpOptions
    {
        public string FromEmail { get; set; }
        public string ApiKey { get; set; }
        public string FromName { get; set; }
        public string? DomainName { get; set; }
    }
}
