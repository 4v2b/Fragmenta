namespace Fragmenta.Api.Configuration
{
    public class AzureStorageOptions
    {
        public string ConnectionString { get; set; } = null!;

        public string ContainerName { get; set; } = null!;

        public string BaseUrl { get; set; } = null!;
    }
}
