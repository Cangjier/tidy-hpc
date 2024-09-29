using System.Net.WebSockets;
using TidyHPC.Routers.Urls;
using TidyHPC.Routers.Urls.Interfaces;

namespace TidyHPC.LiteHttpServer.WebsocketServerSessions;
/// <summary>
/// 请求
/// </summary>
/// <param name="webSocketContext"></param>
/// <param name="body"></param>
/// <param name="url"></param>
public class WebsocketServerRequest(WebSocketContext webSocketContext, Uri? url, Stream body) : IRequest
{
    /// <summary>
    /// 地址
    /// </summary>
    public Uri? Url { get; private set; } = url;

    /// <summary>
    /// 查询参数
    /// </summary>
    public IDictionary<string, string> Query { get; private set; } = new EmptyDictionary();

    /// <summary>
    /// Headers
    /// </summary>
    public IRequestHeaders Headers { get; private set; } = new WebsocketServerRequestHeaders(webSocketContext.Headers);

    /// <summary>
    /// Method
    /// </summary>
    public UrlMethods Method { get; private set; } = UrlMethods.WebSocket;

    /// <summary>
    /// 体
    /// </summary>
    public Stream Body { get; private set; } = body;

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Url = null!;
        Query = null!;
        Headers = null!;
        Body = null!;
    }
}
