using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Blobstorage.Mock.Models;

public class FileDto
{
#nullable disable
    [XmlElement("Filename")]
    [JsonPropertyName("fileName")]
    [Required]
    public string FileName { get; set; }

    [XmlElement("SearchTerm")]
    [JsonPropertyName("searchTerm")]
    public string SearchTerm { get; set; }

    [XmlElement("ContentType")]
    [JsonPropertyName("contentType")]
    public string ContentType { get; set; }

    [XmlElement("Body")]
    [JsonPropertyName("body")]
    public dynamic Body { get; set; }

    [XmlElement("StatusCode")]
    [JsonPropertyName("statusCode")]
    public int? StatusCode { get; set; }
#nullable restore
}
