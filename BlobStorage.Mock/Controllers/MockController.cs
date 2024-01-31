using Microsoft.AspNetCore.Mvc;
using BlobStorage.Mock.Models;
using BlobStorage.Mock.Services;
using System.Security.Claims;

namespace BlobStorage.Mock.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MockController : ControllerBase
    {
        private readonly ILogger<MockController> _logger;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IPathBuilderService _pathBuilderService;

        public MockController(ILogger<MockController> logger, IPathBuilderService pathBuilderService, IBlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
            _pathBuilderService = pathBuilderService;
            _logger = logger;
        }

        [HttpPost("/POST/{*path:regex(^(.*)$)}")]
        public async Task<IActionResult> Post(string path, [FromBody] object request)
        {
            _logger.LogInformation($"Post mock invocked with path: {path}");
            _logger.LogDebug($"Mock invocked with request: {request}");

            SearchFilter searchFilter = _pathBuilderService.CreateSearchFilter("POST/" + path, request.ToString());

            object mockFile = _blobStorageService.GetMockFile(searchFilter);
            return new JsonResult(mockFile);
        }

        [HttpGet("/GET/{*path:regex(^(.*)$)}")]
        public IActionResult Get(string path)
        {
            _logger.LogInformation($"Get mock invocked with path: {path}");

            SearchFilter searchFilter = _pathBuilderService.CreateSearchFilter("GET/" + path);

            object mockFile = _blobStorageService.GetMockFile(searchFilter);

            return new JsonResult(mockFile);
        }
    }
}
