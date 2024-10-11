using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Blobstorage.Mock.Models;

[Serializable()]
[XmlRoot("Metadata")]
public class MetadataDto
{
    [XmlElement("MinResponseTime")]
    [JsonPropertyName("minResponseTime")]
    public int MinResponseTime { get; set; }

    [XmlElement("MaxResponseTime")]
    [JsonPropertyName("maxResponseTime")]
    public int MaxResponseTime { get; set; }

    [XmlElement("File")]
    [JsonPropertyName("files")]
    [Required]
    public FileDto[] Files { get; set; }
}
