using System.Collections;
using TidyHPC.Routers.Urls.Interfaces;

namespace TidyHPC.LiteHttpServer.WebsocketClientSessions;
internal struct WebsocketClientResponseHeaders : IResponseHeaders
{
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        foreach (var item in Array.Empty<KeyValuePair<string, string>>())
        {
            yield return item;
        }
    }

    public string GetHeader(string key)
    {
        return string.Empty;
    }

    public void SetHeader(string key, string value)
    {

    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
