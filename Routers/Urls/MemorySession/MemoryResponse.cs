using TidyHPC.Routers.Urls.Interfaces;

namespace TidyHPC.Routers.Urls.MemorySession;

/// <summary>
/// A response that is stored in memory.
/// </summary>
public class MemoryResponse : IResponse
{
    /// <summary>
    /// The headers of the response.
    /// </summary>
    public IResponseHeaders Headers { get; private set; } = new MemoryResponseHeaders();

    /// <summary>
    /// The body of the response.
    /// </summary>
    public Stream Body { get; private set; } = new MemoryStream();

    /// <summary>
    /// The status code of the response.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// The content length of the response.
    /// </summary>
    public long? ContentLength { get; set; }

    /// <summary>
    /// Disposes the response.
    /// </summary>
    public void Dispose()
    {
        Headers = null!;
        Body?.Dispose();
        Body = null!;
        StatusCode = 0;
        ContentLength = null;
    }
}