using System.Collections.Concurrent;

namespace TidyHPC.LiteDB.Caches;

/// <summary>
/// 缓存
/// </summary>
public class ByteArrayCache
{
    private ConcurrentDictionary<long, byte[]> Caches { get; } = new();

    /// <summary>
    /// 使用缓存
    /// </summary>
    /// <param name="address"></param>
    /// <param name="cacheSize"></param>
    /// <param name="cache"></param>
    /// <param name="isFirst"></param>
    /// <returns></returns>
    public void UseCache(long address, int cacheSize, out byte[] cache, out bool isFirst)
    {
        bool first = false;
        cache = Caches.GetOrAdd(address, (key) =>
        {
            first = true;
            return new byte[cacheSize];
        });
        isFirst = first;
    }
}
