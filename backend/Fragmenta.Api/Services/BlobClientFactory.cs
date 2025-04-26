using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Fragmenta.Api.Configuration;
using Fragmenta.Api.Contracts;
using Microsoft.Extensions.Options;

namespace Fragmenta.Api.Services;

public class BlobClientFactory : IBlobClientFactory
{
    private readonly BlobServiceClient _blobServiceClient;
    private AzureStorageOptions _options;

    public BlobClientFactory(BlobServiceClient blobServiceClient, IOptions<AzureStorageOptions> options)
    {
        _blobServiceClient = blobServiceClient;
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<BlobClient> GetBlobClientAsync(string fileName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);
        return containerClient.GetBlobClient(fileName);
    }
}