using System.IO.Compression;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using TidyHPC.LiteHttpServer.WebsocketClientSessions;
using TidyHPC.LiteJson;
using TidyHPC.Loggers;
using TidyHPC.Routers.Urls;
using TidyHPC.Routers.Urls.Interfaces;

namespace TidyHPC.LiteHttpServer;

/// <summary>
/// WebSocket客户端
/// </summary>
public class WebsocketClient : IServer,IDisposable
{
    private ClientWebSocket Client { get; } = new();

    /// <summary>
    /// 连接的服务器
    /// </summary>
    public Uri Url { get; set; } = null!;

    /// <summary>
    /// 获取下一个会话
    /// </summary>
    /// <returns></returns>
    public async Task<Session> GetNextSession()
    {
        while (true)
        {
            WebsocketExtensions.WebsocketMessage message = new();
            try
            {
                message = await Client.ReceiveMessage();
                if(message.CloseStatus == WebSocketCloseStatus.NormalClosure)
                {
                    throw new Exception("Websocket NormalClosure");
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
            if (Json.TryParse(message.Message, out var msg))
            {
                Uri? url = null;
                if (msg.ContainsKey("url"))
                {
                    url = new Uri(Url, msg.Read("url", string.Empty));
                }
                if (url == null)
                {
                    msg.Dispose();
                    continue;
                }
                MemoryStream requestBody = new(Util.UTF8.GetBytes(message.Message));
                WebsocketClientSendStream responseBody = new(Client);
                responseBody.OnClose = () =>
                {
                    requestBody.Dispose();
                };
                msg.Dispose();
                Session session = new(new WebsocketClientRequest(url, requestBody), new WebsocketClientResponse(Client, responseBody));
                return session;
            }
        }
    }

    /// <summary>
    /// 连接
    /// </summary>
    /// <param name="wsUrl"></param>
    /// <param name="headers"></param>
    /// <returns></returns>
    public async Task Connect(string wsUrl,IDictionary<string,string> headers)
    {
        Url = new(wsUrl);
        foreach (var header in headers)
        {
            Client.Options.SetRequestHeader(header.Key, header.Value);
        }
        await Client.ConnectAsync(Url, CancellationToken.None);
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task SendMessage(string message)
        => await Client.SendAsync(new ArraySegment<byte>(Util.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, CancellationToken.None);

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Client.Dispose();
    }

    /// <summary>
    /// 当前状态
    /// </summary>
    public WebSocketState State => Client.State;
}
