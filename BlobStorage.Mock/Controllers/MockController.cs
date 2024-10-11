using Microsoft.AspNetCore.Mvc;
using Blobstorage.Mock.Helpers;
using Blobstorage.Mock.Models;
using Blobstorage.Mock.Services;

namespace Blobstorage.Mock.Controllers;

[ApiController]
[Route("[controller]")]
public class MockController : ControllerBase
{
    private readonly ILogger<MockController> _logger;
    private readonly IMockService _storageService;
    private readonly IPathBuilderService _pathBuilderService;

    public MockController(ILogger<MockController> logger, IPathBuilderService pathBuilderService, IMockService storageService)
    {
        _storageService = storageService;
        _pathBuilderService = pathBuilderService;
        _logger = logger;
    }

    [HttpPost("/POST/{*path:regex(^(.*)$)}")]
    [Consumes("application/json", "application/xml", "text/xml", "text/xml; charset=utf-8")]
    public async ValueTask<IActionResult> Post(string path)
    {
        _logger.LogInformation($"Post mock invocked with path: {path}");

        SearchFilter searchFilter = await _pathBuilderService.CreateSearchFilterAsync("POST/" + path, Request);

        MockResponse response = await _storageService.GetMockResponse(searchFilter);

        await WaitHelper.Wait(response.MinResponseTime, response.MaxResponseTime);

        return new ContentResult
        {
            Content = response.Payload.ToString(),
            ContentType = response.ContentType,
            StatusCode = response.StatusCode
        };
    }

    [HttpGet("/GET/{*path:regex(^(.*)$)}")]
    public async ValueTask<IActionResult> Get(string path)
    {
        _logger.LogInformation($"Get mock invocked with path: {path}");

        SearchFilter searchFilter = await _pathBuilderService.CreateSearchFilterAsync("GET/" + path, Request);

        MockResponse response = await _storageService.GetMockResponse(searchFilter);

        await WaitHelper.Wait(response.MinResponseTime, response.MaxResponseTime);

        return new ContentResult
        {
            Content = response.Payload.ToString(),
            ContentType = response.ContentType,
            StatusCode = response.StatusCode
        };
    }
}
