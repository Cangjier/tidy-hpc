using System.Collections;
using System.Collections.Specialized;
using TidyHPC.Routers.Urls.Interfaces;

namespace TidyHPC.LiteHttpServer.WebsocketServerSessions;
internal class WebsocketServerRequestHeaders(NameValueCollection Target) : IRequestHeaders
{
    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        foreach (string? key in Target.AllKeys)
        {
            if (key != null)
            {
                yield return new KeyValuePair<string, string>(key, Target[key] ?? string.Empty);
            }
        }
    }

    /// <inheritdoc/>
    public string GetHeader(string key)
    {
        return Target[key] ?? string.Empty;
    }

    /// <inheritdoc/>
    public void SetHeader(string key, string value)
    {
        Target[key] = value;
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}