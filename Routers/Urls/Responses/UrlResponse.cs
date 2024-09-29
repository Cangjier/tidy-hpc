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
/// <param name="FilePath"></param>
/// <param name="ContentType"></param>
/// <param name="ContentDisposition"></param>
/// <param name="ContentEncoding"></param>
/// <param name="FileEncoding"></param>
/// <param name="RelativeFilePath"></param>
public record BinaryFile(string FilePath,
    string ContentType, string? ContentDisposition, string? ContentEncoding,
    string? FileEncoding, string? RelativeFilePath) : UrlResponse;

/// <summary>
/// 文件响应，自动检测文件类型
/// </summary>
/// <param name="FilePath"></param>
/// <param name="RelativePath"></param>
public record DetectFile(string FilePath, string RelativePath)
    : BinaryFile(FilePath, Mime.DetectByFileExtension(Path.GetExtension(FilePath)), null, DefaultContentEncoding, null, RelativePath);

/// <summary>
/// Brotli 文件响应
/// </summary>
/// <param name="FilePath"></param>
/// <param name="ContentDisposition"></param>
/// <param name="RelativeFilePath"></param>
public record BrotliFile(string FilePath, string ContentDisposition, string RelativeFilePath)
    : BinaryFile(FilePath, Mime.DetectByFileExtension(Path.GetExtension(FilePath)), ContentDisposition, "br", "br", RelativeFilePath);

/// <summary>
/// 附件响应
/// </summary>
/// <param name="FilePath"></param>
/// <param name="FileName"></param>
/// <param name="RelativeFilePath"></param>
public record Attachment(string FilePath, string FileName, string RelativeFilePath)
    : BinaryFile(FilePath, Mime.DetectByFileExtension(Path.GetExtension(FilePath)), $"attachment; filename=\"{FileName}\"", DefaultContentEncoding, null, RelativeFilePath);

/// <summary>
/// Brotli 附件响应
/// </summary>
/// <param name="Path"></param>
/// <param name="FileName"></param>
public record BrotliAttachment(string Path, string FileName)
    : BrotliFile(Path, "application/octet-stream", $"attachment; filename=\"{FileName}\"");

/// <summary>
/// Html 文本响应
/// </summary>
/// <param name="Content"></param>
public record TextHtml(string Content) : UrlResponse;

/// <summary>
/// Json 响应
/// </summary>
/// <param name="Content"></param>
public record ApplicationJson(Json Content) : UrlResponse
{
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
/// <param name="Streams"></param>
/// <param name="ContentType"></param>
/// <param name="ContentDisposition"></param>
/// <param name="ContentEncoding"></param>
/// <param name="FileEncoding"></param>
/// <param name="CacheControl"></param>
public record MultiplyStreamFile(
    Stream[] Streams, string ContentType, string? ContentDisposition, 
    string? ContentEncoding,string? FileEncoding,CacheControlHeaderValue? CacheControl) : UrlResponse;

/// <summary>
/// 多流附件响应
/// </summary>
/// <param name="Streams"></param>
/// <param name="FileName"></param>
/// <param name="CacheControl"></param>
public record MultiplyStreamAttachment(Stream[] Streams, string FileName, CacheControlHeaderValue? CacheControl)
    : MultiplyStreamFile(Streams, Mime.DetectByFileExtension(Path.GetExtension(FileName)), $"attachment; filename=\"{FileName}\"", DefaultContentEncoding, null,CacheControl);

/// <summary>
/// 重定向
/// </summary>
public record Redirect : UrlResponse
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
public record ResponseStatusCode : UrlResponse
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
public record FilterResult: UrlResponse
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
    public static implicit operator FilterResult(bool status) => new(status ? UrlFilterStatus.Released : UrlFilterStatus.Rejected);
}

/// <summary>
/// Url Response
/// </summary>
public abstract record UrlResponse
{
    /// <summary>
    /// 默认内容编码
    /// </summary>
    public static string DefaultContentEncoding { get; set; } = "gzip";
}
