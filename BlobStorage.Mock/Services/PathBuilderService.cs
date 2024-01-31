using BlobStorage.Mock.Controllers;
using BlobStorage.Mock.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlobStorage.Mock.Services
{
    public class PathBuilderService : IPathBuilderService
    {
        private readonly ILogger<MockController> _logger;
        private readonly IBlobStorageService _blobStorageService;

        public PathBuilderService(ILogger<MockController> logger, IBlobStorageService blobStorageService)
        {
            _logger = logger;
            _blobStorageService = blobStorageService;
        }

        public SearchFilter CreateSearchFilter(string path, string body = "")
        {
            var searchFilter = new SearchFilter();
            var searchTermBuilder = new StringBuilder();
            var finalPath = "";
            var splittedPath = path.Split('/');

            do
            {
                var tempSplit = splittedPath[0];
                var tempPath = finalPath + tempSplit ;
                if (_blobStorageService.FolderExists(tempPath))
                {
                    finalPath = tempPath + "/";
                }
                else
                {
                    if (!string.IsNullOrEmpty(body)) break;
                    searchTermBuilder.Append($"/{tempSplit}");

                }
                splittedPath = splittedPath.Skip(1).ToArray();

            } while (splittedPath.Length > 0);

            searchFilter.Body = (!string.IsNullOrEmpty(body)) ? JsonSerializer.Deserialize<dynamic>(body) : null;
            searchFilter.SearchTerm = searchTermBuilder.ToString();
            searchFilter.Path = finalPath;

            return searchFilter;
        }
    }
}
