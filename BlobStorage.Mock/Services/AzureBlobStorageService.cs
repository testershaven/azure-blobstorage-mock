using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BlobStorage.Mock.Controllers;
using BlobStorage.Mock.Models;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BlobStorage.Mock.Services
{
    public class AzureBlobStorageService : IBlobStorageService
    {
        private readonly ILogger<MockController> _logger;
        private readonly AzureBlobStorageOptions _azureBlobStorageSettings;
        private BlobContainerClient _blobContainerClient;

        public AzureBlobStorageService(ILogger<MockController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _azureBlobStorageSettings = configuration.GetSection("AzureBlobStorage").Get<AzureBlobStorageOptions>();
            var blobClient = new BlobServiceClient(_azureBlobStorageSettings.ConnectionString);
            _blobContainerClient = blobClient.GetBlobContainerClient(_azureBlobStorageSettings.ContainerName);
        }

        public dynamic GetMockFile(SearchFilter filter)
        {
            var metadataBlob = DownloadBlob(filter.Path + "metadata.json");
            var metadataList = JsonSerializer.Deserialize<MetadataDto[]>(metadataBlob.Content.ToString()).ToList();
            MetadataDto metadata;

            if (filter.Path.Contains("POST"))
            {
                JsonNode jsonFilterBody = JsonSerializer.SerializeToNode(filter.Body);
                
                metadata = metadataList.Find(metadata => {
                    JsonNode jsonMetadataBody = JsonSerializer.SerializeToNode(metadata.Body);
                    
                    return JsonNode.DeepEquals(jsonFilterBody, jsonMetadataBody);
                });

            } else if(filter.Path.Contains("GET"))
            {
                metadata = metadataList.Find(metadata => metadata.SearchTerm == filter.SearchTerm);
            } else
            {
                throw new Exception("No valid HTTP method found in the path");
            }

            try
            {
                var mockBlob = DownloadBlob(filter.Path + metadata.FileName);
                return mockBlob.Content.ToDynamicFromJson();
            }
            catch (RequestFailedException ex)
            {
                throw new Exception("Error accessing or downloading the files in Azure blob storage", ex);
            }
            
        }

        public BlobDownloadResult DownloadBlob(string fileName)
        {
            var blob = _blobContainerClient.GetBlobClient(fileName);
            return blob.DownloadContent();
        }


        public bool FolderExists(string path)
        {
            try
            {
                var blobs = _blobContainerClient.GetBlobs(prefix: path);

                return blobs.Count() > 0;
            }
            catch (RequestFailedException ex)
            {
                throw new Exception("Error getting count of blobs in specified path", ex);
            }
        }
    }
}
