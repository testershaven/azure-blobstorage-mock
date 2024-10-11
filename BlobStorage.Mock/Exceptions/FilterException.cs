namespace Blobstorage.Mock.Exceptions;

public class FilterException : MockException
{
    public FilterException()
    {
    }

    public FilterException(string message) : base(message)
    {
    }

    public FilterException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
