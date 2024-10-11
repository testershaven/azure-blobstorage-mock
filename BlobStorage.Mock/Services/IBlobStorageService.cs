namespace Blobstorage.Mock.Services;

public interface IBlobStorageService
{
    public ValueTask<BinaryData?> RetrieveBlobAsync(string fileName);
    public ValueTask<bool> PathExistsAsync(string path);
    public ValueTask<bool> BlobExistsAsync(string fileName);
}
