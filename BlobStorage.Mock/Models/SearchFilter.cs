using System.ComponentModel.DataAnnotations;

namespace Blobstorage.Mock.Models;

public class SearchFilter
{
#nullable disable
    public string Method { get; set; }

    public string SearchTerm { get; set; }
    public string ContentType { get; set; }

    public dynamic Body { get; set; }

    [Required]
    public string Path { get; set; }
#nullable restore
}
