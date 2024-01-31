using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BlobStorage.Mock.Models
{
    public class SearchFilter
    {
        public string SearchTerm { get; set; }

        public dynamic Body { get; set; }

        [Required]
        public string Path { get; set; }
    }
}
