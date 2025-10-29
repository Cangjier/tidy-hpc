using TidyHPC.LiteJson;

namespace TidyHPC.Routers.Urls;

/// <summary>
/// Url 响应工厂
/// </summary>
public static class UrlResponseFactory
{
    /// <summary>
    /// 创建文本响应
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public static Responses.TextHtml CreateText(string content)
    {
        return new Responses.TextHtml(content);
    }

    /// <summary>
    /// 创建文本响应
    /// </summary>
    /// <param name="content"></param>
    /// <param name="contentEncoding"></param>
    /// <returns></returns>
    public static Responses.TextHtml CreateText(string content, string contentEncoding)
    {
        return new Responses.TextHtml(content, contentEncoding);
    }

    /// <summary>
    /// 创建 HTML 响应
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public static Responses.TextHtml CreateHtml(string content)
    {
        return new Responses.TextHtml(content);
    }

    /// <summary>
    /// 创建 HTML 响应
    /// </summary>
    /// <param name="content"></param>
    /// <param name="contentEncoding"></param>
    /// <returns></returns>
    public static Responses.TextHtml CreateHtml(string content, string contentEncoding)
    {
        return new Responses.TextHtml(content, contentEncoding);
    }

    /// <summary>
    /// 创建 JSON 响应
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public static Responses.ApplicationJson CreateApplicationJson(Json content)
    {
        return new Responses.ApplicationJson(content);
    }

    /// <summary>
    /// 创建 JSON 响应
    /// </summary>
    /// <param name="content"></param>
    /// <param name="contentEncoding"></param>
    /// <returns></returns>
    public static Responses.ApplicationJson CreateApplicationJson(Json content, string contentEncoding)
    {
        return new Responses.ApplicationJson(content, contentEncoding);
    }

    /// <summary>
    /// 创建附件响应
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="fileName"></param>
    /// <param name="relativeFilePath"></param>
    /// <returns></returns>
    public static Responses.Attachment CreateAttachment(string filePath, string fileName, string relativeFilePath)
    {
        return new Responses.Attachment(filePath, fileName, relativeFilePath);
    }

    /// <summary>
    /// 创建附件响应
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="fileName"></param>
    /// <param name="relativeFilePath"></param>
    /// <param name="contentEncoding"></param>
    /// <returns></returns>
    public static Responses.Attachment CreateAttachment(string filePath, string fileName, string relativeFilePath, string contentEncoding)
    {
        return new Responses.Attachment(filePath, fileName, relativeFilePath, contentEncoding);
    }

    /// <summary>
    /// 创建附件流响应
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static Responses.StreamAttachment CreateAttachment(Stream stream,string fileName)
    {
        return new Responses.StreamAttachment(stream, fileName, Responses.UrlResponse.DefaultContentEncoding);
    }

    /// <summary>
    /// 创建附件流响应
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="fileName"></param>
    /// <param name="contentEncoding"></param>
    /// <returns></returns>
    public static Responses.StreamAttachment CreateAttachment(Stream stream, string fileName, string contentEncoding)
    {
        return new Responses.StreamAttachment(stream, fileName, contentEncoding);
    }
}
