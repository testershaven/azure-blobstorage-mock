namespace Blobstorage.Mock.Models;

public class MockResponse
{
    public required int StatusCode { get; set; }
    public required dynamic Payload { get; set; }
    public required string ContentType { get; set; }
    public int MinResponseTime { get; set; }
    public int MaxResponseTime { get; set; }
}
