using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using System.Xml.Serialization;
using Azure;
using DevDecoder.DynamicXml;
using Blobstorage.Mock.Exceptions;
using Blobstorage.Mock.Helpers;
using Blobstorage.Mock.Models;

namespace Blobstorage.Mock.Services;

public class MockService : IMockService
{
    private readonly IBlobStorageService _blobStorageService;
    private readonly ILogger<MockService> _logger;

    public MockService(IBlobStorageService blobStorageService, ILogger<MockService> logger)
    {
        _logger = logger;
        _blobStorageService = blobStorageService;
    }

    public async ValueTask<MockResponse> GetMockResponse(SearchFilter filter)
    {
        var isXmlType = filter.ContentType == "application/xml" || filter.ContentType.Contains("text/xml");
        BinaryData metadataBinary = await _blobStorageService.RetrieveBlobAsync(filter.Path + (isXmlType ? "metadata.xml" : "metadata.json"));
        MetadataDto metadataDto;
        FileDto? fileDto;

        if (isXmlType)
        {
            XmlSerializer serializer = new(typeof(MetadataDto));

            using TextReader reader = new StringReader(metadataBinary.ToString());
            metadataDto = (MetadataDto?)serializer.Deserialize(reader);
        }
        else
        {
            metadataDto = JsonSerializer
                .Deserialize<MetadataDto>(metadataBinary.ToString());
        }

        switch (filter.Method)
        {
            case "GET":
                if (string.IsNullOrEmpty(filter.SearchTerm))
                {
                    throw new FilterException("Search term of file missing in request path");
                }

                fileDto = metadataDto.Files.ToList().Find(metadata => metadata.SearchTerm == filter.SearchTerm)!;
                break;
            case "POST":
                fileDto = FindFile(filter, metadataDto.Files.ToList());
                break;
            default:
                throw new FilterException("No valid HTTP method found in the path");
        }

        try
        {
            if (string.IsNullOrEmpty(filter.ContentType))
            {
                if (fileDto is null || string.IsNullOrEmpty(fileDto.ContentType))
                {
                    _logger.LogWarning("Content-type in metadata not set, will fallback to use file extension");

                    if (fileDto is null || string.IsNullOrEmpty(fileDto.FileName))
                    {
                        _logger.LogWarning("File name with extension not found, will fallback to find a default file");

                        filter.ContentType = await _blobStorageService.BlobExistsAsync(filter.Path + "default.xml")
                            ? "text/xml; charset=utf-8"
                            : await _blobStorageService.BlobExistsAsync(filter.Path + "default.json")
                                ? "application/json"
                                : throw new FilterException("Request did not match any file in specified folder and no default file exists");
                    }
                    else
                    {
                        if (fileDto.FileName.Contains("json"))
                        {
                            filter.ContentType = "application/json";
                        }
                        else if (fileDto.FileName.Contains("xml"))
                        {
                            filter.ContentType = "text/xml; charset=utf-8";
                        }
                    }
                }
                else
                {
                    filter.ContentType = fileDto.ContentType;
                }
            }

            var fileName = filter.ContentType switch
            {
                "application/xml" or "text/xml" or "text/xml; charset=utf-8" => filter.Path + (fileDto?.FileName ?? "default.xml"),
                "application/json" => filter.Path + (fileDto?.FileName ?? "default.json"),
                _ => throw new FilterException("No valid Content type found in the filter"),
            };
            var exists = await _blobStorageService.BlobExistsAsync(fileName);

            if (!exists)
            {
                throw new MockException("Request did not match any file in specified folder and no default file exists");
            }

            BinaryData mockBlob = await _blobStorageService.RetrieveBlobAsync(fileName);
            MockResponse response = new()
            {
                StatusCode = StatusCodes.Status200OK,
                Payload = SetPayload(mockBlob, filter.ContentType),
                ContentType = filter.ContentType,
                MinResponseTime = metadataDto.MinResponseTime,
                MaxResponseTime = metadataDto.MaxResponseTime
            };

            if (fileDto?.StatusCode is not null && Enum.IsDefined(typeof(HttpStatusCode), fileDto.StatusCode.Value))
            {
                response.StatusCode = fileDto.StatusCode.Value;
            }

            return response;
        }
        catch (RequestFailedException ex)
        {
            throw new MockException("Error accessing or downloading the files in Azure blob storage", ex);
        }
    }

    private static FileDto? FindFile(SearchFilter filter, List<FileDto> metadataList)
    {
        FileDto? result = null;
        switch (filter.ContentType)
        {
            case "application/json":
                JsonObject jsonFilterBody = JsonSerializer.SerializeToNode(filter.Body);

                result = metadataList.Find(metadata =>
                {
                    JsonObject jsonMetadataBody = JsonSerializer.SerializeToNode(metadata.Body);

                    return JsonHelper.DeepCompare(jsonFilterBody, jsonMetadataBody);
                })!;
                break;
            case "application/xml":
            case "text/xml":
            case "text/xml; charset=utf-8":
                XElement xmlFilterBody = XElement.Parse(filter.Body.ToString());

                result = metadataList.Find(metadata =>
                {
                    XElement xmlMetadataBody = XElement.Parse(XmlHelper.ConvertXmlNodesToString(metadata.Body));

                    return XmlHelper.DeepCompare(xmlFilterBody, xmlMetadataBody);
                })!;
                break;
            default:
                throw new FilterException("No valid Content type found in the filter");
        }

        return result;
    }

    public dynamic SetPayload(BinaryData blobContent, string contentType)
    {
        switch (contentType)
        {
            case "application/json":
                return blobContent.ToDynamicFromJson();
            case "application/xml":
            case "text/xml":
            case "text/xml; charset=utf-8":
                var binaryString = Encoding.UTF8.GetString(blobContent);
                return XDocument.Parse(binaryString).ToDynamic();
            default:
                throw new MockException("Incorrect content type to convert the response payload");

        }
    }
}
