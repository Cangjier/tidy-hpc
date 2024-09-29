using System.Collections;
using TidyHPC.Routers.Urls.Interfaces;

namespace TidyHPC.LiteHttpServer.WebsocketServerSessions;

/// <summary>
/// Websocket服务器响应头
/// </summary>
public struct WebsocketServerResponseHeaders : IResponseHeaders
{
    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        foreach (var item in Array.Empty<KeyValuePair<string, string>>())
        {
            yield return item;
        }
    }

    /// <inheritdoc/>
    public string GetHeader(string key)
    {
        return string.Empty;
    }

    /// <inheritdoc/>
    public void SetHeader(string key, string value)
    {

    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
