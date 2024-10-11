using Blobstorage.Mock.Models;

namespace Blobstorage.Mock.Services;

public interface IPathBuilderService
{
    public ValueTask<SearchFilter> CreateSearchFilterAsync(string path, HttpRequest request);
}
