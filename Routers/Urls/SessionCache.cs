using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using TidyHPC.LiteHttpServer;
using TidyHPC.LiteJson;
using TidyHPC.Routers.Urls.Responses;

namespace TidyHPC.Routers.Urls;
/// <summary>
/// Session 缓存
/// </summary>
/// <param name="session"></param>
public class SessionCache(Session session) : IDisposable
{
    private Session Session { get; } = session;

    /// <summary>
    /// 运行时的Url正则匹配组
    /// </summary>
    public ImmutableDictionary<string, string>? UrlRegexMatchGroups { get; private set; }

    /// <summary>
    /// 设置Url正则匹配组
    /// </summary>
    /// <param name="groupCollection"></param>
    public void TrySetUrlRegexMatchGroups(GroupCollection groupCollection)
    {
        if (UrlRegexMatchGroups != null) return;
        SetUrlRegexMatchGroups(groupCollection);
    }

    /// <summary>
    /// 设置Url正则匹配组
    /// </summary>
    /// <param name="groupCollection"></param>
    public void SetUrlRegexMatchGroups(GroupCollection groupCollection)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, string>();
        for (int i = 0; i < groupCollection.Count; i++)
        {
            builder.Add(groupCollection[i].Name, groupCollection[i].Value);
        }
        UrlRegexMatchGroups = builder.ToImmutable();
    }

    /// <summary>
    /// 设置Url正则匹配组
    /// </summary>
    /// <param name="urlRegexMatchGroups"></param>
    public void SetUrlRegexMatchGroups(ImmutableDictionary<string, string> urlRegexMatchGroups)
    {
        UrlRegexMatchGroups = urlRegexMatchGroups;
    }

    /// <summary>
    /// 尝试获取Url正则匹配组
    /// </summary>
    /// <param name="keys"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetUrlRegexMatchGroup(string[] keys, [NotNullWhen(true)] out string? value)
    {
        value = string.Empty;
        if (UrlRegexMatchGroups == null) return false;
        foreach (var key in keys)
        {
            if (UrlRegexMatchGroups.TryGetValue(key, out value))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 缓存的请求体Json
    /// </summary>
    private Json? CachedRequestBodyJson { get; set; } = null;

    /// <summary>
    /// 尝试获取请求体的Json
    /// </summary>
    /// <returns></returns>
    public async Task<Json> GetRequstBodyJson()
    {
        if (Session.Request.Headers.ContentType?.IsApplicationJson == true ||
            Session.Response is IWebsocketResponse)
        {
            if (!CachedRequestBodyJson.HasValue)
            {
                CachedRequestBodyJson = await Json.ParseAsync(Session.Request.Body);
            }
            return CachedRequestBodyJson.Value;
        }
        else
        {
            return Json.Null;
        }
    }

    /// <summary>
    /// 当前会话是否已经完成
    /// </summary>
    public bool Completed { get; set; }

    /// <summary>
    /// 过滤器状态
    /// </summary>
    public UrlFilterStatus FilterStatus { get; set; } = UrlFilterStatus.Released;

    /// <summary>
    /// 缓存数据，如过滤期间对权限校验时获取的用户信息
    /// </summary>
    public Dictionary<string, object?> Data { get; private set; } = new();

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        FilterStatus = UrlFilterStatus.Released;
        CachedRequestBodyJson = null;
        UrlRegexMatchGroups = null;
        Data?.Clear();
        Data = null!;
    }

    /// <summary>
    /// 重置资源
    /// </summary>
    public void Reset()
    {
        FilterStatus = UrlFilterStatus.Released;
        CachedRequestBodyJson = null;
        UrlRegexMatchGroups = null;
        Data?.Clear();
        Data = null!;
    }
}
