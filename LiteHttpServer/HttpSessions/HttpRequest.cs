using System.Collections.Specialized;
using System.Net;
using TidyHPC.Routers.Urls;
using TidyHPC.Routers.Urls.Interfaces;
using TidyHPC.Routers.Urls.Utils;

namespace TidyHPC.LiteHttpServer.Sessions;

/// <summary>
/// Http 请求
/// </summary>
public class HttpRequest(HttpListenerRequest target) : IRequest
{
    /// <summary>
    /// 封装对象
    /// </summary>
    public HttpListenerRequest Target = target;

    /// <inheritdoc/>
    public Uri? Url => Target.Url;

    private Dictionary<string, string>? _query = null;

    /// <inheritdoc/>
    public IDictionary<string, string> Query
    {
        get
        {
            if (_query == null)
            {
                _query = [];
                foreach (var key in Target.QueryString.AllKeys)
                {
                    if (key != null)
                    {
                        _query[key] = Target.QueryString[key] ?? string.Empty;
                    }
                }
            }
            return _query;
        }
    }

    /// <inheritdoc/>
    public IRequestHeaders Headers => new HttpRequestHeaders(Target.Headers);

    /// <inheritdoc/>
    public UrlMethods Method => Target.HttpMethod.ToLower() switch
    {
        "get" => UrlMethods.HTTP_GET,
        "post" => UrlMethods.HTTP_POST,
        "put" => UrlMethods.HTTP_PUT,
        "delete" => UrlMethods.HTTP_DELETE,
        "patch" => UrlMethods.HTTP_PATCH,
        "options" => UrlMethods.HTTP_OPTIONS,
        "head" => UrlMethods.HTTP_HEAD,
        "connect" => UrlMethods.HTTP_CONNECT,
        "trace" => UrlMethods.HTTP_TRACE,
        _ => throw new NotImplementedException()
    };

    /// <inheritdoc/>
    public Stream Body => Target.InputStream;

    /// <summary>
    /// 销毁对象
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public void Dispose()
    {
        _query?.Clear();
        _query = null;
    }
}
