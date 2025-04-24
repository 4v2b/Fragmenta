using Azure.Storage.Blobs;

namespace Fragmenta.Api.Contracts;

public interface IBlobClientFactory
{
    Task<BlobClient> GetBlobClientAsync(string fileName);
}