using System.Net.WebSockets;
using TidyHPC.Loggers;

namespace TidyHPC.LiteHttpServer.WebsocketServerSessions;

/// <summary>
/// Websocket服务器发送流
/// </summary>
/// <param name="webSocket"></param>
public class WebsocketServerSendStream(WebSocket webSocket) : MemoryStream
{
    /// <summary>
    /// 封装对象
    /// </summary>
    public WebSocket WebSocket = webSocket;

    /// <summary>
    /// 当关闭时触发
    /// </summary>
    public Action? OnClose { get; set; }

    private bool _IsClosed = false;

    /// <summary>
    /// 关闭
    /// </summary>
    public override void Close()
    {
        if(_IsClosed)
        {
            return;
        }
        _IsClosed = true;
        if (Length == 0) 
            return;
        try
        {
            OnClose?.Invoke(); //主要目的是为了释放请求体
            Seek(0, SeekOrigin.Begin);
            var bytes = ToArray();
            _ = Task.Run(async () =>
            {
                try
                {
                    await WebSocket.SendAsync(bytes, WebSocketMessageType.Binary, true, CancellationToken.None);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            });
        }
        catch (Exception e)
        {
            Logger.Error(e);
        }
        base.Close();
    }
}
