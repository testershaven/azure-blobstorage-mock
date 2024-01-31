using Azure.Storage.Blobs.Models;
using BlobStorage.Mock.Models;

namespace BlobStorage.Mock.Services
{
    public interface IBlobStorageService
    {
        public dynamic GetMockFile(SearchFilter filter);
        public BlobDownloadResult DownloadBlob(string fileName);
        public bool FolderExists(string path);
    }
}
