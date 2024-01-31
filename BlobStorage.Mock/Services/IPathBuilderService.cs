using BlobStorage.Mock.Models;

namespace BlobStorage.Mock.Services
{
    public interface IPathBuilderService
    {
        public SearchFilter CreateSearchFilter(string path, string request = "");
    }
}
