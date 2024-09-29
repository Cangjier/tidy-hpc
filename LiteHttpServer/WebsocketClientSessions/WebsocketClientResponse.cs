using System.Net.WebSockets;
using TidyHPC.Routers.Urls.Interfaces;

namespace TidyHPC.LiteHttpServer.WebsocketClientSessions;
internal class WebsocketClientResponse : IResponse, IWebsocketResponse
{
    public WebsocketClientResponse(ClientWebSocket client, Stream body)
    {
        Client = client;
        Body = body;
    }

    public ClientWebSocket Client { get; }

    public IResponseHeaders Headers { get; } = new WebsocketClientResponseHeaders();

    public async Task SendMessage(string message)
        => await Client.SendAsync(new ArraySegment<byte>(Util.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, CancellationToken.None);

    public bool IsAlive() => Client.State != WebSocketState.Closed &&
            Client.State != WebSocketState.Aborted &&
            Client.State != WebSocketState.CloseSent &&
            Client.State != WebSocketState.CloseReceived;

    public Stream Body { get; }

    public int StatusCode { get; set; }

    public void Dispose()
    {
        Body.Dispose();
    }

    public bool Equals(IWebsocketResponse? other)
    {
        return other is WebsocketClientResponse response && Client == response.Client;
    }

    public void Close()
    {
        Client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close", CancellationToken.None).Wait();
    }
}
