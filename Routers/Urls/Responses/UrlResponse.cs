using System.Net.Http.Headers;
using TidyHPC.LiteJson;

namespace TidyHPC.Routers.Urls.Responses;

/// <summary>
/// Url 过滤器 状态
/// </summary>
public enum UrlFilterStatus
{
    /// <summary>
    /// 放行
    /// </summary>
    Released,

    /// <summary>
    /// 被拦截
    /// </summary>
    Rejected,
}

/// <summary>
/// 文件响应
/// </summary>
public class BinaryFile : UrlResponse
{
    /// <summary>
    /// 文件响应
    /// </summary>
    public BinaryFile()
    {
        FilePath = string.Empty;
        ContentType = "application/octet-stream";
        ContentDisposition = null;
        ContentEncoding = null;
        FileEncoding = null;
        RelativeFilePath = null;
        CacheControl = null;
    }

    /// <summary>
    /// 文件响应
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="contentType"></param>
    /// <param name="contentDisposition"></param>
    /// <param name="contentEncoding"></param>
    /// <param name="fileEncoding"></param>
    /// <param name="relativeFilePath"></param>
    /// <param name="cacheControl"></param>
    public BinaryFile(string filePath, string contentType, string? contentDisposition, string? contentEncoding,
        string? fileEncoding, string? relativeFilePath, CacheControlHeaderValue? cacheControl)
    {
        FilePath = filePath;
        ContentType = contentType;
        ContentDisposition = contentDisposition;
        ContentEncoding = contentEncoding;
        FileEncoding = fileEncoding;
        RelativeFilePath = relativeFilePath;
        CacheControl = cacheControl;
    }

    /// <summary>
    /// 文件路径
    /// </summary>
    public string FilePath { get; set; }

    /// <summary>
    /// 内容类型
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// 内容编码
    /// </summary>
    public string? ContentDisposition { get; set; }

    /// <summary>
    /// 内容编码
    /// </summary>
    public string? ContentEncoding { get; set; }

    /// <summary>
    /// 文件编码
    /// </summary>
    public string? FileEncoding { get; set; }

    /// <summary>
    /// 相对文件路径
    /// </summary>
    public string? RelativeFilePath { get; set; }

    /// <summary>
    /// 缓存控制
    /// </summary>
    public CacheControlHeaderValue? CacheControl { get; set; }

    /// <summary>
    /// 设置缓存控制，使用最大年龄
    /// </summary>
    /// <param name="maxAge"></param>
    public void SetCacheControlByMaxAge(int maxAge)
    {
        CacheControl = new CacheControlHeaderValue
        {
            MaxAge = TimeSpan.FromSeconds(maxAge)
        };
    }
}

/// <summary>
/// 文件响应，自动检测文件类型
/// </summary>
public class DetectFile : BinaryFile
{
    /// <summary>
    /// 文件响应，自动检测文件类型
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="relativePath"></param>
    public DetectFile(string filePath, string relativePath) : base(filePath,
        Mime.DetectByFileExtension(Path.GetExtension(filePath)), null, DefaultContentEncoding, null, relativePath, null)
    {
    }
}

/// <summary>
/// Brotli 文件响应
/// </summary>
public class BrotliFile : BinaryFile
{
    /// <summary>
    /// Brotli 文件响应
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="contentDisposition"></param>
    /// <param name="relativeFilePath"></param>
    public BrotliFile(string filePath, string contentDisposition, string relativeFilePath) : base(filePath,
        Mime.DetectByFileExtension(Path.GetExtension(filePath)), contentDisposition, "br", "br",
        relativeFilePath, null)
    {
        FilePath = filePath;
        ContentDisposition = contentDisposition;
        RelativeFilePath = relativeFilePath;
    }
}

/// <summary>
/// 附件响应
/// </summary>
public class Attachment : BinaryFile
{
    /// <summary>
    /// 附件响应
    /// </summary>
    public Attachment()
    {
    }

    /// <summary>
    /// 附件响应，使用文件路径创建
    /// </summary>
    /// <param name="filePath"></param>
    public Attachment(string filePath)
    {
        FilePath = filePath;
    }

    /// <summary>
    /// 附件响应
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="fileName"></param>
    /// <param name="relativeFilePath"></param>
    /// <param name="contentEncoding"></param>
    /// <param name="cacheControl"></param>
    public Attachment(string filePath, string fileName, string relativeFilePath, string contentEncoding,
        CacheControlHeaderValue? cacheControl) : base(filePath, Mime.DetectByFileExtension(Path.GetExtension(filePath)),
        $"attachment; filename=\"{fileName}\"", contentEncoding, null, relativeFilePath, cacheControl)
    {
    }

    /// <summary>
    /// 附件响应，使用默认内容编码
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="fileName"></param>
    /// <param name="relativeFilePath"></param>
    public Attachment(string filePath, string fileName, string relativeFilePath)
        : this(filePath, fileName, relativeFilePath, DefaultContentEncoding, null)
    {
    }

    /// <summary>
    /// 附件响应，使用默认内容编码
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="fileName"></param>
    /// <param name="relativeFilePath"></param>
    /// <param name="cacheControl"></param>
    public Attachment(string filePath, string fileName, string relativeFilePath, CacheControlHeaderValue? cacheControl)
        : this(filePath, fileName, relativeFilePath, DefaultContentEncoding, cacheControl)
    {
    }
}

/// <summary>
/// 附件流响应
/// </summary>
public class StreamAttachment : MultiplyStreamAttachment
{
    /// <summary>
    /// 附件流响应
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="fileName"></param>
    /// <param name="contentEncoding"></param>
    public StreamAttachment(Stream stream, string fileName, string contentEncoding) : base([stream], fileName,
        contentEncoding, null)
    {
    }
}

/// <summary>
/// Brotli 附件响应
/// </summary>
public class BrotliAttachment : BrotliFile
{
    /// <summary>
    /// Brotli 附件响应
    /// </summary>
    /// <param name="path"></param>
    /// <param name="fileName"></param>
    public BrotliAttachment(string path, string fileName) : base(path, "application/octet-stream",
        $"attachment; filename=\"{fileName}\"")
    {
    }
}

/// <summary>
/// Html 文本响应
/// </summary>
public class TextHtml : UrlResponse
{
    /// <summary>
    /// 内容
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// 内容编码
    /// </summary>
    public string ContentEncoding { get; set; }

    /// <summary>
    /// Html 文本响应
    /// </summary>
    /// <param name="content"></param>
    /// <param name="contentEncoding"></param>
    public TextHtml(string content, string contentEncoding)
    {
        Content = content;
        ContentEncoding = contentEncoding;
    }

    /// <summary>
    /// Html 文本响应，使用默认内容编码
    /// </summary>
    /// <param name="content"></param>
    public TextHtml(string content) : this(content, UrlResponse.DefaultContentEncoding)
    {
    }
}

/// <summary>
/// Json 响应
/// </summary>
public class ApplicationJson : UrlResponse
{
    /// <summary>
    /// 内容
    /// </summary>
    public Json Content { get; set; }

    /// <summary>
    /// 内容编码
    /// </summary>
    public string ContentEncoding { get; set; }

    /// <summary>
    /// Json 响应
    /// </summary>
    /// <param name="content"></param>
    /// <param name="contentEncoding"></param>
    public ApplicationJson(Json content, string contentEncoding)
    {
        Content = content;
        ContentEncoding = contentEncoding;
    }

    /// <summary>
    /// Json 响应，使用默认内容编码
    /// </summary>
    /// <param name="content"></param>
    public ApplicationJson(Json content) : this(content, UrlResponse.DefaultContentEncoding)
    {
    }

    /// <summary>
    /// Implicit conversion from Json to ApplicationJson
    /// </summary>
    /// <param name="content"></param>
    public static implicit operator ApplicationJson(Json content) => new(content);

    /// <summary>
    /// Implicit conversion from NetMessageInterface to ApplicationJson
    /// </summary>
    /// <param name="content"></param>
    public static implicit operator ApplicationJson(NetMessageInterface content) => new(content);
}

/// <summary>
/// 多流响应，按顺序写入响应流
/// </summary>
public class MultiplyStreamFile : UrlResponse
{
    /// <summary>
    /// 流列表
    /// </summary>
    public Stream[] Streams { get; }

    /// <summary>
    /// 内容类型
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// 内容编码
    /// </summary>
    public string? ContentDisposition { get; set; }

    /// <summary>
    /// 文件编码
    /// </summary>
    public string? ContentEncoding { get; set; }

    /// <summary>
    /// 缓存控制
    /// </summary>
    public string? FileEncoding { get; set; }

    /// <summary>
    /// 缓存控制
    /// </summary>
    public CacheControlHeaderValue? CacheControl { get; set; }

    /// <summary>
    /// 多流响应，使用默认内容编码
    /// </summary>
    public MultiplyStreamFile()
    {
        Streams = [];
        ContentType = "application/octet-stream";
        ContentDisposition = null;
        ContentEncoding = null;
        FileEncoding = null;
        CacheControl = null;
    }

    /// <summary>
    /// 多流响应
    /// </summary>
    /// <param name="streams"></param>
    /// <param name="contentType"></param>
    /// <param name="contentDisposition"></param>
    /// <param name="contentEncoding"></param>
    /// <param name="fileEncoding"></param>
    /// <param name="cacheControl"></param>
    public MultiplyStreamFile(Stream[] streams, string contentType, string? contentDisposition, string? contentEncoding,
        string? fileEncoding, CacheControlHeaderValue? cacheControl)
    {
        this.Streams = streams;
        this.ContentType = contentType;
        this.ContentDisposition = contentDisposition;
        this.ContentEncoding = contentEncoding;
        this.FileEncoding = fileEncoding;
        this.CacheControl = cacheControl;
    }
}

/// <summary>
/// 多流附件响应
/// </summary>
public class MultiplyStreamAttachment : MultiplyStreamFile
{
    /// <summary>
    /// 多流附件响应
    /// </summary>
    /// <param name="streams"></param>
    /// <param name="fileName"></param>
    /// <param name="contentEncoding"></param>
    /// <param name="cacheControl"></param>
    public MultiplyStreamAttachment(Stream[] streams, string fileName, string contentEncoding,
        CacheControlHeaderValue? cacheControl) : base(streams, Mime.DetectByFileExtension(Path.GetExtension(fileName)),
        $"attachment; filename=\"{fileName}\"", contentEncoding, null, cacheControl)
    {
    }

    /// <summary>
    /// 多流附件响应，使用默认内容编码
    /// </summary>
    /// <param name="streams"></param>
    /// <param name="fileName"></param>
    /// <param name="cacheControl"></param>
    public MultiplyStreamAttachment(Stream[] streams, string fileName, CacheControlHeaderValue? cacheControl) : this(
        streams, fileName, UrlResponse.DefaultContentEncoding, cacheControl)
    {
    }
}

/// <summary>
/// 重定向
/// </summary>
public class Redirect : UrlResponse
{
    /// <summary>
    /// 重定向地址
    /// </summary>
    public string Url { get; }

    /// <summary>
    /// 重定向
    /// </summary>
    /// <param name="url"></param>
    public Redirect(string url)
    {
        Url = url;
    }
}

/// <summary>
/// Url 过滤状态码
/// </summary>
public class ResponseStatusCode : UrlResponse
{
    /// <summary>
    /// 状态码
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Url 过滤状态码
    /// </summary>
    /// <param name="statusCode"></param>
    public ResponseStatusCode(int statusCode)
    {
        StatusCode = statusCode;
    }
}

/// <summary>
/// 过滤结果
/// </summary>
public class FilterResult : UrlResponse
{
    /// <summary>
    /// 过滤状态
    /// </summary>
    public UrlFilterStatus Status { get; }

    /// <summary>
    /// 过滤结果
    /// </summary>
    /// <param name="status"></param>
    public FilterResult(UrlFilterStatus status)
    {
        Status = status;
    }

    /// <summary>
    /// Implicit conversion from UrlFilterStatus to FilterResult
    /// </summary>
    /// <param name="status"></param>
    public static implicit operator FilterResult(UrlFilterStatus status) => new(status);

    /// <summary>
    /// Implicit conversion from bool to FilterResult
    /// </summary>
    /// <param name="status"></param>
    public static implicit operator FilterResult(bool status) =>
        new(status ? UrlFilterStatus.Released : UrlFilterStatus.Rejected);
}

/// <summary>
/// 没有响应
/// </summary>
public class NoneResponse : UrlResponse;

/// <summary>
/// Url Response
/// </summary>
public abstract class UrlResponse
{
    /// <summary>
    /// 默认内容编码
    /// </summary>
    public static string DefaultContentEncoding { get; set; } = "gzip";
}
