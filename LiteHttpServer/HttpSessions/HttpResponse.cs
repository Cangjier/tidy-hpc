using System.Net;
using TidyHPC.Routers.Urls.Interfaces;

namespace TidyHPC.LiteHttpServer.Sessions;

/// <summary>
/// Http 响应
/// </summary>
/// <param name="Target"></param>
public readonly struct HttpResponse(HttpListenerResponse Target) : IResponse
{
    /// <inheritdoc/>
    public IResponseHeaders Headers => new HttpResponseHeaders(Target.Headers);

    /// <inheritdoc/>
    public Stream Body => Target.OutputStream;

    /// <summary>
    /// 状态码
    /// </summary>
    public int StatusCode
    {
        get => Target.StatusCode;
        set => Target.StatusCode = value;
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Target.Close();
    }
}
