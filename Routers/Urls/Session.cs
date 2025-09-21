using TidyHPC.Routers.Urls.Interfaces;

namespace TidyHPC.Routers.Urls;

/// <summary>
/// 会话
/// <para>Request和Response会随着Session的生命周期而结束</para>
/// <para>Session的销毁由路由器控制</para>
/// <para>如果有三方资源需要被销毁，一般是由Request和Response的Dispose进行控制</para>
/// <para></para>
/// </summary>
public class Session : IDisposable
{
    /// <summary>
    /// 会话
    /// </summary>
    /// <param name="request"></param>
    /// <param name="response"></param>
    public Session(IRequest request, IResponse response)
    {
        Request = request;
        Response = response;
        Cache = new SessionCache(this);
        Setter = new SessionSetter(this);
    }

    /// <summary>
    /// 请求
    /// </summary>
    public IRequest Request { get; private set; }

    /// <summary>
    /// 响应
    /// </summary>
    public IResponse Response { get; private set; }

    /// <summary>
    /// 是否为WebSocket请求
    /// </summary>
    public bool IsWebSocket => Response is IWebsocketResponse;

    /// <summary>
    /// WebSocket响应，如果不是WebSocket请求则为null
    /// </summary>
    public IWebsocketResponse? WebSocketResponse => Response as IWebsocketResponse;

    /// <summary>
    /// 缓存数据
    /// </summary>
    public SessionCache Cache { get; private set; }

    /// <summary>
    /// 辅助设置器
    /// </summary>
    public SessionSetter Setter { get; private set; }

    /// <summary>
    /// 完成会话
    /// </summary>
    /// <param name="onComplete"></param>
    /// <returns></returns>
    public async Task Complete(Func<Task> onComplete)
    {
        if (Cache.Completed) return;
        Cache.Completed = true;
        await onComplete();
    }

    /// <summary>
    /// 完成会话
    /// </summary>
    /// <param name="onComplete"></param>
    /// <returns></returns>
    public void Complete(Action onComplete)
    {
        if (Cache.Completed) return;
        Cache.Completed = true;
        onComplete();
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Request?.Dispose();
        Response?.Dispose();
        Request = null!;
        Response = null!;
        Cache?.Dispose();
        Cache = null!;
    }
}
