namespace Blobstorage.Mock.Models;

public class InMemoryBlob
{
    public string Path { get; set; }

    public BinaryData BlobContent { get; set; }
}
