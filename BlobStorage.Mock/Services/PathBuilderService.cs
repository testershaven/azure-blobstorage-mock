using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using DevDecoder.DynamicXml;
using Blobstorage.Mock.Exceptions;
using Blobstorage.Mock.Models;

namespace Blobstorage.Mock.Services;

public class PathBuilderService : IPathBuilderService
{
    private readonly IBlobStorageService _blobStorageService;
    private readonly ILogger<PathBuilderService> _logger;

    public PathBuilderService(IBlobStorageService blobStorageService, ILogger<PathBuilderService> logger)
    {
        _blobStorageService = blobStorageService;
        _logger = logger;
    }

    public async ValueTask<SearchFilter> CreateSearchFilterAsync(string path, HttpRequest request)
    {
        var finalPath = "";
        var splittedPath = path.Split('/');
        var method = splittedPath[0];
        string body;
        using (StreamReader reader = new(request.Body, Encoding.UTF8))
        {
            body = reader.ReadToEndAsync().Result;

            if (string.IsNullOrEmpty(body))
            {
                _logger.LogInformation($"{method} mock invocked with empty body");
            }
            else
            {
                _logger.LogInformation($"{method} mock invocked with body: {body}");
            }
        }

        SearchFilter searchFilter = new();
        StringBuilder searchTermBuilder = new();
        do
        {
            var tempSplit = splittedPath[0];
            var tempPath = finalPath + tempSplit;
            if (await _blobStorageService.PathExistsAsync(tempPath))
            {
                finalPath = tempPath + "/";
            }
            else
            {
                if (!string.IsNullOrEmpty(body))
                {
                    break;
                }

                searchTermBuilder.Append($"/{tempSplit}");

            }
            splittedPath = splittedPath.Skip(1).ToArray();

        } while (splittedPath.Length > 0);

        string contentType;
        if (string.IsNullOrEmpty(request.Headers.Accept) || request.Headers.Accept == "*/*")
        {
            _logger.LogWarning("Accept header not set or set to */* falling back to Content-Type");

            if (string.IsNullOrEmpty(request.ContentType))
            {
                _logger.LogWarning("Content-type header not set, will fall to metadata to assign it");
                contentType = "";
            }
            else
            {
                contentType = request.ContentType;
            }

        }
        else
        {
            contentType = request.Headers.Accept!;
        }

        searchFilter.Method = method;
        searchFilter.Body = SetBody(body, contentType);
        searchFilter.SearchTerm = searchTermBuilder.ToString();
        searchFilter.Path = finalPath;
        searchFilter.ContentType = contentType;

        return searchFilter;
    }

    private static dynamic? SetBody(string body, string contentType)
    {
        if (!string.IsNullOrEmpty(body))
        {
            return contentType switch
            {
                "application/json" => JsonSerializer.Deserialize<dynamic>(body)!,
                "application/xml" or "text/xml" or "text/xml; charset=utf-8" => XDocument.Parse(body).ToDynamic(),
                _ => throw new FilterException(),
            };
        }
        else
        {
            return null;
        }
    }
}
