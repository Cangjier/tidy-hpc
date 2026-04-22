using System.Collections;
using TidyHPC.Routers.Urls.Interfaces;

namespace TidyHPC.Routers.Urls.MemorySession;

/// <summary>
/// A response headers that is stored in memory.
/// </summary>
public class MemoryResponseHeaders : IResponseHeaders
{
    /// <summary>
    /// Gets the enumerator for the headers.
    /// </summary>
    /// <returns>The enumerator for the headers.</returns>
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        yield break;
    }

    /// <summary>
    /// Gets the header value for the specified key.
    /// </summary>
    /// <param name="key">The key of the header.</param>
    /// <returns>The value of the header.</returns>
    public string GetHeader(string key)
    {
        return string.Empty;
    }

    /// <summary>
    /// Sets the header value for the specified key.
    /// </summary>
    /// <param name="key">The key of the header.</param>
    /// <param name="value">The value of the header.</param>
    public void SetHeader(string key, string value)
    {

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