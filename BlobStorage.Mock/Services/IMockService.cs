using Blobstorage.Mock.Models;

namespace Blobstorage.Mock.Services;
public interface IMockService
{
    ValueTask<MockResponse> GetMockResponse(SearchFilter filter);
    dynamic SetPayload(BinaryData blobContent, string contentType);
}
