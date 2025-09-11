using System.IO.Compression;
using System.Net.WebSockets;
using TidyHPC.Loggers;
using TidyHPC.Routers.Urls.Responses;

namespace TidyHPC.LiteHttpServer;

/// <summary>
/// Websocket扩展
/// </summary>
public static class WebsocketExtensions
{
    /// <summary>
    /// Websocket消息
    /// </summary>
    public class WebsocketMessage
    {
        /// <summary>
        /// Websocket Close Status
        /// </summary>
        public WebSocketCloseStatus? CloseStatus { get; set; }

        /// <summary>
        /// Websocket Message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Implicit conversion from string to WebsocketMessage
        /// </summary>
        /// <param name="message"></param>
        public static implicit operator WebsocketMessage(string message) => new() { Message = message };
    }

    /// <summary>
    /// 接受数据，如果是字节则默认为brotli压缩
    /// </summary>
    /// <param name="webSocket"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static async Task<WebsocketMessage> ReceiveMessage(this WebSocket webSocket,CancellationToken cancellationToken)
    {
        bool isBinary = false;
        using var memoryStream = new MemoryStream();
        WebSocketReceiveResult result;
        byte[] buffer = new byte[1024]; // 可以根据需要调整缓冲区大小  
        do
        {
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
            if (result.CloseStatus != null)
            {
                if(result.CloseStatus == WebSocketCloseStatus.NormalClosure)
                {
                    return new WebsocketMessage
                    {
                        CloseStatus = result.CloseStatus
                    };
                }
                else
                {
                    throw new Exception($"Websocket close: {result.CloseStatus}");
                }
            }
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cancellationToken);
            }
            else if (result.MessageType == WebSocketMessageType.Text)
            {
                memoryStream.Write(buffer, 0, result.Count);
            }
            else if (result.MessageType == WebSocketMessageType.Binary)
            {
                isBinary = true;
                memoryStream.Write(buffer, 0, result.Count);
            }
        }
        while (!result.EndOfMessage); // 继续读取，直到EndOfMessage为true  
        if (memoryStream.Length == 0)
        {
            Logger.Error("Receive empty message");
        }
        memoryStream.Seek(0, SeekOrigin.Begin);
        if (isBinary)
        {
            if(UrlResponse.DefaultContentEncoding == "br")
            {
                using var brotliStream = new BrotliStream(memoryStream, CompressionMode.Decompress);
                using var decompressedStream = new MemoryStream();
                try
                {
                    brotliStream.CopyTo(decompressedStream);
                    return Util.UTF8.GetString(decompressedStream.ToArray());
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    var resultText = Util.UTF8.GetString(memoryStream.ToArray());
                    return resultText;
                }
            }
            else if(UrlResponse.DefaultContentEncoding == "gzip")
            {
                using var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
                using var decompressedStream = new MemoryStream();
                try
                {
                    gzipStream.CopyTo(decompressedStream);
                    return Util.UTF8.GetString(decompressedStream.ToArray());
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    var resultText = Util.UTF8.GetString(memoryStream.ToArray());
                    return resultText;
                }
            }
            else if(UrlResponse.DefaultContentEncoding == "deflate")
            {
                using var deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress);
                using var decompressedStream = new MemoryStream();
                try
                {
                    deflateStream.CopyTo(decompressedStream);
                    return Util.UTF8.GetString(decompressedStream.ToArray());
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    var resultText = Util.UTF8.GetString(memoryStream.ToArray());
                    return resultText;
                }
            }
            else
            {
                return Util.UTF8.GetString(memoryStream.ToArray());
            }
        }
        else
        {
            return Util.UTF8.GetString(memoryStream.ToArray());
        }
    }
}
