using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Blobstorage.Mock.Exceptions;

namespace Blobstorage.Mock.Services;

public class AzureBlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _blobContainerClient;

    public AzureBlobStorageService(BlobContainerClient blobContainerClient)
    {
        _blobContainerClient = blobContainerClient;
    }

    public async ValueTask<BinaryData?> RetrieveBlobAsync(string fileName)
    {
        BlobClient blob = _blobContainerClient.GetBlobClient(fileName);

        Response<BlobDownloadResult> content = await blob.DownloadContentAsync();

        return content.Value.Content;
    }

    public async ValueTask<bool> PathExistsAsync(string path)
    {
        try
        {
            AsyncPageable<BlobItem> blobs = _blobContainerClient.GetBlobsAsync(prefix: path);

            return await blobs.FirstOrDefaultAsync() != null;
        }
        catch (RequestFailedException ex)
        {
            throw new MockException("Error getting count of blobs in specified path", ex);
        }
    }

    public async ValueTask<bool> BlobExistsAsync(string fileName)
    {
        try
        {
            BlobClient blob = _blobContainerClient.GetBlobClient(fileName);
            Response<bool> exists = await blob.ExistsAsync();
            return exists.Value;
        }
        catch (RequestFailedException ex)
        {
            throw new MockException($"Error checking if blob exists at ({fileName})", ex);
        }
    }
}
