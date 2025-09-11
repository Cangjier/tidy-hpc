using System.Net;
using System.Net.WebSockets;
using TidyHPC.LiteHttpServer.Sessions;
using TidyHPC.LiteHttpServer.WebsocketServerSessions;
using TidyHPC.LiteJson;
using TidyHPC.Loggers;
using TidyHPC.Queues;
using TidyHPC.Routers.Urls;
using TidyHPC.Routers.Urls.Interfaces;

namespace TidyHPC.LiteHttpServer;

/// <summary>
/// Http服务器
/// </summary>
public class HttpServer : IServer
{
    /// <summary>
    /// Http服务器
    /// </summary>
    public HttpServer()
    {
        
    }

    private HttpListener Listener { get; } = new();

    /// <summary>
    /// 监听前缀
    /// </summary>
    public HttpListenerPrefixCollection Prefixes=> Listener.Prefixes;

    private SemaphoreSlim StartSemaphore { get; } = new(0);

    /// <summary>
    /// 启动监听
    /// </summary>
    public void Start()
    {
        Listener.Start();
        _ = Task.Run(Loop);
        StartSemaphore.Release();
    }

    private bool EnableLoop = true;

    private async Task Loop()
    {
        while (EnableLoop)
        {
            try
            {
                HttpListenerContext context = await Listener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    _ = Task.Run(async () =>
                    {
                        WebSocketContext websocketContext;
                        try
                        {
                            websocketContext = await context.AcceptWebSocketAsync(subProtocol: null);
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e);
                            context.Response.StatusCode = 500;
                            context.Response.Close();
                            return;
                        }
                        WebSocket webSocket = websocketContext.WebSocket;
                        while (true)
                        {
                            try
                            {
                                var message = await webSocket.ReceiveMessage();
                                if(message.CloseStatus == WebSocketCloseStatus.NormalClosure)
                                {
                                    break;
                                }
                                MemoryStream requestBody = new(Util.UTF8.GetBytes(message.Message));
                                WebsocketServerSendStream responseBody = new(webSocket);
                                responseBody.OnClose = () =>
                                {
                                    requestBody.Dispose();
                                };
                                Uri? url = null;
                                if (Json.TryParse(message.Message, out var msg))
                                {
                                    if (msg.ContainsKey("url"))
                                    {
                                        url = new Uri(context.Request.Url!, msg.Read("url", string.Empty));
                                    }
                                    else
                                    {
                                        throw new Exception($"url not found, {msg}");
                                    }
                                    msg.Dispose();
                                }
                                Session session = new(new WebsocketServerRequest(websocketContext, url, requestBody), new WebsocketServerResponse(websocketContext, responseBody));
                                SessionQueue.Enqueue(session);
                            }
                            catch (Exception e)
                            {
                                Logger.Error(e);
                                break;
                            }
                        }
                        webSocket.Dispose();
                    });
                }
                else
                {
                    SessionQueue.Enqueue(new Session(new HttpRequest(context.Request), new HttpResponse(context.Response)));
                }
            }
            catch(Exception e)
            {
                Logger.Error(e);
                break;
            }
        }
    }

    /// <summary>
    /// 停止监听
    /// </summary>
    public void Stop()
    {
        Listener.Stop();
        EnableLoop = false;
    }

    private WaitQueue<Session> SessionQueue { get; } = new();

    /// <summary>
    /// Get next session
    /// </summary>
    /// <returns></returns>
    public async Task<Session> GetNextSession()
    {
        return await SessionQueue.Dequeue();
    }
}
