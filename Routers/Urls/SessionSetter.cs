using System.IO.Compression;
using System.Runtime.CompilerServices;
using TidyHPC.LiteJson;
using TidyHPC.Loggers;
using TidyHPC.Routers.Urls.Responses;

namespace TidyHPC.Routers.Urls;
/// <summary>
/// Session 设置器
/// </summary>
public class SessionSetter(Session session)
{
    /// <summary>
    /// 会话
    /// </summary>
    public Session Session { get; } = session;

    /// <summary>
    /// 设置响应
    /// </summary>
    /// <param name="resultValue"></param>
    /// <param name="urlRouter"></param>
    /// <returns></returns>
    public async Task<bool> SetResponse(object? resultValue, UrlRouter urlRouter)
    {
        if (resultValue is FilterResult filterResult)
        {
            Session.Cache.FilterStatus = filterResult.Status;
        }
        else if (resultValue is NoneResponse)
        {

        }
        else if (resultValue is Redirect urlFilterRedirect)
        {
            //重定向
            Session.Response.StatusCode = 302;
            Session.Response.Headers.SetHeader("Location", urlFilterRedirect.Url);
        }
        else if (resultValue is ResponseStatusCode urlFilterStatusCode)
        {
            //状态码
            Session.Cache.FilterStatus = UrlFilterStatus.Rejected;
            Session.Response.StatusCode = urlFilterStatusCode.StatusCode;
        }
        else if (resultValue is Stream resultStream)
        {
            // 如果是流，将流拷贝到响应体
            await resultStream.CopyToAsync(Session.Response.Body);
            resultStream.Dispose();
        }
        else if (resultValue is Stream[] resultStreams)
        {
            foreach (var stream in resultStreams)
            {
                await stream.CopyToAsync(Session.Response.Body);
                stream.Dispose();
            }
        }
        else if (resultValue is FileStream[] resultFileStreams)
        {
            foreach (var stream in resultFileStreams)
            {
                await stream.CopyToAsync(Session.Response.Body);
                stream.Dispose();
            }
        }
        else if (resultValue is BinaryFile urlResponseFile)
        {
            if (!File.Exists(urlResponseFile.FilePath))
            {
                Session.Response.StatusCode = 404;
                Session.Response.Headers.SetHeader("Content-Type", "text/html");
                Session.Response.Body.Write(Util.UTF8.GetBytes($"""
                <html>
                <head>
                <title>404 Not Found</title>
                </head>
                <body>
                <h1>404 Not Found</h1>
                <p>The requested File was not found on this server.</p>
                <p>File: {urlResponseFile.RelativeFilePath}</p>
                </body>
                </html>
                """));
            }
            else
            {
                if (string.IsNullOrEmpty(urlResponseFile.ContentEncoding) == false)
                {
                    Session.Response.Headers.ContentEncoding = urlResponseFile.ContentEncoding;
                }
                Session.Response.Headers.ContentType = new Headers.ContentType()
                {
                    MediaType = urlResponseFile.ContentType
                };
                Session.Response.Headers.ContentDisposition = urlResponseFile.ContentDisposition;
                if (urlResponseFile.FileEncoding == urlResponseFile.ContentEncoding)
                {
                    //文件编码和内容编码一致，直接拷贝文件
                    using FileStream fileStream = File.OpenRead(urlResponseFile.FilePath);
                    await fileStream.CopyToAsync(Session.Response.Body);
                }
                else if (string.IsNullOrEmpty(urlResponseFile.FileEncoding))
                {
                    //文件编码为空，但是内容编码不为空，使用内容编码压缩
                    using FileStream fileStream = File.OpenRead(urlResponseFile.FilePath);
                    if (urlResponseFile.ContentEncoding == "br")
                    {
                        using BrotliStream brotliStream = new(Session.Response.Body, CompressionMode.Compress);
                        await fileStream.CopyToAsync(brotliStream);
                    }
                    else if (urlResponseFile.ContentEncoding == "gzip")
                    {
                        using GZipStream gzipStream = new(Session.Response.Body, CompressionMode.Compress);
                        await fileStream.CopyToAsync(gzipStream);
                    }
                    else if (urlResponseFile.ContentEncoding == "deflate")
                    {
                        using DeflateStream deflateStream = new(Session.Response.Body, CompressionMode.Compress);
                        await fileStream.CopyToAsync(deflateStream);
                    }
                    else if (string.IsNullOrEmpty(urlResponseFile.ContentEncoding))
                    {
                        await fileStream.CopyToAsync(Session.Response.Body);
                    }
                    else
                    {
                        throw new NotSupportedException("不支持的内容编码");
                    }
                }
                else
                {
                    throw new NotSupportedException("不支持的文件编码和内容编码组合");
                }
            }
        }
        else if (resultValue is TextHtml urlResponseTextHtml)
        {
            var contentEncoding = urlResponseTextHtml.ContentEncoding ?? UrlResponse.DefaultContentEncoding;
            Session.Response.Headers.ContentEncoding = contentEncoding;
            Session.Response.Headers.ContentType = new Headers.ContentType()
            {
                MediaType = "text/html"
            };

            if (contentEncoding == "br")
            {
                using BrotliStream brotliStream = new(Session.Response.Body, CompressionMode.Compress);
                await brotliStream.WriteAsync(Util.UTF8.GetBytes(urlResponseTextHtml.Content));
            }
            else if (contentEncoding == "gzip")
            {
                using GZipStream gzipStream = new(Session.Response.Body, CompressionMode.Compress);
                await gzipStream.WriteAsync(Util.UTF8.GetBytes(urlResponseTextHtml.Content));
            }
            else if (contentEncoding == "deflate")
            {
                using DeflateStream deflateStream = new(Session.Response.Body, CompressionMode.Compress);
                await deflateStream.WriteAsync(Util.UTF8.GetBytes(urlResponseTextHtml.Content));
            }
            else
            {
                await Session.Response.Body.WriteAsync(Util.UTF8.GetBytes(urlResponseTextHtml.Content));
            }
        }
        else if (resultValue is ApplicationJson urlResponseJson)
        {
            var contentEncoding = urlResponseJson.ContentEncoding ?? UrlResponse.DefaultContentEncoding;
            Session.Response.Headers.ContentEncoding = contentEncoding;
            Session.Response.Headers.ContentType = new Headers.ContentType()
            {
                MediaType = "application/json"
            };
            if (Session.IsWebSocket)
            {

                await urlRouter.Events.ResponseJsonGenerated(Session, urlResponseJson.Content);
                if (Session.WebSocketResponse != null)
                {
                    await Session.WebSocketResponse.SendMessage(urlResponseJson.Content.ToString());
                }
                urlResponseJson.Content.Dispose();
            }
            else
            {
                if (contentEncoding == "br")
                {
                    using BrotliStream brotliStream = new(Session.Response.Body, CompressionMode.Compress);
                    await urlRouter.Events.ResponseJsonGenerated(Session, urlResponseJson.Content);
                    urlResponseJson.Content.WriteTo(brotliStream);
                    urlResponseJson.Content.Dispose();
                }
                else if (contentEncoding == "gzip")
                {
                    using GZipStream gzipStream = new(Session.Response.Body, CompressionMode.Compress);
                    await urlRouter.Events.ResponseJsonGenerated(Session, urlResponseJson.Content);
                    urlResponseJson.Content.WriteTo(gzipStream);
                    urlResponseJson.Content.Dispose();
                }
                else if (contentEncoding == "deflate")
                {
                    using DeflateStream deflateStream = new(Session.Response.Body, CompressionMode.Compress);
                    await urlRouter.Events.ResponseJsonGenerated(Session, urlResponseJson.Content);
                    urlResponseJson.Content.WriteTo(deflateStream);
                    urlResponseJson.Content.Dispose();
                }
                else
                {
                    await urlRouter.Events.ResponseJsonGenerated(Session, urlResponseJson.Content);
                    urlResponseJson.Content.WriteTo(Session.Response.Body);
                    urlResponseJson.Content.Dispose();
                }
            }
        }
        else if (resultValue is MultiplyStreamFile multiplyStreamFile)
        {
            if (multiplyStreamFile.ContentEncoding != null)
            {
                Session.Response.Headers.ContentEncoding = multiplyStreamFile.ContentEncoding;
            }
            Session.Response.Headers.ContentType = new Headers.ContentType()
            {
                MediaType = multiplyStreamFile.ContentType
            };
            if (multiplyStreamFile.CacheControl != null)
            {
                Session.Response.Headers.CacheControl = multiplyStreamFile.CacheControl;
            }
            Session.Response.Headers.ContentDisposition = multiplyStreamFile.ContentDisposition;
            if (multiplyStreamFile.FileEncoding == multiplyStreamFile.ContentEncoding)
            {
                foreach (var stream in multiplyStreamFile.Streams)
                {
                    await stream.CopyToAsync(Session.Response.Body);
                    stream.Dispose();
                }
            }
            else if (multiplyStreamFile.FileEncoding == null && multiplyStreamFile.ContentEncoding != null)
            {
                if (multiplyStreamFile.ContentEncoding == "br")
                {
                    using BrotliStream brotliStream = new(Session.Response.Body, CompressionMode.Compress);
                    foreach (var stream in multiplyStreamFile.Streams)
                    {
                        await stream.CopyToAsync(brotliStream);
                        stream.Dispose();
                    }
                }
                else if (multiplyStreamFile.ContentEncoding == "gzip")
                {
                    using GZipStream gzipStream = new(Session.Response.Body, CompressionMode.Compress);
                    foreach (var stream in multiplyStreamFile.Streams)
                    {
                        await stream.CopyToAsync(gzipStream);
                        stream.Dispose();
                    }
                }
                else if (multiplyStreamFile.ContentEncoding == "deflate")
                {
                    using DeflateStream deflateStream = new(Session.Response.Body, CompressionMode.Compress);
                    foreach (var stream in multiplyStreamFile.Streams)
                    {
                        await stream.CopyToAsync(deflateStream);
                        stream.Dispose();
                    }
                }
                else
                {
                    throw new NotSupportedException("不支持的内容编码");
                }
            }
            else
            {
                throw new NotSupportedException("不支持的文件编码和内容编码组合");
            }
        }
        else if (resultValue is NetMessageInterface netMessageInterface)
        {
            Session.Response.Headers.ContentEncoding = UrlResponse.DefaultContentEncoding;
            Session.Response.Headers.ContentType = new Headers.ContentType()
            {
                MediaType = "application/json"
            };
            if (UrlResponse.DefaultContentEncoding == "br")
            {
                using BrotliStream brotliStream = new(Session.Response.Body, CompressionMode.Compress);
                await urlRouter.Events.ResponseJsonGenerated(Session, netMessageInterface.Target);
                netMessageInterface.Target.WriteTo(brotliStream);
                netMessageInterface.Target.Dispose();
            }
            else if (UrlResponse.DefaultContentEncoding == "gzip")
            {
                using GZipStream gzipStream = new(Session.Response.Body, CompressionMode.Compress);
                await urlRouter.Events.ResponseJsonGenerated(Session, netMessageInterface.Target);
                netMessageInterface.Target.WriteTo(gzipStream);
                netMessageInterface.Target.Dispose();
            }
            else if (UrlResponse.DefaultContentEncoding == "deflate")
            {
                using DeflateStream deflateStream = new(Session.Response.Body, CompressionMode.Compress);
                await urlRouter.Events.ResponseJsonGenerated(Session, netMessageInterface.Target);
                netMessageInterface.Target.WriteTo(deflateStream);
                netMessageInterface.Target.Dispose();
            }
            else
            {
                await urlRouter.Events.ResponseJsonGenerated(Session, netMessageInterface.Target);
                netMessageInterface.Target.WriteTo(Session.Response.Body);
                netMessageInterface.Target.Dispose();
            }
        }
        else if (resultValue != null &&
            resultValue?.GetType().FullName?.StartsWith("System.Runtime.CompilerServices.AsyncTaskMethodBuilder") == false &&
            resultValue is not Task)
        {
            Session.Response.Headers.ContentEncoding = UrlResponse.DefaultContentEncoding;
            Session.Response.Headers.ContentType = new Headers.ContentType()
            {
                MediaType = "application/json"
            };
            using NetMessageInterface resultJson = NetMessageInterface.New();
            resultJson.data = new(resultValue);
            await urlRouter.Events.ResponseJsonGenerated(Session, resultJson.Target);
            if (Session.IsWebSocket)
            {
                if (Session.WebSocketResponse != null)
                {
                    await Session.WebSocketResponse.SendMessage(resultJson.ToString());
                }
            }
            else
            {
                if (UrlResponse.DefaultContentEncoding == "br")
                {
                    using BrotliStream brotliStream = new(Session.Response.Body, CompressionMode.Compress);
                    resultJson.Target.WriteTo(brotliStream);
                }
                else if (UrlResponse.DefaultContentEncoding == "gzip")
                {
                    using GZipStream gzipStream = new(Session.Response.Body, CompressionMode.Compress);
                    resultJson.Target.WriteTo(gzipStream);
                }
                else if (UrlResponse.DefaultContentEncoding == "deflate")
                {
                    using DeflateStream deflateStream = new(Session.Response.Body, CompressionMode.Compress);
                    resultJson.Target.WriteTo(deflateStream);
                }
                else
                {
                    resultJson.Target.WriteTo(Session.Response.Body);
                }
            }
        }
        else
        {
            Session.Response.Headers.ContentEncoding = UrlResponse.DefaultContentEncoding;
            Session.Response.Headers.ContentType = new Headers.ContentType()
            {
                MediaType = "application/json"
            };
            using NetMessageInterface resultJson = NetMessageInterface.New();
            await urlRouter.Events.ResponseJsonGenerated(Session, resultJson.Target);
            if (Session.IsWebSocket)
            {
                if (Session.WebSocketResponse != null)
                {
                    await Session.WebSocketResponse.SendMessage(resultJson.ToString());
                }
            }
            else
            {
                if (UrlResponse.DefaultContentEncoding == "br")
                {
                    using BrotliStream brotliStream = new(Session.Response.Body, CompressionMode.Compress);
                    resultJson.Target.WriteTo(brotliStream);
                }
                else if (UrlResponse.DefaultContentEncoding == "gzip")
                {
                    using GZipStream gzipStream = new(Session.Response.Body, CompressionMode.Compress);
                    resultJson.Target.WriteTo(gzipStream);
                }
                else if (UrlResponse.DefaultContentEncoding == "deflate")
                {
                    using DeflateStream deflateStream = new(Session.Response.Body, CompressionMode.Compress);
                    resultJson.Target.WriteTo(deflateStream);
                }
                else
                {
                    resultJson.Target.WriteTo(Session.Response.Body);
                }
            }
        }
        return true;
    }
}