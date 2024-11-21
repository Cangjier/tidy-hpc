using System.Collections.Concurrent;
using TidyHPC.LiteDB.Layouts;
using TidyHPC.Locks;
using TidyHPC.Queues;

namespace TidyHPC.LiteDB.Caches;

/// <summary>
/// 所有缓存处理地
/// </summary>
public class Cache
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="provider"></param>
    public Cache(LayoutProvider provider)
    {
        Provider = provider;
        BytesCache = new(provider.Config.BytesCacheConfig);
    }

    /// <summary>
    /// 提供者
    /// </summary>
    public LayoutProvider Provider { get; }

    /// <summary>
    /// <para>InterfaceNames的缓存，可以参考</para>
    /// <para>1. 当注册时会加入该集合</para>
    /// <para>2. 当GetInterfaceNames时会加入该集合</para>
    /// </summary>
    public ConcurrentBag<string> CacheInterfaceNames { get; } = new();

    /// <summary>
    /// 字节缓存
    /// </summary>
    public BytesCache BytesCache { get; }

    /// <summary>
    /// IO缓存
    /// </summary>
    internal ByteArrayCache IOCache { get; } = new();

}
