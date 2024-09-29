using System.Collections;
using System.Collections.Specialized;
using TidyHPC.Routers.Urls.Interfaces;

namespace TidyHPC.LiteHttpServer.Sessions;

/// <inheritdoc/>
public struct HttpRequestHeaders(NameValueCollection target) : IRequestHeaders
{
    /// <summary>
    /// 封装对象
    /// </summary>
    public NameValueCollection Target = target;

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
