namespace Blobstorage.Mock.Models;

public class AzureBlobStorageOptions
{
    public const string ConfigPath = "AzureBlobStorage";

    public required string ConnectionString { get; set; }
    public required string ContainerName { get; set; }
    public required bool PerfTestModeEnabled { get; set; }
}
