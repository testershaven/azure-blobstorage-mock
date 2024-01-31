using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BlobStorage.Mock.Models
{
    public class MetadataDto
    {
        [JsonPropertyName("fileName")]
        [Required]
        public string FileName { get; set; }

        [JsonPropertyName("searchTerm")]
        public string SearchTerm { get; set; }

        [JsonPropertyName("body")]
        public dynamic Body { get; set; }
    }
}
