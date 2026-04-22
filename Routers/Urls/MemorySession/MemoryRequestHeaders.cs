using System.Collections;
using TidyHPC.Routers.Urls.Interfaces;

namespace TidyHPC.Routers.Urls.MemorySession;

/// <summary>
/// A request headers that is stored in memory.
/// </summary>
public class MemoryRequestHeaders : IRequestHeaders
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryRequestHeaders"/> class.
    /// </summary>
    public MemoryRequestHeaders(IDictionary<string, string> headers)
    {
        _headers = headers;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryRequestHeaders"/> class.
    /// </summary>
    public MemoryRequestHeaders()
    {
        _headers = new Dictionary<string, string>();
    }

    private readonly IDictionary<string, string> _headers;

    /// <summary>
    /// Gets the header value for the specified key.
    /// </summary>
    /// <param name="key">The key of the header.</param>
    /// <returns>The value of the header.</returns>
    /// <returns></returns>
    public string GetHeader(string key)
    {
        if (_headers.TryGetValue(key, out var value))
        {
            return value;
        }
        return string.Empty;
    }

    /// <summary>
    /// Sets the header value for the specified key.
    /// </summary>
    /// <param name="key">The key of the header.</param>
    /// <param name="value">The value of the header.</param>
    public void SetHeader(string key, string value)
    {
        _headers[key] = value;
    }

    /// <summary>
    /// Gets the enumerator for the headers.
    /// </summary>
    /// <returns>The enumerator for the headers.</returns>
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        return _headers.GetEnumerator();
    }

    /// <summary>
    /// Gets the enumerator for the headers.
    /// </summary>
    /// <returns>The enumerator for the headers.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}