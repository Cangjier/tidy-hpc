using System.Net.WebSockets;
using TidyHPC.Routers.Urls.Interfaces;

namespace TidyHPC.LiteHttpServer.WebsocketServerSessions;
internal class WebsocketServerResponse : IResponse, IWebsocketResponse
{
    public WebsocketServerResponse(WebSocketContext webSocketContext,
        Stream body)
    {
        WebSocketContext = webSocketContext;
        Body = body;
    }

    public WebSocketContext WebSocketContext;

    public IResponseHeaders Headers { get; } = new WebsocketServerResponseHeaders();

    public async Task SendMessage(string message)
    {
        await WebSocketContext.WebSocket.SendAsync(new ArraySegment<byte>(Util.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    public bool IsAlive() => 
        WebSocketContext.WebSocket.State != WebSocketState.Aborted &&
        WebSocketContext.WebSocket.State != WebSocketState.Closed &&
        WebSocketContext.WebSocket.State != WebSocketState.CloseSent &&
        WebSocketContext.WebSocket.State != WebSocketState.CloseReceived;

    public Stream Body { get; }

    public int StatusCode { get; set; }

    public void Dispose()
    {
        Body.Dispose();
    }

    public bool Equals(IWebsocketResponse? other)
    {
        return other is WebsocketServerResponse response && WebSocketContext == response.WebSocketContext;
    }

    public void Close()
    {
        WebSocketContext.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close", CancellationToken.None).Wait();
    }
}