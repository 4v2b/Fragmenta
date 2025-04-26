using Azure.Storage.Blobs;
using Fragmenta.Api.Contracts;

namespace Fragmenta.Tests.Fakes;

public class FakeBlobClientFactory : IBlobClientFactory
{
    private readonly BlobClient _blobClient;

    public FakeBlobClientFactory(BlobClient blobClient)
    {
        _blobClient = blobClient;
    }

    public Task<BlobClient> GetBlobClientAsync(string fileName)
    {
        return Task.FromResult(_blobClient);
    }
}