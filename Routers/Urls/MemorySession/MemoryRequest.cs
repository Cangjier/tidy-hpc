using TidyHPC.Routers.Urls.Interfaces;

namespace TidyHPC.Routers.Urls.MemorySession;

/// <summary>
/// A request that is stored in memory.
/// </summary>
public class MemoryRequest : IRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryRequest"/> class.
    /// </summary>
    /// <param name="url">The URL of the request.</param>
    /// <param name="headers">The headers of the request.</param>
    /// <param name="body">The body of the request.</param>
    public MemoryRequest(string url, Dictionary<string, string> headers, string body)
    {
        Url = new Uri(url);
        Headers = new MemoryRequestHeaders(headers);
        Body = new MemoryStream(Util.UTF8.GetBytes(body));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryRequest"/> class.
    /// </summary>
    /// <param name="url">The URL of the request.</param>
    /// <param name="headers">The headers of the request.</param>
    /// <param name="body">The body of the request.</param>
    public MemoryRequest(string url, Dictionary<string, string> headers, Stream body)
    {
        Url = new Uri(url);
        Headers = new MemoryRequestHeaders(headers);
        Body = body;
        disposeBody = false;
    }

    private bool disposeBody = true;

    /// <summary>
    /// The URL of the request.
    /// </summary>
    public Uri? Url { get; private set; }

    /// <summary>
    /// The query of the request.
    /// </summary>
    public IDictionary<string, string> Query { get; private set; } = new EmptyDictionary();

    /// <summary>
    /// The headers of the request.
    /// </summary>
    public IRequestHeaders Headers { get; private set; }

    /// <summary>
    /// The method of the request.
    /// </summary>
    public UrlMethods Method { get; private set; }

    /// <summary>
    /// The body of the request.
    /// </summary>
    public Stream Body { get; private set; }

    /// <summary>
    /// Disposes the request.
    /// </summary>
    public void Dispose()
    {
        Url = null;
        Query = null!;
        Headers = null!;
        if (disposeBody)
        {
            Body?.Dispose();
        }
        Body = null!;
    }
}