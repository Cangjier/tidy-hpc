using System.Net.Http.Headers;
using TidyHPC.Routers.Urls.Headers;

namespace TidyHPC.Routers.Urls.Interfaces;

/// <summary>
/// 响应头
/// </summary>
public interface IResponseHeaders : IEnumerable<KeyValuePair<string, string>>
{
    /// <summary>
    /// 获取头信息
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    string GetHeader(string key);

    /// <summary>
    /// 设置头信息
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    void SetHeader(string key, string value);

    /// <summary>
    /// Content-Type
    /// </summary>
    public ContentType? ContentType
    {
        get
        {
            string? contentType = GetHeader("Content-Type");
            if (contentType == null) return null;
            return ContentType.Parse(contentType);
        }
        set
        {
            if (value == null) return;
            SetHeader("Content-Type", value.ToString());
        }
    }

    /// <summary>
    /// Content-Disposition
    /// </summary>
    public string? ContentDisposition
    {
        get
        {
            return GetHeader("Content-Disposition");
        }
        set
        {
            if (value != null)
            {
                SetHeader("Content-Disposition", value);
            }
        }
    }

    /// <summary>
    /// Cache-Control
    /// </summary>
    public CacheControlHeaderValue? CacheControl
    {
        get
        {
            string? cacheControl = GetHeader("Cache-Control");
            if (cacheControl == null) return null;
            return CacheControlHeaderValue.Parse(cacheControl);
        }
        set
        {
            if (value == null) return;
            SetHeader("Cache-Control", value.ToString());
        }
    }

    /// <summary>
    /// Authorization
    /// </summary>
    public AuthenticationHeaderValue? Authorization
    {
        get
        {
            string? authorization = GetHeader("Authorization");
            if (authorization == null) return null;
            return AuthenticationHeaderValue.Parse(authorization);
        }

        set
        {
            if (value == null) return;
            SetHeader("Authorization", value.ToString());
        }
    }

    /// <summary>
    /// Content-Range
    /// </summary>
    public ContentRangeHeaderValue? ContentRange
    {
        get
        {
            string? contentRange = GetHeader("Content-Range");
            if (contentRange == null) return null;
            return ContentRangeHeaderValue.Parse(contentRange);
        }
        set
        {
            if (value == null) return;
            SetHeader("Content-Range", value.ToString());
        }
    }

    /// <summary>
    /// Content-Encoding
    /// </summary>
    public string ContentEncoding
    {
        get => GetHeader("Content-Encoding");
        set => SetHeader("Content-Encoding", value);
    }



}