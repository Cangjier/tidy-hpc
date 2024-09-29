using System.Net.WebSockets;
using TidyHPC.Loggers;

namespace TidyHPC.LiteHttpServer.WebsocketClientSessions;
internal class WebsocketClientSendStream(ClientWebSocket client) : MemoryStream
{
    public ClientWebSocket Client = client;

    public Action? OnClose { get; set; }

    private bool _IsClosed = false;

    public override void Close()
    {
        if (_IsClosed)
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
                    await Client.SendAsync(bytes, WebSocketMessageType.Binary, true, CancellationToken.None);
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
