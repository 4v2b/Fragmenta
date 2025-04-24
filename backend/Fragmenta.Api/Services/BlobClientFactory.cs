using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Fragmenta.Api.Contracts;

namespace Fragmenta.Api.Services;

public class BlobClientFactory : IBlobClientFactory
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public BlobClientFactory(BlobServiceClient blobServiceClient, string containerName)
    {
        _blobServiceClient = blobServiceClient;
        _containerName = containerName;
    }

    public async Task<BlobClient> GetBlobClientAsync(string fileName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);
        return containerClient.GetBlobClient(fileName);
    }
}