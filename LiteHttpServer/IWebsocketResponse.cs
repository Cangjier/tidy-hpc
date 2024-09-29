namespace TidyHPC.LiteHttpServer;

/// <summary>
/// Websocket响应
/// </summary>
public interface IWebsocketResponse : IEquatable<IWebsocketResponse>
{
    /// <summary>
    /// 发送消息
    /// </summary>
    Task SendMessage(string message);

    /// <summary>
    /// 是否存活
    /// </summary>
    bool IsAlive();

    /// <summary>
    /// 关闭
    /// </summary>
    void Close();
}
