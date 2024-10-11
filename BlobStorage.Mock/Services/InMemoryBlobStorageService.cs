using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Blobstorage.Mock.Models;

namespace Blobstorage.Mock.Services;

public class InMemoryBlobStorageService : IBlobStorageService
{
    private HashSet<InMemoryBlob>? _inMemoryBlobs;
    private readonly BlobContainerClient _blobContainerClient;

    public InMemoryBlobStorageService(BlobContainerClient blobContainerClient)
    {
        _blobContainerClient = blobContainerClient;
    }

    public async ValueTask<BinaryData?> RetrieveBlobAsync(string fileName)
    {
        await CheckOrDownloadBlobsAsync();

        return _inMemoryBlobs?.FirstOrDefault(blob => blob.Path == fileName)?.BlobContent ?? null;
    }

    public async ValueTask<bool> PathExistsAsync(string path)
    {
        await CheckOrDownloadBlobsAsync();

        return _inMemoryBlobs?.Any(blob => blob.Path.Contains(path)) ?? false;
    }

    public async ValueTask<bool> BlobExistsAsync(string fileName)
    {
        await CheckOrDownloadBlobsAsync();

        return _inMemoryBlobs?.Any(blob => blob.Path.Equals(fileName)) ?? false;
    }

    private async ValueTask CheckOrDownloadBlobsAsync()
    {
        if (_inMemoryBlobs == null)
        {
            _inMemoryBlobs = [];
            await foreach (BlobItem blobItem in _blobContainerClient.GetBlobsAsync())
            {
                var fileName = blobItem.Name;
                BlobClient blobClient = _blobContainerClient.GetBlobClient(blobItem.Name);
                Azure.Response<BlobDownloadResult> response = await blobClient.DownloadContentAsync();

                InMemoryBlob inMemoryBlob = new()
                {
                    BlobContent = response.Value.Content,
                    Path = fileName,
                };

                _inMemoryBlobs.Add(inMemoryBlob);
            }
        }
    }
}
